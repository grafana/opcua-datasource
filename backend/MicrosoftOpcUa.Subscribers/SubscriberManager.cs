using MicrosoftOpcUa.Client.Core;
using MicrosoftOpcUa.Client.Utility;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicrosoftOpcUa.Client.Subscribers
{
    public static class SubscriberManager
    {
        public static OpcUaClient OpcUaClient;

        public static void RegistSubscriber()
        {
            OpcUaClient.Subscrib(new SystemLogSubscriber());
            OpcUaClient.Subscrib(new PerformanceSubscriber());
            OpcUaClient.Subscrib(new SystemLogCountSubscriber());
        }
 
        private static void Subscrib(this OpcUaClient client, IEventSubscriber subscriber)
        {

            client.AddSubscription(subscriber.NodeName, subscriber.NodeId,
               (key, item, args) =>
               {
                   try
                   {
                       if (args.NotificationValue is MonitoredItemNotification notification &&
                notification.Value != null && notification.Value.WrappedValue.Value != null)
                       {
                           if (key == subscriber.NodeName)
                           {
                               subscriber.EventHandler(key, item, args);
                           }
                       }

                   }
                   catch (Exception ex)
                   {
                       Screen.Log(ex.Message, ConsoleColor.Red);
                   }

               });
            Screen.Log($"Subscriber: {subscriber.NodeName} registed.",
                ConsoleColor.DarkGreen);
        }


    }
}
