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


        private Result<DataResponse>[] ReadNodes(Session session, OpcUAQuery[] queries, NamespaceTable namespaceTable)
        {
            var results = new Result<DataResponse>[queries.Length];
            
            //var expandedNodeIds = queries.Select(a => ExpandedNodeId.Parse(a.nodeId)).ToArray();

            //namespaceTable.GetIndex()

            var nodeIds = queries.Select(a => Converter.GetNodeId(a.nodeId, namespaceTable)).ToArray();
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

        private Result<DataResponse>[] ReadHistoryRaw(Session session, OpcUAQuery[] queries, NamespaceTable namespaceTable)
        {
            var indexMap = new Dictionary<ReadRawKey, List<int>>();
            var queryMap = new Dictionary<ReadRawKey, List<NodeId>>();
            for (int i = 0; i < queries.Length; i++)
            {
                var query = queries[i];
                var maxValues = query.maxDataPoints;
                var tr = query.timeRange;
                var nodeId = Converter.GetNodeId(query.nodeId, namespaceTable);
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

        private Result<DataResponse>[] ReadHistoryProcessed(Session session, OpcUAQuery[] queries, NamespaceTable namespaceTable)
        {
            var indexMap = new Dictionary<ReadProcessedKey, List<int>>();
            var queryMap = new Dictionary<ReadProcessedKey, List<NodeId>>();
            for (int i = 0; i < queries.Length; i++)
            {
                var query = queries[i];
                var resampleInterval = query.intervalMs;
                var tr = query.timeRange;
                var nodeId = Converter.GetNodeId(query.nodeId, namespaceTable);
                OpcUaNodeDefinition aggregate = JsonSerializer.Deserialize<OpcUaNodeDefinition>(query.aggregate.ToString());
                var aggregateNodeId = Converter.GetNodeId(aggregate.nodeId, namespaceTable);
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

        private LiteralOperand GetLiteralOperand(LiteralOp literop, NamespaceTable namespaceTable)
        {
            var nodeId = Converter.GetNodeId(literop.typeId, namespaceTable);
            if (nodeId.NamespaceIndex == 0 && nodeId.IdType == IdType.Numeric)
            {
                var id = Convert.ToInt32(nodeId.Identifier);
                if (id == 17)  // NodeId: TODO use constant.
                {
                    var nodeIdVal = Converter.GetNodeId(literop.value, namespaceTable);
                    return new LiteralOperand(nodeIdVal);
                }
            }
            return new LiteralOperand(literop.value);
        }

        private SimpleAttributeOperand GetSimpleAttributeOperand(SimpleAttributeOp literop, NamespaceTable namespaceTable)
        {
            NodeId typeId = null;
            if (!string.IsNullOrWhiteSpace(literop.typeId))
            {
                typeId = Converter.GetNodeId(literop.typeId, namespaceTable);
            }
            return new SimpleAttributeOperand(typeId, literop.browsePath.Select(a => Converter.GetQualifiedName(a, namespaceTable)).ToList());
        }


        private object GetOperand(FilterOperand operand, NamespaceTable namespaceTable)
        {
            JsonElement el = (JsonElement)operand.value;
            switch (operand.type)
            {
                case FilterOperandEnum.Literal:
                    
                    //return GetLiteralOperand(JsonSerializer.Deserialize<LiteralOp>(operand.value), namespaceTable);
                    return GetLiteralOperand(el.ToObject<LiteralOp>(), namespaceTable);
                case FilterOperandEnum.Element:
                    {
                        //var elementOp = JsonSerializer.Deserialize<ElementOp>(operand.value);
                        var elementOp = el.ToObject<ElementOp>();
                        return new ElementOperand(elementOp.index);
                    }
                case FilterOperandEnum.SimpleAttribute:
                    return GetSimpleAttributeOperand(el.ToObject<SimpleAttributeOp>(), namespaceTable);
                //return GetSimpleAttributeOperand(JsonSerializer.Deserialize<SimpleAttributeOp>(operand.value), namespaceTable);
                default:
                    throw new ArgumentException();
            }
        }

        private object[] GetOperands(EventFilter f, NamespaceTable namespaceTable)
        {
            var operands = new object[f.operands.Length];
            for (int i = 0; i < f.operands.Length; i++)
                operands[i] = GetOperand(f.operands[i], namespaceTable);
            return operands;
        
        }

        private Opc.Ua.EventFilter GetEventFilter(OpcUAQuery query, NamespaceTable namespaceTable)
        {
            var eventFilter = new Opc.Ua.EventFilter();
            foreach (var column in query.eventQuery?.eventColumns)
            {
                var nsIdx = string.IsNullOrWhiteSpace(column.browsename.namespaceUrl) ? 0 : namespaceTable.GetIndex(column.browsename.namespaceUrl);
                eventFilter.AddSelectClause(ObjectTypes.BaseEventType, new Opc.Ua.QualifiedName(column.browsename.name, (ushort)nsIdx));
            }


            if (query.eventQuery?.eventFilters != null)
            {
                for (int i = 0; i < query.eventQuery.eventFilters.Length; i++ )
                {
                    var filter = query.eventQuery.eventFilters[i];
                    eventFilter.WhereClause.Push(filter.oper, GetOperands(filter, namespaceTable));
                }
            }


            //if (query.eventQuery?.eventTypeNodeId != null)
            //    eventFilter.WhereClause.Push(FilterOperator.OfType, Converter.GetNodeId(query.eventQuery?.eventTypeNodeId, namespaceTable));

            //uint rootIdx = 0;
            //foreach (var f in query.eventQuery.eventFilters)
            //{
            //    switch (f.oper)
            //    {
            //        case FilterOperator.GreaterThan:
            //        case FilterOperator.GreaterThanOrEqual:
            //        case FilterOperator.LessThan:
            //        case FilterOperator.LessThanOrEqual:
            //        case FilterOperator.Equals:
            //            {
            //                var attr = new SimpleAttributeOperand(new NodeId(ObjectTypes.BaseEventType), f.operands[0]);
            //                // TODO: Must use correct type for field.
            //                var lit = new LiteralOperand(f.operands[1]);
            //                eventFilter.WhereClause.Push(f.oper, attr, lit);
            //                eventFilter.WhereClause.Push(FilterOperator.And, new ElementOperand(rootIdx), new ElementOperand((uint)eventFilter.WhereClause.Elements.Count - 1));
            //                rootIdx = (uint)eventFilter.WhereClause.Elements.Count - 1;
            //            }
            //            break;
            //    }
            //}

            return eventFilter;
        }

        private Dictionary<int, Field> AddEventFields(DataFrame dataFrame, OpcUAQuery query)
        {
            var fields = new Dictionary<int, Field>();
            for (int i = 0; i < query.eventQuery.eventColumns.Length; i++)
            {
                var col = query.eventQuery.eventColumns[i];
                fields.Add(i, dataFrame.AddField<string>(string.IsNullOrEmpty(col.alias) ? col.browsename.name : col.alias));
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


        private Result<DataResponse>[] ReadEvents(Session session, OpcUAQuery[] opcUAQuery, NamespaceTable namespaceTable)
        {
            var results = new Result<DataResponse>[opcUAQuery.Length];
            // Do one by one for now. unsure of use-case with multiple node ids for same filter.
            for (int i = 0; i < opcUAQuery.Length; i++)
            {
                var query = opcUAQuery[i];
                var tr = query.timeRange;
                var nodeId = Converter.GetNodeId(query.nodeId, namespaceTable);
                DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.FromEpochMS).UtcDateTime;
                DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.ToEpochMS).UtcDateTime;
                var eventFilter = GetEventFilter(query, namespaceTable);
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
                var nsTable = connection.NamespaceUris;
                foreach (var queryGroup in queryGroups)
                {
                    var queries = queryGroup.ToArray();
                    try
                    {
                        Result<DataResponse>[] responses = null;
                        switch (queryGroup.Key)
                        {
                            case "ReadNode":
                                responses = ReadNodes(connection, queries, nsTable);
                                break;
                            case "Subscribe":
                                // TODO:
                                // connection.AddSubscription(query.refId, query.nodeId, SubscriptionCallback);
                                break;
                            case "ReadDataRaw":
                                responses = ReadHistoryRaw(connection, queries, nsTable);
                                break;
                            case "ReadDataProcessed":
                                responses = ReadHistoryProcessed(connection, queries, nsTable);
                                break;
                            case "ReadEvents":
                                responses = ReadEvents(connection, queries, nsTable);
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
