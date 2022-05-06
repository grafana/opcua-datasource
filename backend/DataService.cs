using System;
using System.Collections.Generic;
using System.Text;
using Pluginv2;
using Grpc.Core;
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
using System.IO;
using Prediktor.UA.Client;
using Grpc.Core.Logging;

namespace plugin_dotnet
{
    class OpcUaNodeDefinition
    {
        public string name { get; set; }
        public string nodeId { get; set; }
    }

    class DataService : Data.DataBase
    {
        private INodeCacheFactory _nodeCacheFactory;
        private IConnections _connections;
        private readonly ILogger _log;
        private Alias alias;


        public DataService(ILogger log, IConnections connections, INodeCacheFactory nodeCacheFactory)
        {
            _nodeCacheFactory = nodeCacheFactory;
            _connections = connections;
            _log = log;
            _log.Info("Data Service created");
            alias = new Alias();
        }


        private BrowsePath ResolveRelativePath(OpcUAQuery query, NamespaceTable namespaceTable)
        {
            var path = new BrowsePath();

            var nodeId = Converter.GetNodeId(query.nodePath.node.nodeId, namespaceTable);
            path.StartingNode = nodeId;
            if (query.useTemplate && query.relativePath != null && query.relativePath.Length > 0)
            {
                var r = new RelativePath();
                path.RelativePath = r;
                for (int i = 0; i < query.relativePath.Length; i++)
                {
                    r.Elements.Add(new RelativePathElement() { TargetName = Converter.GetQualifiedName(query.relativePath[i], namespaceTable), IncludeSubtypes = true, ReferenceTypeId = Opc.Ua.ReferenceTypes.References, IsInverse = false   }) ;
                }
            }
            return path;
        }

        private Result<NodeId>[] GetNodeIds(Session session, BrowsePath[] browsePaths, NamespaceTable namespaceTable)
        {
            var results = new Result<NodeId>[browsePaths.Length];
            var bps = browsePaths.Where(bp => bp.RelativePath.Elements.Count > 0).ToArray();
            if (bps.Length == 0)
                return browsePaths.Select(a => new Result<NodeId>(a.StartingNode)).ToArray();

            session.TranslateBrowsePathsToNodeIds(null, new BrowsePathCollection(bps), out BrowsePathResultCollection bpr, out DiagnosticInfoCollection diag);
            for (int i = 0, j = 0; i < browsePaths.Length; i++)
            {
                if (browsePaths[i].RelativePath.Elements.Count > 0)
                {
                    if (Opc.Ua.StatusCode.IsGood(bpr[j].StatusCode))
                    {
                        if (bpr[j].Targets.Count > 0)
                        {
                            var expandedNodeId = bpr[j].Targets[0].TargetId;
                            results[i] = new Result<NodeId>(ExpandedNodeId.ToNodeId(expandedNodeId, namespaceTable));
                        }
                        else
                        {
                            var msg = $"Could not find node id for start node: '{browsePaths[i].StartingNode}' with browsepath '{PathElementsToString(browsePaths[i].RelativePath.Elements)}'";
                            _log.Error(msg);
                            results[i] = new Result<NodeId>(Opc.Ua.StatusCodes.BadBrowseNameInvalid, msg);
                        }
                    }
                    else
                    {
                        var msg = $"Could not find node id for start node: '{browsePaths[i].StartingNode}' with browsepath '{PathElementsToString(browsePaths[i].RelativePath.Elements)}'. StatusCode: '{bpr[j].StatusCode}'";
                        _log.Error(msg);
                        results[i] = new Result<NodeId>(bpr[j].StatusCode, msg);
                    }
                    j++;
                }
                else
                {
                    results[i] = new Result<NodeId>(browsePaths[i].StartingNode);
                }
            }
            return results;
        }

		private static string PathElementsToString(RelativePathElementCollection elements)
		{
			if(elements != null)
			{
                StringBuilder sb = new StringBuilder();

                foreach(var elem in elements)
				{
                    sb.Append(elem.TargetName.ToString());
                    sb.Append("/");
                }

                return sb.ToString();
			}

            return string.Empty;
		}

