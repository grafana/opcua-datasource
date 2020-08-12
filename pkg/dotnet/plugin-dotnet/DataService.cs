using System;
using System.Collections.Generic;
using System.Text;
using Pluginv2;
using Grpc.Core;
using Grpc.Core.Logging;
using Google.Protobuf;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using System.Globalization;
using Apache.Arrow;
using Apache.Arrow.Ipc;
using Apache.Arrow.Memory;
using System.Linq;
using Microsoft.Data.Analysis;
using System.IO;
using Prediktor.UA.Client;
//using MicrosoftOpcUa.Client.Core;

namespace plugin_dotnet
{
    class OpcUaNodeDefinition
    {
        public string name { get; set; }
        public string nodeId { get; set; }
    }

    class DataService : Data.DataBase
    {
        private IConnections _connections;
        private readonly ILogger log;
        private Alias alias;

        public DataService(ILogger logIn, IConnections connections)
        {
            _connections = connections;
            log = logIn;
            log.Info("We are using the datasource");
            alias = new Alias();
        }

        private Result<DataResponse>[] ReadNodes(Session session, OpcUAQuery[] queries)
        {
            var results = new Result<DataResponse>[queries.Length];
            var nodeIds = queries.Select(a => NodeId.Parse(a.nodeId)).ToArray();
            var dvs = session.ReadNodeValues(nodeIds);
            for (int i = 0; i < dvs.Length; i++)
            {
                var dataValue = dvs[i];
                if (Opc.Ua.StatusCode.IsGood(dataValue.StatusCode))
                {
                    DataResponse dataResponse = new DataResponse();
                    DataFrame dataFrame = new DataFrame(queries[i].refId);

                    var timeField = dataFrame.AddField("Time", typeof(DateTime));
                    Field valueField = dataFrame.AddField(String.Join(" / ", queries[i].value), dataValue.Value.GetType());
                    timeField.Append(dataValue.SourceTimestamp);
                    valueField.Append(dataValue.Value);
                    dataResponse.Frames.Add(dataFrame.ToGprcArrowFrame());
                    results[i] = new Result<DataResponse>(dataResponse);
                }
                else 
                {
                    results[i] = new Result<DataResponse>(dataValue.StatusCode, string.Format("Error reading node with id {0}", nodeIds[i].ToString()));
                }
            }
            return results;
        }

        // TODO: move to extension methods
        private static void AddDict<U, T>(Dictionary<U, List<T>> dict, U key, T val)
        {
            List<T> values;
            if (dict.TryGetValue(key, out values))
            {
                values.Add(val);
            }
            else
            {
                values = new List<T>();
                values.Add(val);
                dict.Add(key, values);
            }
        }

        private Result<DataResponse>[] ReadHistoryRaw(Session session, OpcUAQuery[] queries)
        {
            var indexMap = new Dictionary<ReadRawKey, List<int>>();
            var queryMap = new Dictionary<ReadRawKey, List<NodeId>>();
            for (int i = 0; i < queries.Length; i++)
            {
                var query = queries[i];
                var maxValues = query.maxDataPoints;
                var tr = query.timeRange;
                var nodeId = NodeId.Parse(query.nodeId);
                DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.FromEpochMS).UtcDateTime;
                DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.ToEpochMS).UtcDateTime;
                var key = new ReadRawKey(fromTime, toTime, Convert.ToInt32(maxValues));
                AddDict(indexMap, key, i);
                AddDict(queryMap, key, nodeId);
            }

