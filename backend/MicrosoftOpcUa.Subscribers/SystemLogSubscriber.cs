using MicrosoftOpcUa.Client.Core;
using MicrosoftOpcUa.Client.Utility;
using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace MicrosoftOpcUa.Client.Subscribers
{
    public class SystemLogSubscriber : IEventSubscriber
    {
        public string Name { get; set; } = "Subscribers";
        public string NodeId { get; set; } = "ns=2;i=15022";
        public string NodeName { get; set; } = "LogLine";

        public void EventHandler(string key,
            MonitoredItem monitoredItem,
            MonitoredItemNotificationEventArgs args)
        {
            if (args.NotificationValue is MonitoredItemNotification notification &&
               notification.Value != null && notification.Value.WrappedValue.Value != null)
            {
                var text = notification.Value.WrappedValue.Value.ToString();
                text = Base64.DecodeBase64(text);
                var data = text.Split("|");
                var index = long.Parse(data[0]);
                Utility.Screen.Log($"File {index}.part.log received.", ConsoleColor.Blue);
                File.WriteAllLines("E:/received/" + index + ".part.log", data);
            }
        }
    }
}
