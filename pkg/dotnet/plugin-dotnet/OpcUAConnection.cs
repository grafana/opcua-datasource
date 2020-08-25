using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Grpc.Core.Logging;
//using MicrosoftOpcUa.Client.Core;
//using MicrosoftOpcUa.Client.Utility;
using Opc.Ua;
using Opc.Ua.Client;
using Pluginv2;
using Prediktor.UA.Client;

namespace plugin_dotnet
{
    public interface IConnection
    { 
        Session Session { get; }

        IEventSubscription EventSubscription { get; }

        void Close();
    }

    public class Connection : IConnection
    {
        public Connection(Session session, IEventSubscription eventSubscription)
        {
            Session = session;
            EventSubscription = eventSubscription;
        }

        public Session Session { get; }

        public IEventSubscription EventSubscription { get; }

        public void Close()
        {
            EventSubscription.Close();
            Session.Close();
        }
    }

    public interface IEventSubscription
    {
        void Close();
        Result<DataResponse> GetEventData(OpcUAQuery query, NodeId nodeId, Opc.Ua.EventFilter eventFilter);
    }

    public class EventSubscription : IEventSubscription
    {
        internal class SourceAndEventTypeKey
        {

            public SourceAndEventTypeKey(NodeId sourceNode, NodeId eventType)
            {
                SourceNode = sourceNode;
                EventType = eventType;
            }

            public NodeId SourceNode { get; }
            public NodeId EventType { get; }

            public override int GetHashCode()
            {
                return SourceNode.GetHashCode() + 31 * EventType.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                SourceAndEventTypeKey other = obj as SourceAndEventTypeKey;
                if (other != null)
                {
                    return SourceNode.Equals(other.SourceNode) && EventType.Equals(other.EventType);
                }
                return base.Equals(obj);
            }

        }

        internal class EventFilterValues
        {
            internal EventFilterValues(MonitoredItem monitoredItem, OpcUAQuery query, Opc.Ua.EventFilter filter)
            {
                MonitoredItem = monitoredItem;
                Query = query;
                Filter = filter;
                Values = new Dictionary<SourceAndEventTypeKey, VariantCollection>();
            }

            internal MonitoredItem MonitoredItem { get; }
            internal IDictionary<SourceAndEventTypeKey, VariantCollection> Values { get; }

            internal OpcUAQuery Query { get; }

            internal Opc.Ua.EventFilter Filter { get; }
        }


        private IDictionary<NodeId, List<EventFilterValues>> _eventData = new Dictionary<NodeId, List<EventFilterValues>>();
        private Session _session;
        private Subscription _subscription;
        public EventSubscription(Session session)
        {
            _session = session;
            // Hard coded for now
            _subscription = new Subscription();
            _subscription.DisplayName = null;
            _subscription.PublishingInterval = 1000;
            _subscription.KeepAliveCount = 10;
            _subscription.LifetimeCount = 100;
            _subscription.MaxNotificationsPerPublish = 1000;
            _subscription.PublishingEnabled = true;
            _subscription.TimestampsToReturn = TimestampsToReturn.Both;
            _session.AddSubscription(_subscription);
            _subscription.Create();
        }

        public void Close()
        {
            _session.RemoveSubscription(_subscription);
            _subscription.Dispose();
        }


        private MonitoredItem CreateMonitoredItem(NodeId startNodeId, Opc.Ua.EventFilter eventFilter)
        {

            // create the item with the filter.
            MonitoredItem monitoredItem = new MonitoredItem();

            monitoredItem.DisplayName = null;
            monitoredItem.StartNodeId = startNodeId;
            monitoredItem.RelativePath = null;
            monitoredItem.NodeClass = NodeClass.Object;
            monitoredItem.AttributeId = Attributes.EventNotifier;
            monitoredItem.IndexRange = null;
            monitoredItem.Encoding = null;
            monitoredItem.MonitoringMode = MonitoringMode.Reporting;
            monitoredItem.SamplingInterval = 0;
            monitoredItem.QueueSize = UInt32.MaxValue;
            monitoredItem.DiscardOldest = true;
            monitoredItem.Filter = eventFilter;

            // save the definition as the handle.
            //monitoredItem.Handle = this;

            return monitoredItem;

        }


        private MonitoredItem AddMonitorItem(NodeId startNodeId, Opc.Ua.EventFilter eventFilter)
        {

            var monitorItem = CreateMonitoredItem(startNodeId, eventFilter);
            monitorItem.Notification += MonitorItem_Notification;
            _subscription.AddItem(monitorItem);
            _subscription.ApplyChanges();
            return monitorItem;
        }


