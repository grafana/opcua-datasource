using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Grpc.Core.Logging;
using MicrosoftOpcUa.Client.Core;
using MicrosoftOpcUa.Client.Utility;
using Opc.Ua;

namespace plugin_dotnet
{
    public static class Connections
    {
        static ILogger log = new ConsoleLogger();
        static Dictionary<string, OpcUAConnection> connections = new Dictionary<string, OpcUAConnection>();

        public static void Add(string url)
        {
            try
            {
                connections[key: url] = new OpcUAConnection(url);
            }
            catch (Exception ex)
            {
                log.Debug("Error while adding endpoint {0}: {1}", url, ex);
            }
        }

        public static OpcUAConnection Get(string url)
        {
            if (!connections.ContainsKey(url) || !connections[url].Connected)
            {
                Add(url);
            }
            
            return connections[url];
        }
    }

    public class OpcUAConnection : OpcUaClient
    {
        private const string rootNode = "i=84";
        private const string objectsNode = "i=85";
        private const string typesNode = "i=86";
        private const string viewsNode = "i=87";
        private string endpoint;

        private ILogger log;

        public OpcUAConnection(string url) : base()
        {
            log = new ConsoleLogger();
            base.UseSecurity = false;
            endpoint = url;
            ConnectServer(endpoint).Wait();
        }

        public string BrowseTypes(string nodeToBrowse = typesNode)
        {
            OpcNodeAttribute[] attributes = ReadNodeAttributes(nodeToBrowse);
            return JsonSerializer.Serialize<OpcNodeAttribute[]>(attributes);
        }

        public BrowseResultsEntry[] TypesBrowse(string nodeToBrowse = typesNode)
        {
            List<BrowseResultsEntry> browseResults = new List<BrowseResultsEntry>();

            foreach (ReferenceDescription entry in this.BrowseNodeReference(nodeToBrowse))
            {
                BrowseResultsEntry bre = new BrowseResultsEntry();
                log.Debug("Processing entry {0}", JsonSerializer.Serialize<ReferenceDescription>(entry));

                bre.displayName = entry.DisplayName.ToString();
                bre.browseName = entry.BrowseName.ToString();
                bre.nodeId = entry.NodeId.ToString();
                browseResults.Add(bre);
                if (entry.TypeDefinition.IdType == IdType.Opaque)
                {
                    browseResults.AddRange(FlatBrowse(entry.NodeId.ToString()));
                }
            }

            return browseResults.ToArray();
        }

        public BrowseResultsEntry[] FlatBrowse(string nodeToBrowse = typesNode)
        {
            List<BrowseResultsEntry> browseResults = new List<BrowseResultsEntry>();

            foreach (ReferenceDescription entry in this.BrowseNodeReference(nodeToBrowse))
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
                browseResults.AddRange(FlatBrowse(entry.NodeId.ToString()));
            }

            return browseResults.ToArray();
        }

        public string Browse(string nodeId = typesNode)
        {
            log.Debug("Browsing node {0}", nodeId);
            var results = this.BrowseNodeReference(nodeId);
            BrowseResultsEntry[] browseResults = new BrowseResultsEntry[results.Length];
            for (int i = 0; i < results.Length; i++)
            {
                browseResults[i] = new BrowseResultsEntry(
                    results[i].DisplayName.ToString(),
                    results[i].BrowseName.ToString(),
                    results[i].NodeId.ToString(),
                    results[i].TypeId,
                    results[i].IsForward,
                    Convert.ToUInt32(results[i].NodeClass));
            }
            return JsonSerializer.Serialize<BrowseResultsEntry[]>(browseResults);
        }

        //private async void Connect()
        //{
        //     base.ConnectServer(endpoint);
        //}

        private async void Connect(string endpoint, string cert, string key)
        {
            X509Certificate2 certificate = new X509Certificate2(Encoding.ASCII.GetBytes(cert));
            X509Certificate2 certWithKey = CertificateFactory.CreateCertificateWithPEMPrivateKey(certificate, Encoding.ASCII.GetBytes(key));
            CertificateIdentifier certificateIdentifier = new CertificateIdentifier(certWithKey);

            UserIdentity = new UserIdentity(certWithKey);
            UseSecurity = true;
            ApplicationCertificate = certificateIdentifier;

            await ConnectServer(endpoint);
        }
    }
}
