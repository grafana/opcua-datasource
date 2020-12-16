using MicrosoftOpcUa.Client.Core;
using Opc.Ua;
using Opc.Ua.Client;
using System;

namespace MicrosoftOpcUa.Client.Subscribers
{
    public class PerformanceSubscriber : IEventSubscriber
    {
        public string NodeId { get; set; } = "ns=2;i=15014";
        public string NodeName { get; set; } = "CPU_Usage";
        public string Name { get; set; } = "Subscribers";
        public string Tag { get; set; } = string.Empty;

        public void EventHandler(string key, MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs args)
        {
            if (key == "CPU_Usage")
            {
                if (args.NotificationValue is MonitoredItemNotification notification)
                {
                    Console.WriteLine(
                        $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff")}] CPU_Usage:"
                        + notification.Value.WrappedValue.Value.ToString() + "%");
                }
                return;
            }
        }
    }
}