            var result = new Result<DataResponse>[queries.Length];
            foreach (var querygroup in queryMap)
            {
                var key = querygroup.Key;
                var nodes = querygroup.Value.ToArray();
                var historyValues = session.ReadHistoryRaw(key.StartTime, key.EndTime, key.MaxValues, nodes);
                var indices = indexMap[key];
                for (int i = 0; i < indices.Count; i++)
                {
                    var idx = indices[i];
                    result[idx] = CreateHistoryDataResponse(historyValues[i], queries[idx]);
                }
            }
            return result;
        }

        private Result<DataResponse> CreateHistoryDataResponse(Result<HistoryData> valuesResult, OpcUAQuery query)
        {
            if (valuesResult.Success)
            {
                var dataResponse = new DataResponse();
                var dataFrame = new DataFrame(query.refId);
                var timeField = dataFrame.AddField("Time", typeof(DateTime));
                Field valueField = null;
                foreach (DataValue entry in valuesResult.Value.DataValues)
                {
                    if (valueField == null && entry.Value != null)
                    {
                        valueField = dataFrame.AddField(String.Join(" / ", query.value), entry.Value.GetType());
                    }

                    if (valueField != null)
                    {
                        valueField.Append(entry.Value);
                        timeField.Append(entry.SourceTimestamp);
                    }
                }
                dataResponse.Frames.Add(dataFrame.ToGprcArrowFrame());
                return new Result<DataResponse>(dataResponse);
            }
            else
            {
                return new Result<DataResponse>(valuesResult.StatusCode, valuesResult.Error);
            }
        }

        private Result<DataResponse>[] ReadHistoryProcessed(Session session, OpcUAQuery[] queries)
        {
            var indexMap = new Dictionary<ReadProcessedKey, List<int>>();
            var queryMap = new Dictionary<ReadProcessedKey, List<NodeId>>();
            for (int i = 0; i < queries.Length; i++)
            {
                var query = queries[i];
                var resampleInterval = query.intervalMs;
                var tr = query.timeRange;
                var nodeId = NodeId.Parse(query.nodeId);
                OpcUaNodeDefinition aggregate = JsonSerializer.Deserialize<OpcUaNodeDefinition>(query.aggregate.ToString());
                var aggregateNodeId = NodeId.Parse(aggregate.nodeId);
                DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.FromEpochMS).UtcDateTime;
                DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.ToEpochMS).UtcDateTime;
                var key = new ReadProcessedKey(fromTime, toTime, aggregateNodeId, resampleInterval);
                AddDict(indexMap, key, i);
                AddDict(queryMap, key, nodeId);
            }

            var result = new Result<DataResponse>[queries.Length];
            foreach (var querygroup in queryMap)
            {
                var key = querygroup.Key;
                var nodes = querygroup.Value.ToArray();
                var historyValues = session.ReadHistoryProcessed(key.StartTime, key.EndTime, key.Aggregate, key.ResampleInterval, nodes);
                var indices = indexMap[key];
                for (int i = 0; i < indices.Count; i++)
                {
                    var idx = indices[i];
                    var valuesResult = historyValues[i];
                    result[idx] = CreateHistoryDataResponse(historyValues[i], queries[idx]);
                }
            }
            return result;
        }

        private Opc.Ua.EventFilter GetEventFilter(OpcUAQuery query)
        {
            var eventFilter = new Opc.Ua.EventFilter();
            foreach (var column in query.eventQuery?.eventColumns)
            {
                eventFilter.AddSelectClause(ObjectTypes.BaseEventType, column.browseName);
            }

            if (!string.IsNullOrEmpty(query.eventQuery?.eventTypeNodeId))
                eventFilter.WhereClause.Push(FilterOperator.OfType, NodeId.Parse(query.eventQuery?.eventTypeNodeId));

            uint rootIdx = 0;
            foreach (var f in query.eventQuery.eventFilters)
            {
                switch (f.oper)
                {
                    case FilterOperator.GreaterThan:
                    case FilterOperator.GreaterThanOrEqual:
                    case FilterOperator.LessThan:
                    case FilterOperator.LessThanOrEqual:
                    case FilterOperator.Equals:
                        {
                            var attr = new SimpleAttributeOperand(new NodeId(ObjectTypes.BaseEventType), f.operands[0]);
                            // TODO: Must use correct type for field.
                            var lit = new LiteralOperand(f.operands[1]);
                            eventFilter.WhereClause.Push(f.oper, attr, lit);
                            eventFilter.WhereClause.Push(FilterOperator.And, new ElementOperand(rootIdx), new ElementOperand((uint)eventFilter.WhereClause.Elements.Count - 1));
                            rootIdx = (uint)eventFilter.WhereClause.Elements.Count - 1;
                        }
                        break;
                }
            }

            return eventFilter;
        }

        private Dictionary<int, Field> AddEventFields(DataFrame dataFrame, OpcUAQuery query)
        {
            var fields = new Dictionary<int, Field>();
            for (int i = 0; i < query.eventQuery.eventColumns.Length; i++)
            {
                var col = query.eventQuery.eventColumns[i];
                fields.Add(i, dataFrame.AddField<string>(string.IsNullOrEmpty(col.alias) ? col.browseName : col.alias));
            }
            return fields;
        }

        private Result<DataResponse> CreateEventDataResponse(Result<HistoryEvent> historyEventResult, OpcUAQuery query)
        {
            if (historyEventResult.Success)
            {
                var historyEvent = historyEventResult.Value;
                var dataResponse = new DataResponse();
                var dataFrame = new DataFrame(query.refId);
                if (historyEvent.Events.Count > 0)
                {
                    var fields = AddEventFields(dataFrame, query);
                    foreach (var e in historyEvent.Events)
                    {
                        for (int k = 0; k < e.EventFields.Count; k++)
                        {
                            var field = e.EventFields[k];
                            if (fields.TryGetValue(k, out Field dataField))
                            {
                                
                                if (field.Value != null)
                                {
                                    if (dataField.Type.Equals(field.Value.GetType()))
                                        dataField.Append(field.Value);
                                    else
                                        dataField.Append(field.Value.ToString());
                                }
                                else
                                {
                                    if (dataField.Type.IsValueType)
                                        dataField.Append(Activator.CreateInstance(dataField.Type));
                                    else if (dataField.Type.Equals(typeof(string)))
                                        dataField.Append(string.Empty);
                                    else
                                        dataField.Append(null);
                                }
                            }
                        }
                    }
                }
                dataResponse.Frames.Add(dataFrame.ToGprcArrowFrame());
                return new Result<DataResponse>(dataResponse);
            }
            else
                return new Result<DataResponse>(historyEventResult.StatusCode, historyEventResult.Error);
        }


        private Result<DataResponse>[] ReadEvents(Session session, OpcUAQuery[] opcUAQuery)
        {
            var results = new Result<DataResponse>[opcUAQuery.Length];
            // Do one by one for now. unsure of use-case with multiple node ids for same filter.
            for (int i = 0; i < opcUAQuery.Length; i++)
            {
                var query = opcUAQuery[i];
                var tr = query.timeRange;
                var nodeId = NodeId.Parse(query.nodeId);
                DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.FromEpochMS).UtcDateTime;
                DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.ToEpochMS).UtcDateTime;
                var eventFilter = GetEventFilter(query);
                var response = session.ReadEvents(fromTime, toTime, uint.MaxValue, eventFilter, new[] { nodeId });
                results[i] = CreateEventDataResponse(response[0], query);
            }
            return results;
        }



        public override async Task<QueryDataResponse> QueryData(QueryDataRequest request, ServerCallContext context)
        {
            QueryDataResponse response = new QueryDataResponse();
            Session connection = null;

            try
            {
                log.Debug("got a request: {0}", request);
                connection = _connections.Get(request.PluginContext.DataSourceInstanceSettings);

                var queryGroups = request.Queries.Select(q => new OpcUAQuery(q)).ToLookup(o => o.readType);

                foreach (var queryGroup in queryGroups)
                {
                    var queries = queryGroup.ToArray();
                    try
                    {
                        Result<DataResponse>[] responses = null;
                        switch (queryGroup.Key)
                        {
                            case "ReadNode":
                                responses = ReadNodes(connection, queries);
                                break;
                            case "Subscribe":
                                // TODO:
                                // connection.AddSubscription(query.refId, query.nodeId, SubscriptionCallback);
                                break;
                            case "ReadDataRaw":
                                responses = ReadHistoryRaw(connection, queries);
                                break;
                            case "ReadDataProcessed":
                                responses = ReadHistoryProcessed(connection, queries);
                                break;
                            case "ReadEvents":
                                responses = ReadEvents(connection, queries);
                                break;

                        }
                        if (responses != null)
                        {
                            int i = 0;
                            foreach (var dataResponse in responses)
                            {
                                if (dataResponse.Success)
                                    response.Responses[queries[i++].refId] = dataResponse.Value;
                                else
                                    response.Responses[queries[i++].refId].Error = dataResponse.Error;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        foreach (var q in queries)
                        {
                            response.Responses[q.refId].Error = e.ToString();
                        }
                        log.Error(e.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
               // Close out the client connection.
               log.Debug("Error: {0}", ex);
               connection.Close();
            }

            return await Task.FromResult(response);
        }

        private void SubscriptionCallback(string refId, MonitoredItem item, MonitoredItemNotificationEventArgs eventArgs)
        {
            QueryDataResponse response = new QueryDataResponse();
            log.Debug("Got a callback {0} - {1} - {2}", refId, item, eventArgs);
            
        }
    }
}