		private Result<DataResponse>[] ReadNodes(Session session, Settings settings, OpcUAQuery[] queries, NamespaceTable namespaceTable)
        {
            var results = new Result<DataResponse>[queries.Length];
            var browsePaths = queries.Select(a => ResolveRelativePath(a, namespaceTable)).ToArray();
            Result<NodeId>[] nodeIdsResult = GetNodeIds(session, browsePaths, namespaceTable);
            var nodeIds = nodeIdsResult.Where(a => a.Success).Select(a => a.Value).ToArray();
            var dvs = session.ReadNodeValues(nodeIds);

            for (int i = 0, j = 0; i < browsePaths.Length; i++)
            {
                if (nodeIdsResult[i].Success)
                {
                    var dataValue = dvs[j];
                    results[i] = ValueDataResponse.GetDataResponseForDataValue(_log, settings, dataValue, nodeIds[j], queries[i], browsePaths[i]);
                    j++;
                }
                else
                {
                    results[i] = new Result<DataResponse>(nodeIdsResult[i].StatusCode, nodeIdsResult[i].Error);
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

            var relativePaths = queries.Select(a => ResolveRelativePath(a, namespaceTable)).ToArray();
            Result<NodeId>[] nodeIdsResult = GetNodeIds(session, relativePaths, namespaceTable);
            //Result<UaMetaData>[] metaData = GetMetaData(session, nodeIdsResult, namespaceTable);


            var result = new Result<DataResponse>[queries.Length];
            for (int i = 0; i < queries.Length; i++)
            {
                var nodeIdResult = nodeIdsResult[i];
                if (nodeIdResult.Success)
                {
                    var query = queries[i];
                    var maxValues = query.maxValuesPerNode > 0 ? query.maxValuesPerNode : 0;
                    var tr = query.timeRange;
                    DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.FromEpochMS).UtcDateTime;
                    DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.ToEpochMS).UtcDateTime;
                    var key = new ReadRawKey(fromTime, toTime, Convert.ToInt32(maxValues));
                    AddDict(indexMap, key, i);
                    AddDict(queryMap, key, nodeIdResult.Value);
                }
                else 
                {
                    result[i] = new Result<DataResponse>(nodeIdResult.StatusCode, nodeIdResult.Error);
                }
            }

            foreach (var querygroup in queryMap)
            {
                var key = querygroup.Key;
                var nodes = querygroup.Value.ToArray();
                var historyValues = session.ReadHistoryRaw(key.StartTime, key.EndTime, key.MaxValues, nodes);
                var indices = indexMap[key];
                for (int i = 0; i < indices.Count; i++)
                {
                    var idx = indices[i];
                    result[idx] = ValueDataResponse.CreateHistoryDataResponse(historyValues[i], queries[idx], relativePaths[idx]);
                }
            }
            return result;
        }

		//private Result<UaMetaData>[] GetMetaData(Session session, Result<NodeId>[] nodeIdsResult, NamespaceTable namespaceTable)
		//{
			
		//}

