using System;
using System.Collections.Generic;
using System.Text;
using Models;
using Serilog;
using Grpc.Core;
using System.Threading.Tasks;
using Opc.Ua;
using MicrosoftOpcUa.Client.Utility;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using Google.Protobuf;
using System.Diagnostics;

namespace plugin_dotnet
{
    class OpcUAQuery
    {
        public string refId { get; set; }
        public Int64 maxDataPoints { get; set; }
        public Int64 intervalMs { get; set; }
        public Int64 datasourceId { get; set; }
        public string call { get; set; }
        public Dictionary<string, string> callParams { get; set; }
    }

    class OpcUaJsonData
    {
        public bool tlsAuth { get; set; }
        public bool tlsSkipVerify { get; set; }
    }

    class BrowseResultsEntry
    {
        public string displayName { get; set; }
        public string browseName { get; set; }
        public string nodeId { get; set; }
        public bool isForward { get; set; }
        public uint nodeClass { get; set; }

        public BrowseResultsEntry() {}
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

    class OpcUaDatasource : DatasourcePlugin.DatasourcePluginBase
    {
        private Dictionary<string, OpcUaClient> clientConnections = new Dictionary<string, OpcUaClient>();
        private readonly Serilog.Core.Logger log;
        private const string rootNode = "i=84";
        private const string objectsNode = "i=85";
        private string browseResults = null;

        public OpcUaDatasource(Serilog.Core.Logger logIn)
        {
            log = logIn;
            clientConnections = new Dictionary<string, OpcUaClient>();
        }

        public OpcUaClient GetClient(DatasourceRequest request, OpcUaJsonData jsonData) 
        {
            OpcUaClient client = null;

            try 
            {
                client = clientConnections[request.Datasource.Url];
            }
            catch(System.Collections.Generic.KeyNotFoundException) 
            {
                Task<OpcUaClient> connectTask;
                if (jsonData.tlsAuth) 
                {
                    connectTask = ConnectAsync(request.Datasource.Url, request.Datasource.DecryptedSecureJsonData["tlsClientCert"], request.Datasource.DecryptedSecureJsonData["tlsClientKey"]);
                } 
                else 
                {
                    connectTask = ConnectAsync(request.Datasource.Url);
                }

                connectTask.Wait();
                client = connectTask.Result;
                clientConnections[request.Datasource.Url] = client;
            }
            catch(System.NullReferenceException)
            {
                clientConnections = new Dictionary<string, OpcUaClient>();
            }
            
            return client;
        }


        public BrowseResultsEntry[] FlatBrowse(OpcUaClient client, string nodeToBrowse = objectsNode)
        {
            List<BrowseResultsEntry> browseResults = new List<BrowseResultsEntry>();

            foreach (ReferenceDescription entry in client.BrowseNodeReference(nodeToBrowse))
            {
                BrowseResultsEntry bre = new BrowseResultsEntry();
                log.Information("Processing entry {0}", JsonSerializer.Serialize<ReferenceDescription>(entry));
                if (entry.NodeClass == NodeClass.Variable)
                {
                    bre.displayName = entry.DisplayName.ToString();
                    bre.browseName = entry.BrowseName.ToString();
                    bre.nodeId = entry.NodeId.ToString();
                    bre.isForward = entry.IsForward;
                    bre.nodeClass = Convert.ToUInt32(entry.NodeClass);
                    browseResults.Add(bre);
                }
                browseResults.AddRange(FlatBrowse(client, entry.NodeId.ToString()));
            }

            return browseResults.ToArray();
        }

