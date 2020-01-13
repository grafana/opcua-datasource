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

    class BrowseResultsEntry
    {
        public string displayName { get; set; }
        public string browseName { get; set; }
        public string nodeId { get; set; }

        public BrowseResultsEntry() {}
        public BrowseResultsEntry(string displayName, string browseName, string nodeId)
        {
            this.displayName = displayName;
            this.browseName = browseName;
            this.nodeId = nodeId;
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
        private OpcUaClient client = null;
        private readonly Serilog.Core.Logger log;
        private const string rootNode = "i=84";
        private const string objectsNode = "i=85";
        private string browseResults = null;

        public OpcUaDatasource(Serilog.Core.Logger logIn)
        {
            log = logIn;
        }

        public BrowseResultsEntry[] FlatBrowse(string nodeToBrowse = objectsNode)
        {
            List<BrowseResultsEntry> browseResults = new List<BrowseResultsEntry>();

            foreach (ReferenceDescription entry in this.client.BrowseNodeReference(nodeToBrowse))
            {
                BrowseResultsEntry bre = new BrowseResultsEntry();
                log.Information("Processing entry {0}", JsonSerializer.Serialize<ReferenceDescription>(entry));
                if (entry.NodeClass == NodeClass.Variable)
                {
                    bre.displayName = entry.DisplayName.ToString();
                    bre.browseName = entry.BrowseName.ToString();
                    bre.nodeId = entry.NodeId.ToString();
                    browseResults.Add(bre);
                }
                browseResults.AddRange(FlatBrowse(entry.NodeId.ToString()));
            }

            return browseResults.ToArray();
        }

        public async override Task<DatasourceResponse> Query(DatasourceRequest request, ServerCallContext context)
        {
            DatasourceResponse response = new DatasourceResponse { };

            try
            {
                log.Information("got a request: {0}", request);
                List<OpcUAQuery> queries = ParseJSONQueries(request);

                // Prepare a response
                if (client == null)
                {
                    client = await ConnectAsync(request.Datasource.Url, request.Datasource.DecryptedSecureJsonData["tlsClientCert"], request.Datasource.DecryptedSecureJsonData["tlsClientKey"]);
                }

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
                                    browseResults[i] = new BrowseResultsEntry(results[i].DisplayName.ToString(), results[i].BrowseName.ToString(), results[i].NodeId.ToString());
                                }
                                var jsonResults = JsonSerializer.Serialize<BrowseResultsEntry[]>(browseResults);
                                queryResult.MetaJson = jsonResults;
                            }
                            break;
                        case "FlatBrowse":
                            {
                                if (this.browseResults == null)
                                {
                                    var browseResults = this.FlatBrowse();
                                    var jsonResults = JsonSerializer.Serialize<BrowseResultsEntry[]>(browseResults);
                                    this.browseResults = jsonResults;
                                }
                                queryResult.MetaJson = this.browseResults;

                            }
                            break;
                        case "ReadDataRaw":
                            {
                                var readResults = client.ReadHistoryRawDataValues(
                                    query.callParams["nodeId"],
                                    DateTime.Parse(request.TimeRange.FromRaw, null, System.Globalization.DateTimeStyles.RoundtripKind),
                                    DateTime.Parse(request.TimeRange.ToRaw, null, System.Globalization.DateTimeStyles.RoundtripKind),
                                    (uint)query.maxDataPoints,
                                    true);
                                var jsonResults = JsonSerializer.Serialize<IEnumerable<DataValue>>(readResults);
                                log.Information("Results: {0}", readResults);
                                queryResult.MetaJson = jsonResults;
                            }
                            break;
                        case "ReadDataProcessed":
                            {
                                var readResults = client.ReadHistoryProcessed(
                                    query.callParams["nodeId"],
                                    DateTime.Parse(request.TimeRange.FromRaw, null, System.Globalization.DateTimeStyles.RoundtripKind),
                                    DateTime.Parse(request.TimeRange.ToRaw, null, System.Globalization.DateTimeStyles.RoundtripKind),
                                    query.intervalMs,
                                    (uint)query.maxDataPoints,
                                    true);
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
    }
}
