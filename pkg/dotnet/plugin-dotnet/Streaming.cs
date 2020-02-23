using System;
using System.Collections.Generic;
using System.Text;
using Models;
using Pluginv2;
using Serilog;
using Grpc.Core;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using MicrosoftOpcUa.Client.Utility;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using Google.Protobuf;
using System.Globalization;

class OpcUaStreaming : StreamingPlugin.StreamingPluginBase
{
    private readonly Serilog.Core.Logger log;
    
    public OpcUaStreaming(Serilog.Core.Logger logIn)
    {
        log = logIn;
    }

    private void SubCallback(string key, MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs args)
    {
        log.Information("subscription callback {0}", key);

        MonitoredItemNotification notification = args.NotificationValue as MonitoredItemNotification;
        if (notification != null)
        {
            DataQueryResponse response = new DataQueryResponse {};
            log.Information("subscription callback {0}", notification.Value);
            QueryResult queryResult = new QueryResult();
            queryResult.RefId = key;
            var jsonResults = JsonSerializer.Serialize<DataValue>(notification.Value);
            queryResult.MetaJson = jsonResults;
        }
    }
}