using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Logging;
//using MicrosoftOpcUa.Client.Core;
using Prediktor.UA.Client;
using Opc.Ua.Client;
using Pluginv2;

namespace plugin_dotnet
{
    public class ResourceService : Resource.ResourceBase
    {
        private IConnections _connections;
        private ILogger log;
        private IServerStreamWriter<CallResourceResponse> responseStream = null;

        public ResourceService(IConnections connections)
        {
            _connections = connections;
            log = new ConsoleLogger();
        }

        public override Task CallResource(CallResourceRequest request, IServerStreamWriter<CallResourceResponse> responseStream, ServerCallContext context)
<<<<<<< HEAD:backend/ResourceService.cs
        {            
            CallResourceResponse response = new CallResourceResponse();
            try
            {
                log.Debug("Call Resource {0} | {1}", request, context);
                string fullUrl = HttpUtility.UrlDecode(request.PluginContext.DataSourceInstanceSettings.Url + "/" + request.Url);
                Uri uri = new Uri(fullUrl);
                NameValueCollection queryParams = HttpUtility.ParseQueryString(uri.Query);
                var connection = _connections.Get(request.PluginContext.DataSourceInstanceSettings);
=======
        {
            log.Debug("Call Resource {0} | {1}", request, context);
            
            try
            {
                CallResourceResponse response = new CallResourceResponse();
                string fullUrl = HttpUtility.UrlDecode(request.PluginContext.DataSourceInstanceSettings.Url + request.Url);
                Uri uri = new Uri(fullUrl);
                NameValueCollection queryParams = HttpUtility.ParseQueryString(uri.Query);
                OpcUAConnection connection = Connections.Get(request.PluginContext.DataSourceInstanceSettings);
>>>>>>> master:pkg/dotnet/plugin-dotnet/ResourceService.cs
                switch (request.Path)
                {
                    case "browse":
                        {
                            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
<<<<<<< HEAD:backend/ResourceService.cs
                            var nId = Opc.Ua.NodeId.Parse(nodeId);
                            var browseResult = connection.Browse(nId);
                            var result = JsonSerializer.Serialize(browseResult.Select(a => Converter.ConvertToBrowseResult(a)).ToArray());
=======
                            string refId = HttpUtility.UrlDecode(queryParams["refId"]);
                            log.Debug("Got a subscription: NodeId[{0}], RefId[{1}]", nodeId, refId);
                            this.responseStream = responseStream;
                            connection.AddSubscription(refId, nodeId, SubscriptionCallback);
                            response.Code = 204;
                        }
                        break;

                    case "browseTypes":
                        {
                            string results = connection.BrowseTypes();
>>>>>>> master:pkg/dotnet/plugin-dotnet/ResourceService.cs
                            response.Code = 200;
                            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
                            log.Debug("We got a result from browse => {0}", result);
                        }
                        break;
                    case "browseTypes":
                        {
                            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
                            var nId = Opc.Ua.NodeId.Parse(nodeId);
                            var browseResult = connection.Browse(nId, (uint)(Opc.Ua.NodeClass.ObjectType | Opc.Ua.NodeClass.VariableType));
                            var result = JsonSerializer.Serialize(browseResult.Select(a => Converter.ConvertToBrowseResult(a)).ToArray());
                            response.Code = 200;
                            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
                            log.Debug("We got a result from browse => {0}", result);
                        }
                        break;
                }

                return responseStream.WriteAsync(response);
            }
            catch(Exception ex)
            {
                log.Debug("Got browse exception {0}", ex);
                throw ex;
            }
<<<<<<< HEAD:backend/ResourceService.cs

            responseStream.WriteAsync(response);
            return Task.CompletedTask;
=======
>>>>>>> master:pkg/dotnet/plugin-dotnet/ResourceService.cs
        }

        private void SubscriptionCallback(string refId, MonitoredItem item, MonitoredItemNotificationEventArgs eventArgs) {
            CallResourceResponse response = new CallResourceResponse();
            
            string jsonData = JsonSerializer.Serialize<MonitoredItem>(item);
            log.Debug("Subscription Response: {0}", jsonData);
            response.Code = 200;
            response.Body = ByteString.CopyFrom(jsonData, Encoding.ASCII);
            responseStream.WriteAsync(response);
        }
    }
}
