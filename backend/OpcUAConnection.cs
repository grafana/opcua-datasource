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
using Grpc.Core.Logging;

namespace plugin_dotnet
{
    // TODO: move things into separate files...

    public enum OPCTimestamp {
        Server,
        Source,
    }

    public interface IConnection
    {
        Session Session { get; }

        OPCTimestamp TimestampSource { get; }

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
                Result<object>[] readRes = _session.ReadAttributes(nodeId, new[] { attributeId });
                if (readRes[0].Success)
                {
                    object value = readRes[0].Value;
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
                _logger.Error(e, "Error reading node: " + nodeId);
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
        public Connection(Session session, Settings settings, IEventSubscription eventSubscription, IDataValueSubscription dataValueSubscription, ISubscriptionReaper subscriptionReaper, IEventDataResponse eventDataResponse)
        {
            _subscriptionReaper = subscriptionReaper;
            Session = session;
            EventSubscription = eventSubscription;
            DataValueSubscription = dataValueSubscription;
            EventDataResponse = eventDataResponse;
            TimestampSource = settings.TimestampSource;
        }

        public Session Session { get; }

        public IEventSubscription EventSubscription { get; }

        public IDataValueSubscription DataValueSubscription { get; }

        public IEventDataResponse EventDataResponse { get; }

        public INodeCache NodeCache { get; }

        public OPCTimestamp TimestampSource { get; }

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
        void Add(Settings settings);
        IConnection Get(Settings settings);
        void Remove(Settings settings);
    }

    public class Connections : IConnections
    {
        private ILogger _log;
        private Prediktor.UA.Client.ISessionFactory _sessionFactory;
        private INodeCacheFactory _nodeCacheFactory;
        private Func<ApplicationConfiguration> _applicationConfiguration;
        private Dictionary<string, IConnection> connections = new Dictionary<string, IConnection>();
        public Connections(ILogger log, Prediktor.UA.Client.ISessionFactory sessionFactory, INodeCacheFactory nodeCacheFactory, Func<ApplicationConfiguration> applicationConfiguration)
        {
            _log = log;
            _sessionFactory = sessionFactory;
            _nodeCacheFactory = nodeCacheFactory;
            _applicationConfiguration = applicationConfiguration;
        }


        private void CreateConnection(Settings settings, Session session)
        {
            SubscriptionReaper subscriptionReaper = new SubscriptionReaper(_log, 60000);

            EventDataResponse eventDataResponse = new EventDataResponse(_log);
            EventSubscription eventSubscription = new EventSubscription(_log, subscriptionReaper, eventDataResponse, _nodeCacheFactory, session, new TimeSpan(0, 60, 0));
            DataValueSubscription dataValueSubscription = new DataValueSubscription(_log, subscriptionReaper, session, new TimeSpan(0, 10, 0));

            lock (connections)
            {
                Connection conn = new Connection(session, settings, eventSubscription, dataValueSubscription, subscriptionReaper, eventDataResponse);
                connections[key: settings.ID] = conn;
                subscriptionReaper.Start();
            }
        }

        public void Add(Settings settings)
        {
            try
            {
                Session session;
                if (settings.TLSClientCert != null && settings.TLSClientKey != null) {
                    _log.Info("Creating session: {0}", settings.URL);
                    //connections[key: url] = new OpcUAConnection(url, clientCert, clientKey);
                    X509Certificate2 certificate = new X509Certificate2(Encoding.ASCII.GetBytes(settings.TLSClientCert));
                    X509Certificate2 certWithKey = CertificateFactory.CreateCertificateWithPEMPrivateKey(certificate, Encoding.ASCII.GetBytes(settings.TLSClientKey));
                    CertificateIdentifier certificateIdentifier = new CertificateIdentifier(certWithKey);

                    UserIdentity userIdentity = new UserIdentity(certWithKey);


                    ApplicationConfiguration appConfig = _applicationConfiguration();
                    appConfig.SecurityConfiguration.ApplicationCertificate = certificateIdentifier;
                    session = _sessionFactory.CreateSession(settings.URL, "Grafana Session", userIdentity, true, false, appConfig);
                    CreateConnection(settings, session);
                    return;
                }

                _log.Info("Creating anonymous session: {0}", settings.URL);
                session = _sessionFactory.CreateAnonymously(settings.URL, "Grafana Anonymous Session", false, false, _applicationConfiguration());
                CreateConnection(settings, session); 
            }
            catch (Exception ex)
            {
                _log.Error("Error while adding endpoint {0}: {1}", settings.URL, ex);
            }
        }

        public IConnection Get(Settings settings)
        {
            _log.Debug("We have a connection request with settings {0}", settings);

            lock (connections)
            {

                bool connect = true;
                if (connections.TryGetValue(settings.URL, out IConnection value))
                {
                    connect = false;
                    if (!value.Session.Connected || value.Session.KeepAliveStopped)
                    {
                        try
                        {
                            _log.Info("Closing old session due to stale connection. Was connected {0}. KeepAliveStopped {1}", 
                                value.Session.Connected, value.Session.KeepAliveStopped);
                            value.Close();
                            connections.Remove(settings.URL);
                            _log.Info("Old session closed successfully");
                        }
                        catch (Exception e)
                        {
                            _log.Warning(e, "Error when closing connection.");
                        }
                        connect = true;
                    }
                }

                if (connect)
                {
                    Add(settings);
                }

                return connections[settings.ID];
            }
        }

        public void Remove(Settings settings)
        {
            lock (connections)
            {
                connections.Remove(settings.ID);
            }
        }
    }
}
