using MicrosoftOpcUa.Client.Core;
using MicrosoftOpcUa.Client.Utility;
using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicrosoftOpcUa.Client.Subscribers
{
    public class SystemLogCountSubscriber : IEventSubscriber
    {
        public string Name { get; set; } = "Subscribers";
        public string NodeId { get; set; } = "ns=2;i=15024";
        public string NodeName { get; set; } = "LogCount";

        public SystemLogCountSubscriber()
        {

        }

        public void EventHandler(string key, MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs args)
        {
            if (args.NotificationValue is MonitoredItemNotification notification &&
               notification.Value != null && notification.Value.WrappedValue.Value != null)
            {
                var text = notification.Value.WrappedValue.Value.ToString();
                var count = long.Parse(text);
                GetLostedFile(count);
            }
        }


        private void GetLostedFile(long count = 0)
        {
            DirectoryInfo directory = new DirectoryInfo("E:/received/");
            var files = directory.GetFiles();
            var fileNames = files.Select(p => p.Name);

            for (int i = 0; i < count; i++)
            {
                if (!fileNames.Contains($"{i}.part.log"))
                {
                    Utility.Screen.Log($"Lost File:{i}.part.log, Receving...", ConsoleColor.Yellow);
                    SubscriberManager.OpcUaClient.WriteNode<string>("ns=2;i=15023", i.ToString());
                    Thread.Sleep(1000);
                }
            }
            if (fileNames.Count() == count)
            {
                Utility.Screen.Log($"All File Reveiced.", ConsoleColor.Green);
            }
            Thread.Sleep(5000);
            var value = SubscriberManager.OpcUaClient.ReadNode<string>(NodeId);
            long.TryParse(value, out count);
            if (count > 0)
            {
                GetLostedFile(count);
            }
        }
    }
}
