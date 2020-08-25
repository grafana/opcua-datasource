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
                    result[idx] = Converter.CreateHistoryDataResponse(historyValues[i], queries[idx]);
                }
            }
            return result;
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
                    result[idx] = Converter.CreateHistoryDataResponse(historyValues[i], queries[idx]);
                }
            }
            return result;
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
                var eventFilter = Converter.GetEventFilter(query, namespaceTable);
                var response = session.ReadEvents(fromTime, toTime, uint.MaxValue, eventFilter, new[] { nodeId });
                results[i] = Converter.CreateEventDataResponse(response[0], query);
            }
            return results;
        }



        public override async Task<QueryDataResponse> QueryData(QueryDataRequest request, ServerCallContext context)
        {
            QueryDataResponse response = new QueryDataResponse();
            IConnection connection = null;

            try
            {
                log.Debug("got a request: {0}", request);
                connection = _connections.Get(request.PluginContext.DataSourceInstanceSettings);

                var queryGroups = request.Queries.Select(q => new OpcUAQuery(q)).ToLookup(o => o.readType);
                var nsTable = connection.Session.NamespaceUris;
                foreach (var queryGroup in queryGroups)
                {
                    var queries = queryGroup.ToArray();
                    try
                    {
                        Result<DataResponse>[] responses = null;
                        switch (queryGroup.Key)
                        {
                            case "ReadNode":
                                responses = ReadNodes(connection.Session, queries, nsTable);
                                break;
                            case "Subscribe":
                                // TODO:
                                // connection.AddSubscription(query.refId, query.nodeId, SubscriptionCallback);
                                break;
                            case "ReadDataRaw":
                                responses = ReadHistoryRaw(connection.Session, queries, nsTable);
                                break;
                            case "ReadDataProcessed":
                                responses = ReadHistoryProcessed(connection.Session, queries, nsTable);
                                break;
                            case "ReadEvents":
                                responses = ReadEvents(connection.Session, queries, nsTable);
                                break;
                            case "SubscribeEvents":
                                responses = SubscribeEvents(connection.EventSubscription, queries, nsTable);
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

        private Result<DataResponse>[] SubscribeEvents(IEventSubscription eventSubscription, OpcUAQuery[] queries, NamespaceTable nsTable)
        {
            var responses = new Result<DataResponse>[queries.Length];
            for (int i = 0; i < queries.Length; i++)
            {
                try
                {
                    if (queries[i].eventQuery != null)
                    {
                        var eventFilter = Converter.GetEventFilter(queries[i], nsTable);
                        var nodeId = Converter.GetNodeId(queries[i].nodeId, nsTable);
                        responses[i] = eventSubscription.GetEventData(queries[i], nodeId, eventFilter);
                    }
                    else
                        responses[i] = new Result<DataResponse>(Opc.Ua.StatusCodes.BadUnknownResponse, "Event query null");
                }
                catch (Exception e)
                {
                    responses[i] = new Result<DataResponse>(Opc.Ua.StatusCodes.BadUnknownResponse, e.ToString());
                }
            }
            return responses;
        }

        private void SubscriptionCallback(string refId, MonitoredItem item, MonitoredItemNotificationEventArgs eventArgs)
        {
            QueryDataResponse response = new QueryDataResponse();
            log.Debug("Got a callback {0} - {1} - {2}", refId, item, eventArgs);
            
        }
    }
}
