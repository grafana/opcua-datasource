using Grpc.Core.Logging;
using Opc.Ua;
using Opc.Ua.Client;
using Pluginv2;
using Prediktor.UA.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace plugin_dotnet
{
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
                LastRead = DateTimeOffset.UtcNow;
            }

            internal MonitoredItem MonitoredItem { get; }
            internal IDictionary<SourceAndEventTypeKey, VariantCollection> Values { get; }

            internal OpcUAQuery Query { get; }

            internal Opc.Ua.EventFilter Filter { get; }

            internal DateTimeOffset LastRead { get; set; }
        }


        private IDictionary<NodeId, List<EventFilterValues>> _eventData = new Dictionary<NodeId, List<EventFilterValues>>();
        private Session _session;
        private Subscription _subscription;
        private ILogger _log;
        private ISubscriptionReaper _subscriptionReaper;
        private IEventDataResponse _eventDataResponse;
        private INodeCacheFactory _nodeCacheFactory;
        // Maximum interval between reads before monitored item is removed.
        private TimeSpan _maxReadInterval;
        public EventSubscription(ILogger log, ISubscriptionReaper subscriptionReaper, IEventDataResponse eventDataResponse, INodeCacheFactory nodeCacheFactory, Session session, TimeSpan maxReadInterval)
        {
            _log = log;
            _session = session;
            _subscriptionReaper = subscriptionReaper;
            _maxReadInterval = maxReadInterval;
            _eventDataResponse = eventDataResponse;
            _nodeCacheFactory = nodeCacheFactory;

            _subscriptionReaper.OnTimer += _subscriptionReaper_OnTimer;
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

        private void _subscriptionReaper_OnTimer(object sender, EventArgs e)
        {
            ReapSubscription();
        }

        private void ReapSubscription()
        {
            List<MonitoredItem> removedMonitoredItems = new List<MonitoredItem>();
            try
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                lock (_eventData)
                {
                    ICollection<NodeId> keys = _eventData.Keys;
                    foreach (NodeId key in keys)
                    {
                        if (_eventData.TryGetValue(key, out List<EventFilterValues> values))
                        {
                            for (int i = values.Count; i >= 0; i--)
                            {
                                EventFilterValues eventFilterValue = values[i];
                                TimeSpan ts = now.Subtract(eventFilterValue.LastRead);
                                if (ts.CompareTo(_maxReadInterval) > 0)
                                {
                                    values.RemoveAt(i);
                                    removedMonitoredItems.Add(eventFilterValue.MonitoredItem);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e, "Error when detecting if monitored items shall be removed");
            }

            if (removedMonitoredItems.Count > 0)
            {
                try
                {
                    _subscription.RemoveItems(removedMonitoredItems);
                }
                catch (Exception e)
                {
                    _log.Error(e, "Error when removing monitored items.");
                }
            }
        }

        public void Close()
        {
            _subscriptionReaper.OnTimer -= _subscriptionReaper_OnTimer;
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

            MonitoredItem monitorItem = CreateMonitoredItem(startNodeId, eventFilter);
            monitorItem.Notification += MonitorItem_Notification;
            _subscription.AddItem(monitorItem);
            _subscription.ApplyChanges();
            monitorItem.Subscription.ConditionRefresh();
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
                Opc.Ua.EventFilter filter = monitoredItem.Filter as Opc.Ua.EventFilter;

                if (filter != null)
                {
                    EventFilterValues values = GetEventFilterValues(monitoredItem.StartNodeId, filter);
                    if (values != null)
                    {
                        NodeId eventType = (NodeId)notification.EventFields[notification.EventFields.Count - 1].Value;
                        NodeId sourceNode = (NodeId)notification.EventFields[notification.EventFields.Count - 2].Value;
                        SourceAndEventTypeKey key = new SourceAndEventTypeKey(sourceNode, eventType);
                        lock (_eventData)
                        {
                            List<EventFilterValues> eventFilterValuesList;
                            if (_eventData.TryGetValue(monitoredItem.StartNodeId, out eventFilterValuesList))
                            {
                                EventFilterValues eventFilterValues = eventFilterValuesList.FirstOrDefault(a => a.Filter.IsEqual(filter));
                                if (eventFilterValues != null)
                                {
                                    eventFilterValues.Values[key] = notification.EventFields;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Error in monitor notification");
            }
        }

        private EventFilterValues GetEventFilterValues(NodeId startNodeId, Opc.Ua.EventFilter eventFilter)
        {
            lock (_eventData)
            {
                if (_eventData.TryGetValue(startNodeId, out List<EventFilterValues> values))
                {
                    EventFilterValues vals = values.FirstOrDefault(a => a.Filter.IsEqual(eventFilter));
                    if (vals != null)
                    {
                        vals.LastRead = DateTimeOffset.UtcNow;
                        return vals;
                    }
                }
            }
            return null;
        }

        public Result<DataResponse> GetEventData(OpcUAQuery query, NodeId startNodeId, Opc.Ua.EventFilter eventFilter)
        {
            INodeCache nodeCache = _nodeCacheFactory.Create(_session);
            eventFilter.AddSelectClause(ObjectTypeIds.BaseEventType, "SourceNode");
            eventFilter.AddSelectClause(ObjectTypeIds.BaseEventType, "EventType");

            EventFilterValues eventFilterValues = GetEventFilterValues(startNodeId, eventFilter);
            if (eventFilterValues == null)
            {
                MonitoredItem monitorItem = AddMonitorItem(startNodeId, eventFilter);
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
                    return _eventDataResponse.CreateEventSubscriptionDataResponse(eventFilterValues.Values.Values, query, nodeCache);
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
}
