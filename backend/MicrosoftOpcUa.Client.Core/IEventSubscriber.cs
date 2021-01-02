using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicrosoftOpcUa.Client.Core
{
    public interface IEventSubscriber
    {
        /// <summary>
        /// 订阅器名称 (自定义不讲究)
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 订阅节点Id （例如：ns=2;i=15014）
        /// </summary>
        string NodeId { get; set; }

        /// <summary>
        /// 订阅节点名称(例如:CPU_USAGE)
        /// </summary>
        string NodeName { get; set; }

        /// <summary>
        /// 收到订阅后的处理方法
        /// </summary>
        /// <param name="key"></param>
        /// <param name="monitoredItem"></param>
        /// <param name="args"></param>
        void EventHandler(string key, MonitoredItem monitoredItem,
            MonitoredItemNotificationEventArgs args);
    }
}
