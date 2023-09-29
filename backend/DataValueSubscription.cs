using Grpc.Core.Logging;
using Opc.Ua;
using Opc.Ua.Client;
using Prediktor.UA.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace plugin_dotnet
{

    public class VariableValue
    {
        public VariableValue(MonitoredItem monitoredItem)
        {
            MonitoredItem = monitoredItem;
        }

        public DataValue Value { get; set; }
        public DateTimeOffset LastRead { get; set; }

        public MonitoredItem MonitoredItem { get; }
    }

    public class DataValueSubscription : IDataValueSubscription
    {
        private ILogger _logger;
        private Session _session;
        private Subscription _subscription;
        private Dictionary<NodeId, VariableValue> _subscribedValues = new Dictionary<NodeId, VariableValue>();
        private TimeSpan _maxReadInterval;
        private ISubscriptionReaper _subscriptionReaper;


        public DataValueSubscription(ILogger logger, ISubscriptionReaper subscriptionReaper, Session session, TimeSpan maxReadInterval)
        {
            _logger = logger;
            _session = session;
            _subscriptionReaper = subscriptionReaper;
            _maxReadInterval = maxReadInterval;

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
            ReapSubscribedValues();
        }

        private MonitoredItem CreateMonitoredItem(NodeId nodeId)
        {
            MonitoredItem monitoredItem = new MonitoredItem();

            monitoredItem.DisplayName = null;
            monitoredItem.StartNodeId = nodeId;
            monitoredItem.RelativePath = null;
            monitoredItem.NodeClass = NodeClass.Variable;
            monitoredItem.AttributeId = Attributes.Value;
            monitoredItem.IndexRange = null;
            monitoredItem.Encoding = null;
            monitoredItem.MonitoringMode = MonitoringMode.Reporting;
            monitoredItem.SamplingInterval = 0;
            monitoredItem.QueueSize = uint.MaxValue;
            monitoredItem.DiscardOldest = true;
            //monitoredItem.Handle = 
            return monitoredItem;

        }


        private void ReapSubscribedValues()
        {
            List<MonitoredItem> removedMonitoredItems = new List<MonitoredItem>();
            try 
            { 
                DateTimeOffset now = DateTimeOffset.UtcNow;
                lock (_subscribedValues)
                {
                    Dictionary<NodeId, VariableValue>.KeyCollection keys = _subscribedValues.Keys;
                    foreach (NodeId key in keys)
                    {
                        if (_subscribedValues.TryGetValue(key, out VariableValue value))
                        {
                            TimeSpan ts = now.Subtract(value.LastRead);
                            if (ts.CompareTo(_maxReadInterval) > 0)
                            {
                                _subscribedValues.Remove(key);
                                removedMonitoredItems.Add(value.MonitoredItem);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error when detecting if monitored items shall be removed");
            }

            if (removedMonitoredItems.Count > 0)
            {
                try
                {
                    _subscription.RemoveItems(removedMonitoredItems);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error when removing monitored items.");
                }
            }
        }

        private void Subscribe(NodeId[] nodeIds)
        {
            List<MonitoredItem> monItems = new List<MonitoredItem>();
            List<NodeId> newNodeIds = new List<NodeId>();
            lock (_subscribedValues)
            {
                for (int i = 0; i < nodeIds.Length; i++)
                {
                    if (!_subscribedValues.ContainsKey(nodeIds[i]))
                    { 
                        MonitoredItem monitoredItem = CreateMonitoredItem(nodeIds[i]);
                        monitoredItem.Notification += MonitoredItem_Notification;
                        monItems.Add(monitoredItem);
                        newNodeIds.Add(nodeIds[i]);
                        _subscribedValues.Add(nodeIds[i], new VariableValue(monitoredItem) { LastRead = DateTimeOffset.UtcNow, Value = new DataValue(Variant.Null) });
                    }
                }
            }
            if (monItems.Count > 0)
            {
                // Force read of current value.
                InitializeNodeValues(nodeIds);
                _logger.Info("Subscribing to {0} data values", monItems.Count);
                _subscription.AddItems(monItems);
                _subscription.ApplyChanges();
            }
        }


        private void InitializeNodeValues(NodeId[] nodeIds)
        {
            try
            {
                DataValue[] dataValues = _session.ReadNodeValues(nodeIds);
                lock (_subscribedValues)
                {
                    for (int i = 0; i < dataValues.Length; i++)
                    {
                        if (StatusCode.IsGood(dataValues[i].StatusCode))
                        {
                            if (_subscribedValues.TryGetValue(nodeIds[i], out VariableValue value))
                            {
                                value.Value = dataValues[i];
                            }
                        }
                        else
                        {
                            _logger.Warning("Could not read node value for node: {0}. StatusCode: {1}", nodeIds[i], dataValues[i].StatusCode);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error reading node values for initialization");
            }
        }

        private void MonitoredItem_Notification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            lock (_subscribedValues)
            {
                if (_subscribedValues.TryGetValue(monitoredItem.StartNodeId, out VariableValue value))
                {
                    MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;
                    value.Value = notification.Value;
                }
            }
        }

        public Result<DataValue>[] GetValues(NodeId[] nodeIds)
        {
            Subscribe(nodeIds);
            Result<DataValue>[] result = new Result<DataValue>[nodeIds.Length];
            lock (_subscribedValues)
            {
                for (int i = 0; i < nodeIds.Length; i++)
                {
                    if (_subscribedValues.TryGetValue(nodeIds[i], out VariableValue value))
                    {
                        value.LastRead = DateTimeOffset.UtcNow;
                        result[i] = new Result<DataValue>(value.Value);
                    }
                    else
                    {
                        result[i] = new Result<DataValue>(StatusCodes.BadNodeIdInvalid, string.Format("Could not find node id {0} in subscription list", nodeIds[i]));
                    }
                }
            }
            return result;
        }

        public void Close()
        {
            _subscriptionReaper.OnTimer -= _subscriptionReaper_OnTimer;
            _logger.Info("Disposing DataValueSubscription");
            _subscription.Dispose();
        }
    }
}
