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
    // TODO: move things into separate files...

    public interface IConnection
    {
        Session Session { get; }

        IEventSubscription EventSubscription { get; }
        IDataValueSubscription DataValueSubscription { get; }
        IEventDataResponse EventDataResponse { get; }
        void Close();
    }

    public interface IEventSubscription
    {
        void Close();
        Result<DataResponse> GetEventData( OpcUAQuery query, NodeId nodeId, Opc.Ua.EventFilter eventFilter);
    }


    public interface IDataValueSubscription
    {
        Result<DataValue>[] GetValues(NodeId[] nodeIds);

        void Close();
    }


    public interface INodeCacheFactory
    {
        INodeCache Create(Session session);
    }

    public interface INodeCache
    {
        Opc.Ua.QualifiedName GetBrowseName(NodeId nodeId);
    }

    public class NodeCacheFactory : INodeCacheFactory
    {
        private ILogger _logger;
        public NodeCacheFactory(ILogger logger)
        {
            _logger = logger;
        }
        public INodeCache Create(Session session)
        {
            return new NodeCache(_logger, session);
        }
    }


    public class NodeCache : INodeCache
    {
        private ILogger _logger;
        private Session _session;
        private Dictionary<NodeId, Dictionary<uint, object>> _nodeAttributes = new Dictionary<NodeId, Dictionary<uint, object>>();
        public NodeCache(ILogger logger, Session session)
        {
            _logger = logger;
            _session = session;
        }

        private T GetAttributeValue<T>(NodeId nodeId, uint attributeId, T defaultValue) where T : class
        {
            
            lock (_nodeAttributes)
            {
                Dictionary<uint, object> node = null;
                if (!_nodeAttributes.TryGetValue(nodeId, out node))
                {
                    node = new Dictionary<uint, object>();
                    _nodeAttributes.Add(nodeId, node);
                }
                if (node.TryGetValue(attributeId, out object value))
                {
                    if (value is T)
                        return (T)value;
                    else
                        node.Remove(attributeId);
                }
            }

            try
            {
                var readRes = _session.ReadAttributes(nodeId, new[] { attributeId });
                if (readRes[0].Success)
                {
                    var value = readRes[0].Value;
                    if (value != null && value is T)
                    {
                        lock (_nodeAttributes)
                        {
                            Dictionary<uint, object> node = null;
                            if (!_nodeAttributes.TryGetValue(nodeId, out node))
                            {
                                node = new Dictionary<uint, object>();
                                _nodeAttributes.Add(nodeId, node);
                            }

                            if (!node.ContainsKey(attributeId))
                                node.Add(attributeId, value);
                        }
                        return (T)value;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error reading node: " + nodeId);
            }
            return defaultValue;
        }


        public Opc.Ua.QualifiedName GetBrowseName(NodeId nodeId)
        {
            return GetAttributeValue<Opc.Ua.QualifiedName>(nodeId, Attributes.BrowseName, null);
        }
    }


    public class Connection : IConnection
    {
        private bool _closed = false;
        private ISubscriptionReaper _subscriptionReaper;
        public Connection(Session session, IEventSubscription eventSubscription, IDataValueSubscription dataValueSubscription, ISubscriptionReaper subscriptionReaper, IEventDataResponse eventDataResponse)
        {
            _subscriptionReaper = subscriptionReaper;
            Session = session;
            EventSubscription = eventSubscription;
            DataValueSubscription = dataValueSubscription;
            EventDataResponse = eventDataResponse;
        }

        public Session Session { get; }

        public IEventSubscription EventSubscription { get; }

        public IDataValueSubscription DataValueSubscription { get; }

        public IEventDataResponse EventDataResponse { get; }

        public INodeCache NodeCache { get; }

        public void Close()
        {
            lock (this)
            {
                if (!_closed)
                {
                    _closed = true;
                    _subscriptionReaper.Stop();
                    _subscriptionReaper.Dispose();
                    DataValueSubscription.Close();
                    EventSubscription.Close();
                    Session.Close();
                }
            }
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
        private INodeCacheFactory _nodeCacheFactory;
        private Func<ApplicationConfiguration> _applicationConfiguration;
        private Dictionary<string, IConnection> connections = new Dictionary<string, IConnection>();
        public Connections(ILogger log, ISessionFactory sessionFactory, INodeCacheFactory nodeCacheFactory, Func<ApplicationConfiguration> applicationConfiguration)
        {
            _log = log;
            _sessionFactory = sessionFactory;
            _nodeCacheFactory = nodeCacheFactory;
            _applicationConfiguration = applicationConfiguration;
        }


        private void CreateConnection(string url, Session session)
        {
            var subscriptionReaper = new SubscriptionReaper(_log, 60000);

            var eventDataResponse = new EventDataResponse(_log);
            var eventSubscription = new EventSubscription(_log, subscriptionReaper, eventDataResponse, _nodeCacheFactory, session, new TimeSpan(0, 60, 0));
            var dataValueSubscription = new DataValueSubscription(_log, subscriptionReaper, session, new TimeSpan(0, 10, 0));

            lock (connections)
            {
                var conn = new Connection(session, eventSubscription, dataValueSubscription, subscriptionReaper, eventDataResponse);
                connections[key: url] = conn;
                subscriptionReaper.Start();
            }
        }

        public void Add(string url, string clientCert, string clientKey)
        {
            try
            {
                _log.LogInformation("Creating session: {0}", url);
                //connections[key: url] = new OpcUAConnection(url, clientCert, clientKey);
                X509Certificate2 certificate = new X509Certificate2(Encoding.ASCII.GetBytes(clientCert));
                X509Certificate2 certWithKey = CertificateFactory.CreateCertificateWithPEMPrivateKey(certificate, Encoding.ASCII.GetBytes(clientKey));
                CertificateIdentifier certificateIdentifier = new CertificateIdentifier(certWithKey);

                var userIdentity = new UserIdentity(certWithKey);


                var appConfig = _applicationConfiguration();
                appConfig.SecurityConfiguration.ApplicationCertificate = certificateIdentifier;
                var session = _sessionFactory.CreateSession(url, "Grafana Session", userIdentity, true, false, appConfig);
                CreateConnection(url, session);
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
                _log.LogInformation("Creating anonymous session: {0}", url);
                var session = _sessionFactory.CreateAnonymously(url, "Grafana Anonymous Session", false, false, _applicationConfiguration());
                CreateConnection(url, session);
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

                bool connect = true;
                if (connections.TryGetValue(settings.Url, out IConnection value))
                {
                    connect = false;
                    if (!value.Session.Connected || value.Session.KeepAliveStopped)
                    {
                        try
                        {
                            _log.LogInformation("Closing old session due to stale connection. Was connected {0}. KeepAliveStopped {1}", 
                                value.Session.Connected, value.Session.KeepAliveStopped);
                            value.Close();
                            connections.Remove(settings.Url);
                            _log.LogInformation("Old session closed successfully");
                        }
                        catch (Exception e)
                        {
                            _log.LogWarning(e, "Error when closing connection.");
                        }
                        connect = true;
                    }
                }

                if (connect)
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
