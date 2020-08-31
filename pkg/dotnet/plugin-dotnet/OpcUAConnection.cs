using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Pluginv2;
using Prediktor.UA.Client;
using Microsoft.Extensions.Logging;

namespace plugin_dotnet
{
    public interface IConnection
    {
        Session Session { get; }

        IEventSubscription EventSubscription { get; }
        IDataValueSubscription DataValueSubscription { get; }

        void Close();
    }

    public interface IEventSubscription
    {
        void Close();
        Result<DataResponse> GetEventData(OpcUAQuery query, NodeId nodeId, Opc.Ua.EventFilter eventFilter);
    }


    public interface IDataValueSubscription
    {
        Result<DataValue>[] GetValues(NodeId[] nodeIds);

        void Close();
    }



    public class Connection : IConnection
    {
        public Connection(Session session, IEventSubscription eventSubscription, IDataValueSubscription dataValueSubscription)
        {
            Session = session;
            EventSubscription = eventSubscription;
            DataValueSubscription = dataValueSubscription;
        }

        public Session Session { get; }

        public IEventSubscription EventSubscription { get; }

        public IDataValueSubscription DataValueSubscription { get; }

        public void Close()
        {
            DataValueSubscription.Close();
            EventSubscription.Close();
            Session.Close();
        }
    }



    public interface IConnections
    {
        void Add(string url, string clientCert, string clientKey);
        void Add(string url);
        IConnection Get(DataSourceInstanceSettings settings);
        IConnection Get(string url);

        void Remove(string url);
    }

    public class Connections : IConnections
    {
        private ILogger _log;
        private ISessionFactory _sessionFactory;
        private Func<ApplicationConfiguration> _applicationConfiguration;
        private Dictionary<string, IConnection> connections = new Dictionary<string, IConnection>();
        public Connections(ILogger log, ISessionFactory sessionFactory, Func<ApplicationConfiguration> applicationConfiguration)
        {
            _log = log;
            _sessionFactory = sessionFactory;
            _applicationConfiguration = applicationConfiguration;
        }

        public void Add(string url, string clientCert, string clientKey)
        {
            try
            {
                //connections[key: url] = new OpcUAConnection(url, clientCert, clientKey);
                X509Certificate2 certificate = new X509Certificate2(Encoding.ASCII.GetBytes(clientCert));
                X509Certificate2 certWithKey = CertificateFactory.CreateCertificateWithPEMPrivateKey(certificate, Encoding.ASCII.GetBytes(clientKey));
                CertificateIdentifier certificateIdentifier = new CertificateIdentifier(certWithKey);

                var userIdentity = new UserIdentity(certWithKey);


                var appConfig = _applicationConfiguration();
                appConfig.SecurityConfiguration.ApplicationCertificate = certificateIdentifier;
                var session = _sessionFactory.CreateSession(url, "Grafana Session", userIdentity, true, appConfig);
                var eventSubscription = new EventSubscription(_log, session);
                var dataValueSubscription = new DataValueSubscription(_log, session);
                lock (connections)
                {
                    var conn = new Connection(session, eventSubscription, dataValueSubscription);
                    connections[key: url] = conn;
                }
            }
            catch (Exception ex)
            {
                _log.LogError("Error while adding endpoint {0}: {1}", url, ex);
            }
        }

        public void Add(string url)
        {
            try
            {
                var session = _sessionFactory.CreateAnonymously(url, "Grafana Anonymous Session", false, _applicationConfiguration());
                var eventSubscription = new EventSubscription(_log, session);
                var dataValueSubscription = new DataValueSubscription(_log, session);
                lock (connections)
                {
                    var conn = new Connection(session, eventSubscription, dataValueSubscription);
                    connections[key: url] = conn;
                }
            }
            catch (Exception ex)
            {
                _log.LogError("Error while adding endpoint {0}: {1}", url, ex);
            }
        }


        public IConnection Get(string url)
        {
            lock (connections)
            {

                if (!connections.ContainsKey(url) || !connections[url].Session.Connected)
                {
                    Add(url);
                }

                return connections[url];
            }
        }

        public IConnection Get(DataSourceInstanceSettings settings)
        {
            lock (connections)
            {

                if (!connections.ContainsKey(settings.Url) || !connections[settings.Url].Session.Connected)
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
}
