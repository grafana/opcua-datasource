using System;
using System.Collections.Generic;
using System.Text;
using Pluginv2;
using Grpc.Core;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using System.Text.Json;
using System.Linq;
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
            BrowsePath path = new BrowsePath();

            NodeId nodeId = Converter.GetNodeId(query.nodePath.node.nodeId, namespaceTable);
            path.StartingNode = nodeId;
            if (query.useTemplate && query.relativePath != null && query.relativePath.Length > 0)
            {
                RelativePath r = new RelativePath();
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
            Result<NodeId>[] results = new Result<NodeId>[browsePaths.Length];
            BrowsePath[] bps = browsePaths.Where(bp => bp.RelativePath.Elements.Count > 0).ToArray();
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
                            ExpandedNodeId expandedNodeId = bpr[j].Targets[0].TargetId;
                            results[i] = new Result<NodeId>(ExpandedNodeId.ToNodeId(expandedNodeId, namespaceTable));
                        }
                        else
                        {
                            string msg = $"Could not find node id for start node: '{browsePaths[i].StartingNode}' with browsepath '{PathElementsToString(browsePaths[i].RelativePath.Elements)}'";
                            _log.Error(msg);
                            results[i] = new Result<NodeId>(Opc.Ua.StatusCodes.BadBrowseNameInvalid, msg);
                        }
                    }
                    else
                    {
                        string msg = $"Could not find node id for start node: '{browsePaths[i].StartingNode}' with browsepath '{PathElementsToString(browsePaths[i].RelativePath.Elements)}'. StatusCode: '{bpr[j].StatusCode}'";
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

                foreach(RelativePathElement elem in elements)
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
            Result<DataResponse>[] results = new Result<DataResponse>[queries.Length];
            BrowsePath[] browsePaths = queries.Select(a => ResolveRelativePath(a, namespaceTable)).ToArray();
            Result<NodeId>[] nodeIdsResult = GetNodeIds(session, browsePaths, namespaceTable);
            NodeId[] nodeIds = nodeIdsResult.Where(a => a.Success).Select(a => a.Value).ToArray();
            DataValue[] dvs = session.ReadNodeValues(nodeIds);

            for (int i = 0, j = 0; i < browsePaths.Length; i++)
            {
                if (nodeIdsResult[i].Success)
                {
                    DataValue dataValue = dvs[j];
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

        private Result<DataResponse>[] ReadHistoryRaw(Session session, OpcUAQuery[] queries, NamespaceTable namespaceTable, Settings settings)
        {
            Dictionary<ReadRawKey, List<int>> indexMap = new Dictionary<ReadRawKey, List<int>>();
            Dictionary<ReadRawKey, List<NodeId>> queryMap = new Dictionary<ReadRawKey, List<NodeId>>();

            BrowsePath[] relativePaths = queries.Select(a => ResolveRelativePath(a, namespaceTable)).ToArray();
            Result<NodeId>[] nodeIdsResult = GetNodeIds(session, relativePaths, namespaceTable);
            //Result<UaMetaData>[] metaData = GetMetaData(session, nodeIdsResult, namespaceTable);


            Result<DataResponse>[] result = new Result<DataResponse>[queries.Length];
            for (int i = 0; i < queries.Length; i++)
            {
                Result<NodeId> nodeIdResult = nodeIdsResult[i];
                if (nodeIdResult.Success)
                {
                    OpcUAQuery query = queries[i];
                    int maxValues = query.maxValuesPerNode > 0 ? query.maxValuesPerNode : 0;
                    TimeRange tr = query.timeRange;
                    DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.FromEpochMS).UtcDateTime;
                    DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.ToEpochMS).UtcDateTime;
                    ReadRawKey key = new ReadRawKey(fromTime, toTime, Convert.ToInt32(maxValues));
                    AddDict(indexMap, key, i);
                    AddDict(queryMap, key, nodeIdResult.Value);
                }
                else 
                {
                    result[i] = new Result<DataResponse>(nodeIdResult.StatusCode, nodeIdResult.Error);
                }
            }

            foreach (KeyValuePair<ReadRawKey, List<NodeId>> querygroup in queryMap)
            {
                ReadRawKey key = querygroup.Key;
                NodeId[] nodes = querygroup.Value.ToArray();
                Result<HistoryData>[] historyValues = session.ReadHistoryRaw(key.StartTime, key.EndTime, key.MaxValues, nodes);
                List<int> indices = indexMap[key];
                for (int i = 0; i < indices.Count; i++)
                {
                    int idx = indices[i];
                    result[idx] = ValueDataResponse.CreateHistoryDataResponse(historyValues[i], queries[idx], relativePaths[idx], settings);
                }
            }
            return result;
        }

		//private Result<UaMetaData>[] GetMetaData(Session session, Result<NodeId>[] nodeIdsResult, NamespaceTable namespaceTable)
		//{
			
		//}

		private Result<DataResponse>[] ReadHistoryProcessed(Session session, OpcUAQuery[] queries, NamespaceTable namespaceTable, Settings settings)
        {
            Dictionary<ReadProcessedKey, List<int>> indexMap = new Dictionary<ReadProcessedKey, List<int>>();
            Dictionary<ReadProcessedKey, List<NodeId>> queryMap = new Dictionary<ReadProcessedKey, List<NodeId>>();

            BrowsePath[] browsePaths = queries.Select(a => ResolveRelativePath(a, namespaceTable)).ToArray();
            Result<NodeId>[] nodeIdsResult = GetNodeIds(session, browsePaths, namespaceTable);
            Result<DataResponse>[] result = new Result<DataResponse>[queries.Length];

            
            for (int i = 0; i < queries.Length; i++)
            {
                OpcUAQuery query = queries[i];
                OpcUaNodeDefinition aggregate = null;
                Result<NodeId> nodeIdResult = nodeIdsResult[i];
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
                    double resampleInterval = query.resampleInterval > 0 ? (double)(query.resampleInterval * 1000.0) : (double)query.intervalMs;
                    TimeRange tr = query.timeRange;
                    NodeId aggregateNodeId = Converter.GetNodeId(aggregate.nodeId, namespaceTable);
                    DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.FromEpochMS).UtcDateTime;
                    DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.ToEpochMS).UtcDateTime;
                    ReadProcessedKey key = new ReadProcessedKey(fromTime, toTime, aggregateNodeId, resampleInterval);
                    AddDict(indexMap, key, i);
                    AddDict(queryMap, key, nodeIdResult.Value);
                }
                else 
                {
                    result[i] = new Result<DataResponse>(nodeIdResult.StatusCode, nodeIdResult.Error);
                }
            }
            
            foreach (KeyValuePair<ReadProcessedKey, List<NodeId>> querygroup in queryMap)
            {
                ReadProcessedKey key = querygroup.Key;
                NodeId[] nodes = querygroup.Value.ToArray();
                Result<HistoryData>[] historyValues = session.ReadHistoryProcessed(key.StartTime, key.EndTime, key.Aggregate, key.ResampleInterval, nodes);
                List<int> indices = indexMap[key];
                for (int i = 0; i < indices.Count; i++)
                {
                    int idx = indices[i];
                    Result<HistoryData> valuesResult = historyValues[i];
                    result[idx] = ValueDataResponse.CreateHistoryDataResponse(historyValues[i], queries[idx], browsePaths[idx], settings);
                }
            }
            return result;
        }




        private Result<DataResponse>[] ReadEvents(Session session, IEventDataResponse eventDataResponse, OpcUAQuery[] opcUAQuery, NamespaceTable namespaceTable)
        {
            INodeCache nodeCache = _nodeCacheFactory.Create(session);
            BrowsePath[] browsePaths = opcUAQuery.Select(a => ResolveRelativePath(a, namespaceTable)).ToArray();
            Result<NodeId>[] nodeIdsResult = GetNodeIds(session, browsePaths, namespaceTable);

            Result<DataResponse>[] results = new Result<DataResponse>[opcUAQuery.Length];
            // Do one by one for now. unsure of use-case with multiple node ids for same filter.
            for (int i = 0; i < opcUAQuery.Length; i++)
            {
                Result<NodeId> nodeIdResult = nodeIdsResult[i];
                if (nodeIdResult.Success)
                {
                    OpcUAQuery query = opcUAQuery[i];
                    TimeRange tr = query.timeRange;
                    //let nodeId = Converter.GetNodeId(query.nodeId, namespaceTable);
                    DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.FromEpochMS).UtcDateTime;
                    DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(tr.ToEpochMS).UtcDateTime;
                    Opc.Ua.EventFilter eventFilter = Converter.GetEventFilter(query, namespaceTable);
                    Result<HistoryEvent>[] response = session.ReadEvents(fromTime, toTime, uint.MaxValue, eventFilter, new[] { nodeIdResult.Value });
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
            Dictionary<string, string> error = new Dictionary<string, string>();
            foreach (OpcUAQuery q in queries)
			{
                StringBuilder errors = new StringBuilder();
                if (q.nodePath == null && (q.readType != "Resource" || string.IsNullOrEmpty(q.readType)))
                {
                    string instanceOrType = q.useTemplate ? "Type" : "Instance";
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

                IEnumerable<OpcUAQuery> uaQueries = request.Queries.Select(q => new OpcUAQuery(q));
                IDictionary<string, string> invalidQueries = GetInvalidQueries(uaQueries);//uaQueries.Where(a => a.nodePath != null).ToLookup(a => a.refId);

                ILookup<string, OpcUAQuery> queryGroups = uaQueries.Where(a => !invalidQueries.ContainsKey(a.refId)).ToLookup(o => o.readType);
                NamespaceTable nsTable = connection.Session.NamespaceUris;
                foreach (IGrouping<string, OpcUAQuery> queryGroup in queryGroups)
                {
                    OpcUAQuery[] queries = queryGroup.ToArray();
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
                                responses = ReadHistoryRaw(connection.Session, queries, nsTable, settings);
                                break;
                            case "ReadDataProcessed":
                                responses = ReadHistoryProcessed(connection.Session, queries, nsTable, settings);
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
                            foreach (Result<DataResponse> dataResponse in responses)
                            {
                                if (dataResponse.Success)
                                    response.Responses[queries[i++].refId] = dataResponse.Value;
                                else
                                {
                                    DataResponse dr = new DataResponse();
                                    dr.Error = string.Format("{0} {1}", dataResponse.StatusCode.ToString(),  dataResponse.Error);
                                    _log.Error(dr.Error);
                                    response.Responses[queries[i++].refId] = dr;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        foreach (OpcUAQuery q in queries)
                        {
                            DataResponse dr = new DataResponse();
                            dr.Error = e.ToString();
                            response.Responses[q.refId] = dr; 
                        }
                        _log.Error(e.ToString());
                    }
                }
                foreach (KeyValuePair<string, string> invalidQuery in invalidQueries)
                {
                    string refId = invalidQuery.Key;
                    string message = invalidQuery.Value;
                    DataResponse dr = new DataResponse();
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
            Result<DataResponse>[] responses = new Result<DataResponse>[queries.Length];
            BrowsePath[] browsePaths = queries.Select(a => ResolveRelativePath(a, nsTable)).ToArray();
            Result<NodeId>[] nodeIdsResult = GetNodeIds(session, browsePaths, nsTable);

            NodeId[] nodeIds = nodeIdsResult.Where(a => a.Success).Select(a => a.Value).ToArray();

            Result<DataValue>[] dataValues = dataValueSubscription.GetValues(nodeIds);
            Result<DataResponse>[] results = new Result<DataResponse>[nodeIds.Length];
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
            Result<DataResponse>[] responses = new Result<DataResponse>[queries.Length];
            BrowsePath[] browsePaths = queries.Select(a => ResolveRelativePath(a, nsTable)).ToArray();
            Result<NodeId>[] nodeIdsResult = GetNodeIds(session, browsePaths, nsTable);

            for (int i = 0; i < queries.Length; i++)
            {
                try
                {
                    if (nodeIdsResult[i].Success)
                    {
                        NodeId nodeId = nodeIdsResult[i].Value;
                        if (queries[i].eventQuery != null)
                        {
                            Opc.Ua.EventFilter eventFilter = Converter.GetEventFilter(queries[i], nsTable);
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