        public override Task<DatasourceResponse> Query(DatasourceRequest request, ServerCallContext context)
        {
            DatasourceResponse response = new DatasourceResponse { };

            try
            {
                log.Information("got a request: {0}", request);
                List<OpcUAQuery> queries = ParseJSONQueries(request);
                OpcUaJsonData jsonData = JsonSerializer.Deserialize<OpcUaJsonData>(request.Datasource.JsonData);
                OpcUaClient client = GetClient(request, jsonData);
                log.Information("got a client {0}", client);

                // Process the queries
                foreach (OpcUAQuery query in queries)
                {
                    QueryResult queryResult = new QueryResult();
                    queryResult.RefId = query.refId;
                    switch (query.call)
                    {
                        case "Browse":
                            {
                                log.Information("client {0}", client);
                                var results = client.BrowseNodeReference(query.callParams["nodeId"]);
                                BrowseResultsEntry[] browseResults = new BrowseResultsEntry[results.Length];
                                for (int i = 0; i < results.Length; i++)
                                {
                                    browseResults[i] = new BrowseResultsEntry(
                                        results[i].DisplayName.ToString(), 
                                        results[i].BrowseName.ToString(), 
                                        results[i].NodeId.ToString(),
                                        results[i].IsForward,
                                        Convert.ToUInt32(results[i].NodeClass));
                                }
                                var jsonResults = JsonSerializer.Serialize<BrowseResultsEntry[]>(browseResults);
                                queryResult.MetaJson = jsonResults;
                            }
                            break;
                        case "FlatBrowse":
                            {
                                if (this.browseResults == null)
                                {
                                    var browseResults = this.FlatBrowse(client);
                                    var jsonResults = JsonSerializer.Serialize<BrowseResultsEntry[]>(browseResults);
                                    this.browseResults = jsonResults;
                                }
                                queryResult.MetaJson = this.browseResults;

                            }
                            break;
                        case "ReadNode":
                            {
                                log.Information("ReadNode {0}", query.callParams["nodeId"]);
                                var readResults = client.ReadNode(query.callParams["nodeId"]);
                                log.Information("Results: {0}", readResults);
                                var jsonResults = JsonSerializer.Serialize<DataValue>(readResults);
                                queryResult.MetaJson = jsonResults;
                            }
                            break;
/*                        case "Subscription":
                            {
                                client.AddSubscription(query.refId, query.callParams["nodeId"], SubCallback);
                                queryResult.MetaJson = "";
                            }
                            break;*/
                        case "ReadDataRaw":
                            {
                                log.Information("Query: {0}", request.TimeRange);
                                DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(request.TimeRange.FromEpochMs).UtcDateTime;
                                DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(request.TimeRange.ToEpochMs).UtcDateTime;
                                log.Information("Parsed Time: {0} {1}", fromTime, toTime);

                                var readResults = client.ReadHistoryRawDataValues(
                                    query.callParams["nodeId"],
                                    fromTime,
                                    toTime,
                                    (uint)query.maxDataPoints,
                                    false);

                                var jsonResults = JsonSerializer.Serialize<IEnumerable<DataValue>>(readResults);
                                queryResult.MetaJson = jsonResults;
                            }
                            break;
                        case "ReadDataProcessed":
                            {
                                DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(request.TimeRange.FromEpochMs).UtcDateTime;
                                DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(request.TimeRange.ToEpochMs).UtcDateTime;
                                var readResults = client.ReadHistoryProcessed(
                                    query.callParams["nodeId"],
                                    fromTime,
                                    toTime,
                                    query.callParams["aggregate"],
                                    query.intervalMs,
                                    (uint)query.maxDataPoints,
                                    false);
                                var jsonResults = JsonSerializer.Serialize<IEnumerable<DataValue>>(readResults);
                                log.Information("Results: {0}", readResults);
                                queryResult.MetaJson = jsonResults;
                            }
                            break;
                    }
                    response.Results.Add(queryResult);
                }
            }
            catch (Exception ex)
            {
                // Close out the client connection.
                clientConnections[request.Datasource.Url].Disconnect();
                clientConnections.Remove(request.Datasource.Url);
                log.Information("Error: {0}", ex);
                QueryResult queryResult = new QueryResult();
                queryResult.Error = ex.ToString();
                response.Results.Add(queryResult);
            }

            return response;
        }

        private List<OpcUAQuery> ParseJSONQueries(DatasourceRequest request)
        {
            List<OpcUAQuery> ret = new List<OpcUAQuery>();

            foreach (Query query in request.Queries)
            {
                try
                {
                    log.Information("Handling {0}", query.ModelJson);
                    OpcUAQuery uaq = JsonSerializer.Deserialize<OpcUAQuery>(query.ModelJson);
                    ret.Add(uaq);
                }
                catch (Exception ex)
                {
                    Log.Information("Caught exception {0}", ex);
                }
            }

            return ret;
        }

        private static async Task<OpcUaClient> ConnectAsync(string endpoint, string cert, string key)
        {
            X509Certificate2 certificate = new X509Certificate2(Encoding.ASCII.GetBytes(cert));
            X509Certificate2 certWithKey = CertificateFactory.CreateCertificateWithPEMPrivateKey(certificate, Encoding.ASCII.GetBytes(key));
            CertificateIdentifier certificateIdentifier = new CertificateIdentifier(certWithKey);

            var client = new OpcUaClient();
            client.UserIdentity = new UserIdentity(certWithKey);
            client.UseSecurity = true;
            client.ApplicationCertificate = certificateIdentifier;

            await client.ConnectServer(endpoint);
            return client;
        }

        private static async Task<OpcUaClient> ConnectAsync(string endpoint)
        {
            var client = new OpcUaClient();
            client.UseSecurity = false;
            await client.ConnectServer(endpoint);
            return client;
        }
    }
}
