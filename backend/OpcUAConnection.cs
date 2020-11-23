using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Grpc.Core.Logging;
using Opc.Ua;
using Opc.Ua.Client;
using Pluginv2;
using Prediktor.UA.Client;

namespace plugin_dotnet
{

    public interface IConnections
    {
        void Add(string url, string clientCert, string clientKey);
        void Add(string url);
        Session Get(DataSourceInstanceSettings settings);
        Session Get(string url);

        void Remove(string url);
    }

    public class Connections : IConnections
    {
        private ISessionFactory _sessionFactory;
        private Func<ApplicationConfiguration> _applicationConfiguration;
        private Dictionary<string, Session> connections = new Dictionary<string, Session>();
        private ILogger log;
        public Connections(ISessionFactory sessionFactory, Func<ApplicationConfiguration> applicationConfiguration)
        {
            _sessionFactory = sessionFactory;
            _applicationConfiguration = applicationConfiguration;
            log = new ConsoleLogger();
        }

        public void Add(string url, string clientCert, string clientKey)
        {
            try
            {
                X509Certificate2 certificate = new X509Certificate2(Encoding.ASCII.GetBytes(clientCert));
                X509Certificate2 certWithKey = CertificateFactory.CreateCertificateWithPEMPrivateKey(certificate, Encoding.ASCII.GetBytes(clientKey));
                CertificateIdentifier certificateIdentifier = new CertificateIdentifier(certWithKey);

                var userIdentity = new UserIdentity(certWithKey);


                var appConfig = _applicationConfiguration();
                appConfig.SecurityConfiguration.ApplicationCertificate = certificateIdentifier;
                lock (connections)
                {
                    connections[key: url] = _sessionFactory.CreateSession(url, "Grafana Session", userIdentity, true, appConfig);
                }
            }
            catch (Exception ex)
            {
                log.Debug("Error while adding endpoint {0}: {1}", url, ex);
            }
        }

        public void Add(string url)
        {
            try
            {
                lock (connections)
                {
                    connections[key: url] = _sessionFactory.CreateAnonymously(url, "Grafana Anonymous Session", false, _applicationConfiguration());
                }
            }
            catch (Exception ex)
            {
                log.Debug("Error while adding endpoint {0}: {1}", url, ex);
            }
        }

        public Session Get(string url)
        {
            lock (connections)
            {

                if (!connections.ContainsKey(url) || !connections[url].Connected)
                {
                    Add(url);
                }

                return connections[url];
            }
        }

        public Session Get(DataSourceInstanceSettings settings)
        {
            lock (connections)
            {

                if (!connections.ContainsKey(settings.Url) || !connections[settings.Url].Connected)
                {
                    if (settings.DecryptedSecureJsonData.ContainsKey("tlsClientCert") && settings.DecryptedSecureJsonData.ContainsKey("tlsClientKey"))
                    {
                        Add(settings.Url, settings.DecryptedSecureJsonData["tlsClientCert"], settings.DecryptedSecureJsonData["tlsClientKey"]);
                    }
                    else
                    {
                        Add(settings.Url);
                    }
                }

                return connections[settings.Url];
            }
        }

        public void Remove(string url)
        {
            lock (connections)
            {
                connections.Remove(url);
            }
        }
    }


    //    public static class Connections
    //    {
    //        static ILogger log = new ConsoleLogger();
    //        static Dictionary<string, Session> connections = new Dictionary<string, Session>();

    //        public void Add(string url)
    //        {
    //            try
    //            {
    //                connections[key: url] = new OpcUAConnection(url);
    //            }
    //            catch (Exception ex)
    //            {
    //                log.Debug("Error while adding endpoint {0}: {1}", url, ex);
    //            }
    //        }

    //        public void Add(string url, string clientCert, string clientKey)
    //        {
    //            try
    //            {
    //                connections[key: url] = new OpcUAConnection(url, clientCert, clientKey);
    //            }
    //            catch (Exception ex)
    //            {
    //                log.Debug("Error while adding endpoint {0}: {1}", url, ex);
    //            }
    //        }

    //        public Session Get(string url)
    //        {
    //            if (!connections.ContainsKey(url) || !connections[url].Connected)
    //            {
    //                Add(url);
    //            }

    //            return connections[url];
    //        }

    //        public Session Get(DataSourceInstanceSettings settings)
    //        {
    //            if (!connections.ContainsKey(settings.Url) || !connections[settings.Url].Connected)
    //            {
    //                if (settings.DecryptedSecureJsonData.ContainsKey("tlsClientCert") && settings.DecryptedSecureJsonData.ContainsKey("tlsClientKey"))
    //                {
    //                    Add(settings.Url, settings.DecryptedSecureJsonData["tlsClientCert"], settings.DecryptedSecureJsonData["tlsClientKey"]);
    //                }
    //                else
    //                {
    //                    Add(settings.Url);
    //                }
    //            }

