using Opc.Ua;
using Opc.Ua.Client;
using Prediktor.UA.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace plugin_dotnet
{

    public class VariableValue
    { 
        public DataValue Value { get; set; }
        public DateTimeOffset LastRead { get; set; }
    }

    public class DataValueSubscription : IDataValueSubscription
    {
        private Session _session;
        private Subscription _subscription;
        private Dictionary<NodeId, VariableValue> _subscribedValues = new Dictionary<NodeId, VariableValue>();


        public DataValueSubscription(Session session)
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
            monitoredItem.QueueSize = UInt32.MaxValue;
            monitoredItem.DiscardOldest = true;
            //monitoredItem.Handle = 
            return monitoredItem;

        }
        private void Subscribe(NodeId[] nodeIds)
        {
            var monItems = new List<MonitoredItem>();
            lock (_subscribedValues)
            {
                for (int i = 0; i < nodeIds.Length; i++)
                {
                    if (!_subscribedValues.ContainsKey(nodeIds[i]))
                    { 
                        var monitoredItem = CreateMonitoredItem(nodeIds[i]);
                        monitoredItem.Notification += MonitoredItem_Notification;
                        monItems.Add(monitoredItem);
                        _subscribedValues.Add(nodeIds[i], new VariableValue() { LastRead = DateTimeOffset.UtcNow, Value = new DataValue(Variant.Null) });
                    }
                }
            }
            if (monItems.Count > 0)
            {
                _subscription.AddItems(monItems);
                _subscription.ApplyChanges();
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
            var result = new Result<DataValue>[nodeIds.Length];
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
            _subscription.Dispose();
        }
    }
}
