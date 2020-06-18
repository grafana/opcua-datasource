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
                    Field timeField = dataFrame.AddField("Time", dataValue.SourceTimestamp.GetType());
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
                    var valuesResult = historyValues[i];
                    if (valuesResult.Success)
                    {
                        var dataResponse = new DataResponse();
                        var dataFrame = new DataFrame(queries[idx].refId);
                        Field timeField = dataFrame.AddField<DateTime>("Time");
                        Field valueField = null;
                        foreach (DataValue entry in valuesResult.Value.DataValues)
                        {
                            if (valueField == null && entry.Value != null)
                            {
                                valueField = dataFrame.AddField(String.Join(" / ", queries[idx].value), entry.Value.GetType());
                            }

                            if (valueField != null)
                            {
                                valueField.Append(entry.Value);
                                timeField.Append(entry.SourceTimestamp);
                            }
                        }
                        dataResponse.Frames.Add(dataFrame.ToGprcArrowFrame());
                        result[idx] = new Result<DataResponse>(dataResponse);
                    }
                    else
                    {
                        result[idx] = new Result<DataResponse>(valuesResult.StatusCode, valuesResult.Error);
                    }
                }
            }
            return result;
        }

        private Result<DataResponse>[] ReadHistoryProcessed(Session session, OpcUAQuery[] queries)
        {
            var indexMap = new Dictionary<ReadProcessedKey, List<int>>();
            var queryMap = new Dictionary<ReadProcessedKey, List<NodeId>>();
            for (int i = 0; i < queries.Length; i++)
            {
                var query = queries[i];
                var resampleInterval = query.intervalMs / 1000.0;
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
                    if (valuesResult.Success)
                    {
                        var dataResponse = new DataResponse();
                        var dataFrame = new DataFrame(queries[idx].refId);
                        Field timeField = dataFrame.AddField<DateTime>("Time");
                        Field valueField = null;
                        foreach (DataValue entry in valuesResult.Value.DataValues)
                        {
                            if (valueField == null && entry.Value != null)
                            {
                                valueField = dataFrame.AddField(String.Join(" / ", queries[idx].value), entry.Value.GetType());
                            }

                            if (valueField != null)
                            {
                                valueField.Append(entry.Value);
                                timeField.Append(entry.SourceTimestamp);
                            }
                        }
                        dataResponse.Frames.Add(dataFrame.ToGprcArrowFrame());
                        result[idx] = new Result<DataResponse>(dataResponse);
                    }
                    else
                    {
                        result[idx] = new Result<DataResponse>(valuesResult.StatusCode, valuesResult.Error);
                    }
                }
            }
            return result;
        }


        private Result<DataResponse>[] ReadEvents(Session session, OpcUAQuery[] opcUAQuery)
        {
            //session.ReadEvents();
            throw new NotImplementedException();
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
                    Result<DataResponse>[] responses = null;
                    var queries = queryGroup.ToArray();
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


                //foreach (DataQuery currRequest in request.Queries)
                //{
                //    try
                //    {
                //        DataResponse dataResponse = new DataResponse();
                //        OpcUAQuery query = new OpcUAQuery(currRequest);
                //        DataFrame dataFrame = new DataFrame(query.refId);

                //        log.Debug("Processing a request: {0} {1} {2}", query.nodeId, query.value, query.readType);
                //        log.Debug("Query: {0}", query);

                //        //switch (query.readType)
                //        //{
                //        //    case "ReadNode":
                //        //        {
                //        //            log.Debug("Reading node");
                //        //            var nId = NodeId.Parse(query.nodeId);
                //        //            DataValue value = connection.ReadNodeValue(nId);
                //        //            log.Debug("Got a value {0}, {1}", value.Value, value.Value.GetType());

                //        //            Field timeField = dataFrame.AddField("Time", value.SourceTimestamp.GetType());
                //        //            Field valueField = dataFrame.AddField(String.Join(" / ", query.value), value.Value.GetType());
                //        //            timeField.Append(value.SourceTimestamp);
                //        //            valueField.Append(value.Value);
                //        //        }
                //        //        break;
                //        //    case "Subscribe":
                //        //        {
                //        //            // TODO:
                //        //            // connection.AddSubscription(query.refId, query.nodeId, SubscriptionCallback);
                //        //        }
                //        //        break;
                //        //    case "ReadDataRaw":
                //        //        {
                //        //            DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(query.timeRange.FromEpochMS).UtcDateTime;
                //        //            DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(query.timeRange.ToEpochMS).UtcDateTime;
                //        //            log.Debug("Parsed Time: {0} {1}", fromTime, toTime);

                //        //            IEnumerable<DataValue> readResults = connection.ReadHistoryRawDataValues(
                //        //                query.nodeId,
                //        //                fromTime,
                //        //                toTime,
                //        //                (uint)query.maxDataPoints,
                //        //                false);

                //        //            Field timeField = dataFrame.AddField<DateTime>("Time");
                //        //            Field valueField = null;
                //        //            foreach (DataValue entry in readResults)
                //        //            {
                //        //                if (valueField == null && entry.Value != null)
                //        //                {
                //        //                    valueField = dataFrame.AddField(String.Join(" / ", query.value), entry.Value.GetType());
                //        //                }

                //        //                if (valueField != null)
                //        //                {
                //        //                    valueField.Append(entry.Value);
                //        //                    timeField.Append(entry.SourceTimestamp);
                //        //                }
                //        //            }
                //        //        }
                //        //        break;
                //        //    case "ReadDataProcessed":
                //        //        {
                //        //            DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(query.timeRange.FromEpochMS).UtcDateTime;
                //        //            DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(query.timeRange.ToEpochMS).UtcDateTime;
                //        //            log.Debug("Parsed Time: {0} {1}, Aggregate {2}", fromTime, toTime, query.aggregate);
                //        //            OpcUaNodeDefinition aggregate = JsonSerializer.Deserialize<OpcUaNodeDefinition>(query.aggregate.ToString());

                //        //            IEnumerable<DataValue> readResults = connection.ReadHistoryProcessed(
                //        //                query.nodeId,
                //        //                fromTime,
                //        //                toTime,
                //        //                aggregate.nodeId,
                //        //                query.intervalMs,
                //        //                (uint)query.maxDataPoints,
                //        //                true);

                //        //            Field timeField = dataFrame.AddField<DateTime>("Time");
                //        //            Field valueField = null;
                //        //            foreach (DataValue entry in readResults)
                //        //            {
                //        //                if (valueField == null && entry.Value != null)
                //        //                {
                //        //                    valueField = dataFrame.AddField(String.Join(" / ", query.value), entry.Value.GetType());
                //        //                }

                //        //                if (valueField != null)
                //        //                {
                //        //                    valueField.Append(entry.Value);
                //        //                    timeField.Append(entry.SourceTimestamp);
                //        //                }
                //        //            }
                //        //        }
                //        //        break;
                //        //}

                //        dataResponse.Frames.Add(dataFrame.ToGprcArrowFrame());

                //        // Handle the alias parsing

                //        // Deliver the response
                //        response.Responses[currRequest.RefId] = dataResponse;
                //    }
                //    catch (Exception ex)
                //    {
                //        response.Responses[currRequest.RefId].Error = ex.Message;
                //    }
               // }
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