        private void RemoveMonitorItem(MonitoredItem monitorItem)
        {
            monitorItem.Notification -= MonitorItem_Notification;
            _subscription.RemoveItem(monitorItem);
            _subscription.ApplyChanges();
        }

        private void MonitorItem_Notification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {

            try
            {
                EventFieldList notification = e.NotificationValue as EventFieldList;

                if (notification == null)
                {
                    return;
                }
                var filter = monitoredItem.Filter as Opc.Ua.EventFilter;

                if (filter != null)
                {
                    var values = GetEventFilterValues(monitoredItem.StartNodeId, filter);
                    if (values != null)
                    {
                        var eventType = (NodeId)notification.EventFields[notification.EventFields.Count - 1].Value;
                        var sourceNode = (NodeId)notification.EventFields[notification.EventFields.Count - 2].Value;
                        var key = new SourceAndEventTypeKey(sourceNode, eventType);
                        lock (_eventData)
                        {
                            List<EventFilterValues> eventFilterValuesList;
                            if (_eventData.TryGetValue(monitoredItem.StartNodeId, out eventFilterValuesList))
                            {
                                var eventFilterValues = eventFilterValuesList.FirstOrDefault(a => a.Filter.IsEqual(filter));
                                if (eventFilterValues != null)
                                {
                                    eventFilterValues.Values[key] = notification.EventFields;
                                }
                            }
                        }
                    }
                }
            }
            catch //(Exception exception)
            { 
                // Log.
            }
        }

        private EventFilterValues GetEventFilterValues(NodeId startNodeId, Opc.Ua.EventFilter eventFilter)
        {
            lock (_eventData)
            {
                if (_eventData.TryGetValue(startNodeId, out List<EventFilterValues> values))
                {
                    return values.FirstOrDefault(a => a.Filter.IsEqual(eventFilter));
                }
            }
            return null;
        }

        public Result<DataResponse> GetEventData(OpcUAQuery query, NodeId startNodeId, Opc.Ua.EventFilter eventFilter)
        {
            eventFilter.AddSelectClause(ObjectTypeIds.BaseEventType, "SourceNode");
            eventFilter.AddSelectClause(ObjectTypeIds.BaseEventType, "EventType");

            var eventFilterValues = GetEventFilterValues(startNodeId, eventFilter);
            if (eventFilterValues == null)
            {
                var monitorItem = AddMonitorItem(startNodeId, eventFilter);
                eventFilterValues = new EventFilterValues(monitorItem, query, eventFilter);
                if (!TryAddEventFilterValues(startNodeId, eventFilterValues))
                {
                    RemoveMonitorItem(monitorItem);
                    eventFilterValues = null;
                }
            }

            lock (_eventData)
            {
                if (eventFilterValues == null)
                {
                    eventFilterValues = GetEventFilterValues(startNodeId, eventFilter);
                }
                if (eventFilterValues != null)
                {
                    return Converter.CreateEventSubscriptionDataResponse(eventFilterValues.Values.Values, query);
                }
            }
            return new Result<DataResponse>(StatusCodes.BadUnexpectedError, "");
        }

        private bool TryAddEventFilterValues(NodeId startNodeId, EventFilterValues eventFilterValues)
        {
            lock (_eventData)
            {
                List<EventFilterValues> l;
                if (!_eventData.TryGetValue(startNodeId, out l))
                {
                    l = new List<EventFilterValues>();
                    l.Add(eventFilterValues);
                    _eventData.Add(startNodeId, l);
                    return true;
                }
                else
                {
                    if (!l.Any(a => a.Filter.IsEqual(eventFilterValues.Filter)))
                    {
                        l.Add(eventFilterValues);
                        return true;
                    }
                }
            }
            return false;
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
        private ISessionFactory _sessionFactory;
        private Func<ApplicationConfiguration> _applicationConfiguration;
        private Dictionary<string, IConnection> connections = new Dictionary<string, IConnection>();
        public Connections(ISessionFactory sessionFactory, Func<ApplicationConfiguration> applicationConfiguration)
        {
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
                var eventSubscription = new EventSubscription(session);
                lock (connections)
                {
                    var conn = new Connection(session, eventSubscription);
                    connections[key: url] = conn;
                }
            }
            catch (Exception ex)
            {
                //log.Debug("Error while adding endpoint {0}: {1}", url, ex);
            }
        }

        public void Add(string url)
        {
            try
            {
                var session = _sessionFactory.CreateAnonymously(url, "Grafana Anonymous Session", false, _applicationConfiguration());
                var eventSubscription = new EventSubscription(session);
                lock (connections)
                {
                    var conn = new Connection(session, eventSubscription);
                    connections[key: url] = conn;
                }
            }
            catch (Exception ex)
            {
                //log.Debug("Error while adding endpoint {0}: {1}", url, ex);
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