    //            return connections[settings.Url];
    //        }

    //        public static void Remove(string url)
    //        {
    //            connections.Remove(url);
    //        }
    //    }

    //public class OpcUAConnection : OpcUaClient
    //{
    //    private const string rootNode = "i=84";
    //    private const string objectsNode = "i=85";
    //    private const string typesNode = "i=86";
    //    private const string viewsNode = "i=87";
    //    private string endpoint;

    //    private ILogger log;

    //    public OpcUAConnection()
    //    {
    //        log = new ConsoleLogger();
    //    }

    //    public OpcUAConnection(string url) : this()
    //    {
    //        base.UseSecurity = false;
    //        endpoint = url;
    //        ConnectServer(endpoint).Wait();
    //    }

    //    public OpcUAConnection(string url, string cert, string key) : this()
    //    {
    //        base.UseSecurity = true;
    //        endpoint = url;
    //        Connect(endpoint, cert, key);
    //    }

    //    public void Close()
    //    {
    //        base.Disconnect();
    //        Connections.Remove(endpoint);
    //    }

    //    public string BrowseTypes(string nodeToBrowse = typesNode)
    //    {
    //        OpcNodeAttribute[] attributes = ReadNodeAttributes(nodeToBrowse);
    //        return JsonSerializer.Serialize<OpcNodeAttribute[]>(attributes);
    //    }

    //    public BrowseResultsEntry[] TypesBrowse(string nodeToBrowse = typesNode)
    //    {
    //        List<BrowseResultsEntry> browseResults = new List<BrowseResultsEntry>();

    //        foreach (ReferenceDescription entry in this.BrowseNodeReference(nodeToBrowse))
    //        {
    //            BrowseResultsEntry bre = new BrowseResultsEntry();
    //            log.Debug("Processing entry {0}", JsonSerializer.Serialize<ReferenceDescription>(entry));

    //            bre.displayName = entry.DisplayName.ToString();
    //            bre.browseName = entry.BrowseName.ToString();
    //            bre.nodeId = entry.NodeId.ToString();
    //            browseResults.Add(bre);
    //            if (entry.TypeDefinition.IdType == IdType.Opaque)
    //            {
    //                browseResults.AddRange(FlatBrowse(entry.NodeId.ToString()));
    //            }
    //        }

    //        return browseResults.ToArray();
    //    }

    //    public BrowseResultsEntry[] FlatBrowse(string nodeToBrowse = typesNode)
    //    {
    //        List<BrowseResultsEntry> browseResults = new List<BrowseResultsEntry>();

    //        foreach (ReferenceDescription entry in this.BrowseNodeReference(nodeToBrowse))
    //        {
    //            BrowseResultsEntry bre = new BrowseResultsEntry();
    //            log.Debug("Processing entry {0}", JsonSerializer.Serialize<ReferenceDescription>(entry));
    //            if (entry.NodeClass == NodeClass.Variable)
    //            {
    //                bre.displayName = entry.DisplayName.ToString();
    //                bre.browseName = entry.BrowseName.ToString();
    //                bre.nodeId = entry.NodeId.ToString();
    //                browseResults.Add(bre);
    //            }
    //            browseResults.AddRange(FlatBrowse(entry.NodeId.ToString()));
    //        }

    //        return browseResults.ToArray();
    //    }

    //    public string Browse(string nodeId = typesNode)
    //    {
    //        log.Debug("Browsing node {0}", nodeId);
    //        var results = this.BrowseNodeReference(nodeId);
    //        BrowseResultsEntry[] browseResults = new BrowseResultsEntry[results.Length];
    //        for (int i = 0; i < results.Length; i++)
    //        {
    //            browseResults[i] = new BrowseResultsEntry(
    //                results[i].DisplayName.ToString(),
    //                results[i].BrowseName.ToString(),
    //                results[i].NodeId.ToString(),
    //                results[i].TypeId,
    //                results[i].IsForward,
    //                Convert.ToUInt32(results[i].NodeClass));
    //        }
    //        return JsonSerializer.Serialize<BrowseResultsEntry[]>(browseResults);
    //    }

    //    private async void Connect(string endpoint, string cert, string key)
    //    {
    //        X509Certificate2 certificate = new X509Certificate2(Encoding.ASCII.GetBytes(cert));
    //        X509Certificate2 certWithKey = CertificateFactory.CreateCertificateWithPEMPrivateKey(certificate, Encoding.ASCII.GetBytes(key));
    //        CertificateIdentifier certificateIdentifier = new CertificateIdentifier(certWithKey);

    //        UserIdentity = new UserIdentity(certWithKey);
    //        UseSecurity = true;
    //        ApplicationCertificate = certificateIdentifier;

    //        await ConnectServer(endpoint);
    //    }
    //}
}
