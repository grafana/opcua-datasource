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

        public BrowseResultsEntry(string displayName, string browseName, string nodeId)
        {
            this.displayName = displayName;
            this.browseName = browseName;
            this.nodeId = nodeId;
        }
    }

    class OpcUaDatasource : DatasourcePlugin.DatasourcePluginBase
    {
        private OpcUaClient client = null;
        private readonly Serilog.Core.Logger log;

        public OpcUaDatasource(Serilog.Core.Logger logIn)
        {
            log = logIn;
        }

        public async override Task<DatasourceResponse> Query(DatasourceRequest request, ServerCallContext context)
        {
            log.Information("got a request: {0}", request);
            List<OpcUAQuery> queries = ParseJSONQueries(request);

            // Prepare a response
            DatasourceResponse response = new DatasourceResponse { };
            if (this.client == null)
            {
                this.client = await ConnectAsync(request.Datasource.Url, request.Datasource.DecryptedSecureJsonData["tlsClientCert"], request.Datasource.DecryptedSecureJsonData["tlsClientKey"]);
            }

            // Process the queries
            foreach (OpcUAQuery query in queries)
            {
                QueryResult queryResult = new QueryResult();
                queryResult.RefId = query.refId;
                switch (query.call)
                {
                    case "Browse":
                        var results = client.BrowseNodeReference(query.callParams["nodeId"]);
                        log.Information("Browse result => {0}", results);
                        BrowseResultsEntry[] browseResults = new BrowseResultsEntry[results.Length];
                        for (int i = 0; i < results.Length; i++) {
                            browseResults[i] = new BrowseResultsEntry(results[i].DisplayName.ToString(), results[i].BrowseName.ToString(), results[i].NodeId.ToString());
                        }
                        var jsonResults = JsonSerializer.Serialize<BrowseResultsEntry[]>(browseResults);
                        queryResult.MetaJson = jsonResults;
                        break;
                }
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
                    log.Information("Query Item {0} {1}", uaq.call, uaq.datasourceId);

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
