using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using Grpc.Core.Logging;
using System.Threading.Tasks;
using Opc.Ua;
using MicrosoftOpcUa.Client.Utility;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using Google.Protobuf;
using Pluginv2;

namespace plugin_dotnet
{
    class OpcUAQuery
    {
        public string refId { get; set; }
        public Int64 maxDataPoints { get; set; }
        public Int64 intervalMs { get; set; }
        public Int64 datasourceId { get; set; }
        public TimeRange timeRange { get; set; }
        public string nodeId { get; set; }
        public string[] value { get; set; }
        public string readType { get; set; }
        public object aggregate { get; set; }
        public string interval { get; set; }

        public OpcUAQuery() { }

        public OpcUAQuery(DataQuery dataQuery)
        {
            refId = dataQuery.RefId;
            maxDataPoints = dataQuery.MaxDataPoints;
            intervalMs = dataQuery.IntervalMS;
            timeRange = dataQuery.TimeRange;
            byte[] byDecoded = Convert.FromBase64String(dataQuery.Json.ToBase64());
            OpcUAQuery query = JsonSerializer.Deserialize<OpcUAQuery>(byDecoded);
            datasourceId = query.datasourceId;
            nodeId = query.nodeId;
            value = query.value;
            readType = query.readType;
            aggregate = query.aggregate;
            interval = query.interval;
        }

        public OpcUAQuery(string refId, Int64 maxDataPoints, Int64 intervalMs, Int64 datasourceId, string nodeId)
        {
            this.refId = refId;
            this.maxDataPoints = maxDataPoints;
            this.intervalMs = intervalMs;
            this.datasourceId = datasourceId;
            this.nodeId = nodeId;
        }
    }

    class OpcUaJsonData
    {
        public bool tlsAuth { get; set; }
        public bool tlsAuthWithCACert { get; set; }
        public bool tlsSkipVerify { get; set; }

        public OpcUaJsonData(ByteString base64encoded)
        {
            byte[] byDecoded = System.Convert.FromBase64String(base64encoded.ToString());
            OpcUaJsonData jsonData = JsonSerializer.Deserialize<OpcUaJsonData>(byDecoded);
            tlsAuth = jsonData.tlsAuth;
            tlsAuthWithCACert = jsonData.tlsAuthWithCACert;
            tlsSkipVerify = jsonData.tlsSkipVerify;
        }
    }

    class BrowseResultsEntry
    {
        public string displayName { get; set; }
        public string browseName { get; set; }
        public string nodeId { get; set; }
        public bool isForward { get; set; }
        public uint nodeClass { get; set; }

        public BrowseResultsEntry() { }
        public BrowseResultsEntry(string displayName, string browseName, string nodeId, bool isForward, uint nodeClass)
        {
            this.displayName = displayName;
            this.browseName = browseName;
            this.nodeId = nodeId;
            this.isForward = isForward;
            this.nodeClass = nodeClass;
        }
    }

    class LogMessage
    {
        public string type { get; set; }
        public string message { get; set; }

        public LogMessage(string type, string message)
        {
            this.type = type;
            this.message = message;
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize<LogMessage>(this);
        }
    }

    class OpcUaDatasource
    {
        private readonly ILogger log;
        private const string rootNode = "i=84";
        private const string objectsNode = "i=85";
        private string browseResults = null;

        public OpcUaDatasource(ILogger logIn)
        {
            log = logIn;
        }

        public BrowseResultsEntry[] FlatBrowse(OpcUaClient client, string nodeToBrowse = objectsNode)
        {
            List<BrowseResultsEntry> browseResults = new List<BrowseResultsEntry>();

            foreach (ReferenceDescription entry in client.BrowseNodeReference(nodeToBrowse))
            {
                BrowseResultsEntry bre = new BrowseResultsEntry();
                log.Debug("Processing entry {0}", JsonSerializer.Serialize<ReferenceDescription>(entry));
                if (entry.NodeClass == NodeClass.Variable)
                {
                    bre.displayName = entry.DisplayName.ToString();
                    bre.browseName = entry.BrowseName.ToString();
                    bre.nodeId = entry.NodeId.ToString();
                    browseResults.Add(bre);
                }
                browseResults.AddRange(FlatBrowse(client, entry.NodeId.ToString()));
            }

            return browseResults.ToArray();
        }

        public async Task<string> Query(PluginContext context, DataQuery dataQuery)
        {
            string result = "";
            string url = context.DataSourceInstanceSettings.Url;

            try
            {
                OpcUAConnection connection = new OpcUAConnection(context.DataSourceInstanceSettings.Url);

                

                OpcUAQuery query = new OpcUAQuery(dataQuery);
            //    switch (query)
            //    {
            //        case "Browse":
            //            {
                            
            //            }
            //            break;
            //        case "FlatBrowse":
            //            {
            //                if (this.browseResults == null)
            //                {
            //                    var browseResults = this.FlatBrowse(connection);
            //                    var jsonResults = JsonSerializer.Serialize<BrowseResultsEntry[]>(browseResults);
            //                    this.browseResults = jsonResults;
            //                }
            //                result = this.browseResults;

            //            }
            //            break;
            //        case "ReadNode":
            //            {
            //                var readResults = connection.ReadNode(query.nodeId);
            //                result = JsonSerializer.Serialize<DataValue>(readResults);
            //            }
            //            break;
            //        /*                        case "Subscription":
            //                                    {
            //                                        client.AddSubscription(query.refId, query.callParams["nodeId"], SubCallback);
            //                                        queryResult.MetaJson = "";
            //                                    }
            //                                    break;*/
            //        case "ReadDataRaw":
            //            {
            //                log.Debug("Query: {0}", dataQuery.TimeRange);
            //                DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(dataQuery.TimeRange.FromEpochMS).UtcDateTime;
            //                DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(dataQuery.TimeRange.ToEpochMS).UtcDateTime;
            //                log.Debug("Parsed Time: {0} {1}", fromTime, toTime);

            //                var readResults = connection.ReadHistoryRawDataValues(
            //                    query.nodeId,
            //                    fromTime,
            //                    toTime,
            //                    (uint)query.maxDataPoints,
            //                    false);

            //                result = JsonSerializer.Serialize<IEnumerable<DataValue>>(readResults);
            //            }
            //            break;
            //        case "ReadDataProcessed":
            //            {
            //                //DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(dataQuery.TimeRange.FromEpochMS).UtcDateTime;
            //                //DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(dataQuery.TimeRange.ToEpochMS).UtcDateTime;
            //                //var readResults = connection.ReadHistoryProcessed(
            //                //    query.nodeId
            //                //    fromTime,
            //                //    toTime,
            //                //    query.callParams["aggregate"],
            //                //    query.intervalMs,
            //                //    (uint)query.maxDataPoints,
            //                //    true);
            //                //result = JsonSerializer.Serialize<IEnumerable<DataValue>>(readResults);
            //                //log.Debug("Results: {0}", readResults);
            //            }
            //            break;
            //    }
            }
            catch (Exception ex)
            {
            //    // Close out the client connection.
            //    //clientConnections[url].Disconnect();
            //    //clientConnections.Remove(url);
            //    log.Error("Error: {0}", ex);
            }

            return await Task.FromResult(result);
        }
    }
}