		private Result<DataResponse>[] ReadHistoryProcessed(Session session, OpcUAQuery[] queries, NamespaceTable namespaceTable)
        {
            var indexMap = new Dictionary<ReadProcessedKey, List<int>>();
            var queryMap = new Dictionary<ReadProcessedKey, List<NodeId>>();

            var browsePaths = queries.Select(a => ResolveRelativePath(a, namespaceTable)).ToArray();
            Result<NodeId>[] nodeIdsResult = GetNodeIds(session, browsePaths, namespaceTable);
            var result = new Result<DataResponse>[queries.Length];

            
            for (int i = 0; i < queries.Length; i++)
            {
                var query = queries[i];
                OpcUaNodeDefinition aggregate = null;
                var nodeIdResult = nodeIdsResult[i];
                try
                {
                    aggregate = JsonSerializer.Deserialize<OpcUaNodeDefinition>(query.aggregate.ToString());
                }
                catch (Exception e)
                {
                    _log.Error(e, "Error getting aggregate. ");
                }

                if (aggregate?.nodeId == null)
                {
                    _log.Error("Aggregate is not set");
                    result[i] = new Result<DataResponse>(StatusCodes.BadNodeIdInvalid, "Aggregate is not set");
                }
                else if (nodeIdResult.Success)
                {
                    var resampleInterval = query.resampleInterval > 0 ? (double)(query.resampleInterval * 1000.0) : (double)query.intervalMs;
                    var tr = query.timeRange;
                    var aggregateNodeId = Converter.GetNodeId(aggregate.nodeId, namespaceTable);
                    DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.FromEpochMS).UtcDateTime;
                    DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.ToEpochMS).UtcDateTime;
                    var key = new ReadProcessedKey(fromTime, toTime, aggregateNodeId, resampleInterval);
                    AddDict(indexMap, key, i);
                    AddDict(queryMap, key, nodeIdResult.Value);
                }
                else 
                {
                    result[i] = new Result<DataResponse>(nodeIdResult.StatusCode, nodeIdResult.Error);
                }
            }
            
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
                    result[idx] = ValueDataResponse.CreateHistoryDataResponse(historyValues[i], queries[idx], browsePaths[idx]);
                }
            }
            return result;
        }




        private Result<DataResponse>[] ReadEvents(Session session, IEventDataResponse eventDataResponse, OpcUAQuery[] opcUAQuery, NamespaceTable namespaceTable)
        {
            var nodeCache = _nodeCacheFactory.Create(session);
            var browsePaths = opcUAQuery.Select(a => ResolveRelativePath(a, namespaceTable)).ToArray();
            Result<NodeId>[] nodeIdsResult = GetNodeIds(session, browsePaths, namespaceTable);

            var results = new Result<DataResponse>[opcUAQuery.Length];
            // Do one by one for now. unsure of use-case with multiple node ids for same filter.
            for (int i = 0; i < opcUAQuery.Length; i++)
            {
                var nodeIdResult = nodeIdsResult[i];
                if (nodeIdResult.Success)
                {
                    var query = opcUAQuery[i];
                    var tr = query.timeRange;
                    //var nodeId = Converter.GetNodeId(query.nodeId, namespaceTable);
                    DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.FromEpochMS).UtcDateTime;
                    DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.ToEpochMS).UtcDateTime;
                    var eventFilter = Converter.GetEventFilter(query, namespaceTable);
                    var response = session.ReadEvents(fromTime, toTime, uint.MaxValue, eventFilter, new[] { nodeIdResult.Value });
                    results[i] = eventDataResponse.CreateEventDataResponse(response[0], query, nodeCache);
                }
                else 
                {
                    results[i] = new Result<DataResponse>(nodeIdResult.StatusCode, nodeIdResult.Error);
                }
            }
            return results;
        }


        private static IDictionary<string, string> GetInvalidQueries(IEnumerable<OpcUAQuery> queries)
		{
            var error = new Dictionary<string, string>();
            foreach (var q in queries)
			{
                var errors = new StringBuilder();
                if (q.nodePath == null && (q.readType != "Resource" || string.IsNullOrEmpty(q.readType)))
                {
                    var instanceOrType = q.useTemplate ? "Type" : "Instance";
                    errors.Append(instanceOrType + " is not specified for query: " + q.refId);
                }
                // TODO: Add more error checks.
                if (errors.Length > 0)
                {
                    error.Add(q.refId, errors.ToString());
                }
			}
            return error;
		}


        public override async Task<QueryDataResponse> QueryData(QueryDataRequest request, ServerCallContext context)
        {
            QueryDataResponse response = new QueryDataResponse();
            IConnection connection = null;

            try
            {
                _log.Debug("got a request: {0}", request);
                Settings settings = RawSettingsParser.Parse(request.PluginContext.DataSourceInstanceSettings);
                connection = _connections.Get(settings);

                var uaQueries = request.Queries.Select(q => new OpcUAQuery(q));
                var invalidQueries = GetInvalidQueries(uaQueries);//uaQueries.Where(a => a.nodePath != null).ToLookup(a => a.refId);

                var queryGroups = uaQueries.Where(a => !invalidQueries.ContainsKey(a.refId)).ToLookup(o => o.readType);
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
                                responses = ReadNodes(connection.Session, settings, queries, nsTable);
                                break;
                            case "Subscribe":
                                responses = SubscribeDataValues(connection.Session, settings, connection.DataValueSubscription, queries, nsTable);
                                break;
                            case "ReadDataRaw":
                                responses = ReadHistoryRaw(connection.Session, queries, nsTable);
                                break;
                            case "ReadDataProcessed":
                                responses = ReadHistoryProcessed(connection.Session, queries, nsTable);
                                break;
                            case "ReadEvents":
                                responses = ReadEvents(connection.Session, connection.EventDataResponse, queries, nsTable);
                                break;
                            case "SubscribeEvents":
                                responses = SubscribeEvents(connection.Session, connection.EventSubscription, queries, nsTable);
                                break;
                            case "Resource":
                                responses = null;
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
                                {
                                    var dr = new DataResponse();
                                    dr.Error = string.Format("{0} {1}", dataResponse.StatusCode.ToString(),  dataResponse.Error);
                                    _log.Error(dr.Error);
                                    response.Responses[queries[i++].refId] = dr;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        foreach (var q in queries)
                        {
                            var dr = new DataResponse();
                            dr.Error = e.ToString();
                            response.Responses[q.refId] = dr; 
                        }
                        _log.Error(e.ToString());
                    }
                }
                foreach (var invalidQuery in invalidQueries)
                {
                    var refId = invalidQuery.Key;
                    var message = invalidQuery.Value;
                    var dr = new DataResponse();
                    dr.Error = message;
                    response.Responses[refId] = dr;
                    _log.Error(message);
                }
            }
            catch (Exception ex)
            {
               // Close out the client connection.
               _log.Error("Error: {0}", ex);
               connection?.Close();
            }

            return await Task.FromResult(response);
        }

        private Result<DataResponse>[] SubscribeDataValues(Session session, Settings settings, IDataValueSubscription dataValueSubscription, OpcUAQuery[] queries, NamespaceTable nsTable)
        {
            var responses = new Result<DataResponse>[queries.Length];
            var browsePaths = queries.Select(a => ResolveRelativePath(a, nsTable)).ToArray();
            Result<NodeId>[] nodeIdsResult = GetNodeIds(session, browsePaths, nsTable);

            var nodeIds = nodeIdsResult.Where(a => a.Success).Select(a => a.Value).ToArray();

            var dataValues = dataValueSubscription.GetValues(nodeIds);
            var results = new Result<DataResponse>[nodeIds.Length];
            for (int i = 0, j = 0; i < nodeIdsResult.Length; i++)
            {
                if (nodeIdsResult[i].Success)
                {
                    if (dataValues[j].Success)
                        results[i] = ValueDataResponse.GetDataResponseForDataValue(_log, settings, dataValues[j].Value, nodeIds[j], queries[i], browsePaths[i]);
                    else
                        results[i] = new Result<DataResponse>(dataValues[j].StatusCode, dataValues[j].Error);
                    j++;
                }
                else
                {
                    results[i] = new Result<DataResponse>(nodeIdsResult[i].StatusCode, nodeIdsResult[i].Error);
                }
            }
            return results;

        }

        private Result<DataResponse>[] SubscribeEvents(Session session, IEventSubscription eventSubscription, OpcUAQuery[] queries, NamespaceTable nsTable)
        {
            var responses = new Result<DataResponse>[queries.Length];
            var browsePaths = queries.Select(a => ResolveRelativePath(a, nsTable)).ToArray();
            Result<NodeId>[] nodeIdsResult = GetNodeIds(session, browsePaths, nsTable);

            for (int i = 0; i < queries.Length; i++)
            {
                try
                {
                    if (nodeIdsResult[i].Success)
                    {
                        var nodeId = nodeIdsResult[i].Value;
                        if (queries[i].eventQuery != null)
                        {
                            var eventFilter = Converter.GetEventFilter(queries[i], nsTable);
                            responses[i] = eventSubscription.GetEventData(queries[i], nodeId, eventFilter);
                        }
                        else
                            responses[i] = new Result<DataResponse>(Opc.Ua.StatusCodes.BadUnknownResponse, "Event query null");
                    }
                    else 
                    {
                        responses[i] = new Result<DataResponse>(nodeIdsResult[i].StatusCode, nodeIdsResult[i].Error);
                    }
                }
                catch (Exception e)
                {
                    responses[i] = new Result<DataResponse>(Opc.Ua.StatusCodes.BadUnknownResponse, e.ToString());
                }
            }
            return responses;
        }
    }
}
