using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Logging;
using MicrosoftOpcUa.Client.Core;
using Opc.Ua.Client;
using Pluginv2;

namespace plugin_dotnet
{
    public class ResourceService : Resource.ResourceBase
    {
        private ILogger log;
        private IServerStreamWriter<CallResourceResponse> responseStream = null;

        public ResourceService()
        {
            log = new ConsoleLogger();
        }

        public override Task CallResource(CallResourceRequest request, IServerStreamWriter<CallResourceResponse> responseStream, ServerCallContext context)
        {
            log.Debug("Call Resource {0} | {1}", request, context);
            
            try
            {
                CallResourceResponse response = new CallResourceResponse();
                string fullUrl = HttpUtility.UrlDecode(request.PluginContext.DataSourceInstanceSettings.Url + request.Url);
                Uri uri = new Uri(fullUrl);
                NameValueCollection queryParams = HttpUtility.ParseQueryString(uri.Query);
                OpcUAConnection connection = Connections.Get(request.PluginContext.DataSourceInstanceSettings);
                switch (request.Path)
                {
                    case "subscribe":
                        {
                            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
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
                            response.Code = 200;
                            response.Body = ByteString.CopyFromUtf8(results);
                        }
                        break;

                    case "getAggregates":
                        {
                            Dictionary<string, OpcNodeAttribute> results = connection.ReadNodeAttributesAsDictionary(queryParams["nodeId"]);
                            string jsonResults = JsonSerializer.Serialize<Dictionary<string, OpcNodeAttribute>>(results);
                            response.Code = 200;
                            response.Body = ByteString.CopyFromUtf8(jsonResults);
                        }
                        break;

                    case "browse":
                        {
                            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
                            string result = connection.Browse(nodeId);
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
