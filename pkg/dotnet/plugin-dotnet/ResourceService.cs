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
//using MicrosoftOpcUa.Client.Core;
using Prediktor.UA.Client;
using Opc.Ua.Client;
using Pluginv2;
using Microsoft.Extensions.Logging;
using Opc.Ua;

namespace plugin_dotnet
{
    public class ResourceService : Resource.ResourceBase
    {
        private readonly IConnections _connections;
        private IServerStreamWriter<CallResourceResponse> responseStream = null;
        private readonly ILogger _log;
        private readonly IDashboardResolver _dashboardResolver;

        public ResourceService(ILogger log, IConnections connections, IDashboardResolver dashboardResolver)
        {
            _connections = connections;
            _log = log;
            _dashboardResolver = dashboardResolver;
        }

        public override Task CallResource(CallResourceRequest request, IServerStreamWriter<CallResourceResponse> responseStream, ServerCallContext context)
        {
            _log.LogDebug("Call Resource {0} | {1}", request, context);
            CallResourceResponse response = new CallResourceResponse();
            string fullUrl = HttpUtility.UrlDecode(request.PluginContext.DataSourceInstanceSettings.Url + request.Url);
            Uri uri = new Uri(fullUrl);
            NameValueCollection queryParams = HttpUtility.ParseQueryString(uri.Query);
            var connection = _connections.Get(request.PluginContext.DataSourceInstanceSettings).Session;
            try
            {
                connection.FetchNamespaceTables();
                var nsTable = connection.NamespaceUris;
                switch (request.Path)
                {
                    //case "subscribe":
                    //    {
                    //        string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
                    //        string refId = HttpUtility.UrlDecode(queryParams["refId"]);
                    //        log.Debug("Got a subscription: NodeId[{0}], RefId[{1}]", nodeId, refId);
                    //        this.responseStream = responseStream;
                    //        connection.AddSubscription(refId, nodeId, SubscriptionCallback);
                    //        response.Code = 204;
                    //    }
                    //    break;

                    //case "types":
                    //    {
                    //        string results = connection.BrowseTypes();
                    //        response.Code = 200;
                    //        response.Body = ByteString.CopyFromUtf8(results);
                    //    }
                    //    break;

                    //case "getAggregates":
                    //    {
                    //        Dictionary<string, OpcNodeAttribute> results = connection.ReadNodeAttributesAsDictionary(queryParams["nodeId"]);
                    //        string jsonResults = JsonSerializer.Serialize<Dictionary<string, OpcNodeAttribute>>(results);
                    //        response.Code = 200;
                    //        response.Body = ByteString.CopyFromUtf8(jsonResults);
                    //    }
                    //    break;
                    //case "browseEventType":
                    //    {
                    //        string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
                    //        var nId = Converter.GetNodeId(nodeId, nsTable);
                    //        var browseResult = connection.GetEventProperties();
                    //        var result = JsonSerializer.Serialize(browseResult.Select(a => Converter.ConvertToBrowseResult(a, nsTable)).ToArray());
                    //        response.Code = 200;
                    //        response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
                    //        _log.LogDebug("We got a result from browse => {0}", result);
                    //    }
                    //    break;
                    case "readNode":
                        {
                            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
                            var nId = Converter.GetNodeId(nodeId, nsTable);
                            var readRes = connection.ReadAttributes(nId, new[] { Opc.Ua.Attributes.BrowseName, Opc.Ua.Attributes.DisplayName, Opc.Ua.Attributes.NodeClass });
                            var browseName = (Opc.Ua.QualifiedName)readRes[0].Value;
                            var displayName = (Opc.Ua.LocalizedText)readRes[1].Value;
                            var nodeClass = (Opc.Ua.NodeClass)readRes[2].Value;
                            var nodeInfo = new NodeInfo() { browseName = Converter.GetQualifiedName(browseName, nsTable), displayName = displayName.Text, nodeClass = (uint)nodeClass, nodeId = nodeId };

                            var result = JsonSerializer.Serialize(nodeInfo);
                            response.Code = 200;
                            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
                        }
                        break;
                    case "browse":
                        {
                            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
                            var mask = queryParams["nodeClassMask"];
                            var hasMask = !string.IsNullOrWhiteSpace(mask);
                            uint nodeClassMask = 0;
                            if (hasMask) {
                                if (!uint.TryParse(mask, out nodeClassMask))
                                    hasMask = false;
                            }
                            var nId = Converter.GetNodeId(nodeId, nsTable);
                            var browseResult = hasMask ? connection.Browse(nId, nodeClassMask) : connection.Browse(nId);
                            var result = JsonSerializer.Serialize(browseResult.Select(a => Converter.ConvertToBrowseResult(a, nsTable)).ToArray());
                            response.Code = 200;
                            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
                            _log.LogDebug("We got a result from browse => {0}", result);
                        }
                        break;
                    case "browseTypes":
                        {
                            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
                            var nId = Converter.GetNodeId(nodeId, nsTable);
                            var browseResult = connection.Browse(nId, (uint)(Opc.Ua.NodeClass.ObjectType | Opc.Ua.NodeClass.VariableType));
                            var result = JsonSerializer.Serialize(browseResult.Select(a => Converter.ConvertToBrowseResult(a, nsTable)).ToArray());
                            response.Code = 200;
                            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
                            _log.LogDebug("We got a result from browse => {0}", result);
                        }
                        break;

                    case "browseDataTypes":
                        {
                            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
                            var nId = Converter.GetNodeId(nodeId, nsTable);
                            var browseResult = connection.Browse(nId, (uint)(Opc.Ua.NodeClass.ObjectType | Opc.Ua.NodeClass.VariableType));
                            var result = JsonSerializer.Serialize(browseResult.Select(a => Converter.ConvertToBrowseResult(a, nsTable)).ToArray());
                            response.Code = 200;
                            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
                            _log.LogDebug("We got a result from browse => {0}", result);
                        }
                        break;
                    case "gettypedefinition":
						{
                            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
                            var nId = Converter.GetNodeId(nodeId, nsTable);
                            connection.Browse(null, null, nId, 1, Opc.Ua.BrowseDirection.Forward, "i=40", true, (uint)(Opc.Ua.NodeClass.ObjectType | Opc.Ua.NodeClass.VariableType), out byte[] cont, out Opc.Ua.ReferenceDescriptionCollection references);
                            var result = JsonSerializer.Serialize( Converter.ConvertToBrowseResult(references[0], nsTable));
                            response.Code = 200;
                            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
                            _log.LogDebug("We got a result from browse => {0}", result);
                        }
                        break;
                    case "getdashboard":
                        {
                            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
                            string perspective = HttpUtility.UrlDecode(queryParams["perspective"]);

                            var targetNodeIdJson = ConvertNodeIdToJson(nodeId);

                            (string dashboard, string[] dashKeys) = _dashboardResolver.ResolveDashboard(connection, nodeId, targetNodeIdJson, perspective, nsTable);
                            var dashboardInfo = new DashboardInfo() { name = dashboard, dashKeys = dashKeys };
                            var result = JsonSerializer.Serialize(dashboardInfo);
                            response.Code = 200;
                            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
                            _log.LogDebug("We got a dash => {0}", dashboard);
                        }
                        break;
                    case "adddashboardmapping":
						{
                            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
                            string typeNodeId = HttpUtility.UrlDecode(queryParams["typeNodeId"]);
                            bool useType = bool.Parse(HttpUtility.UrlDecode(queryParams["useType"]));
							string[] interfaces = JsonSerializer.Deserialize<string[]>(HttpUtility.UrlDecode(queryParams["interfaces"]));
							string dashboard = HttpUtility.UrlDecode(queryParams["dashboard"]);
							string existingDashboard = HttpUtility.UrlDecode(queryParams["existingDashboard"]);
							string perspective = HttpUtility.UrlDecode(queryParams["perspective"]);

                            var nodeIdJson = ConvertNodeIdToJson(nodeId);
                            var typeNodeIdJson = ConvertNodeIdToJson(typeNodeId);

                            var interfacesIds = Array.Empty<string>();
                            if (interfaces != null)
                                interfacesIds = interfaces.Select(a => ConvertNodeIdToJson(a)).ToArray();

							var dashboardMappingData = new DashboardMappingData(connection, nodeId, nodeIdJson, typeNodeIdJson, useType, interfacesIds, dashboard, existingDashboard, perspective, nsTable);
							var r = _dashboardResolver.AddDashboardMapping(dashboardMappingData);
							var result = JsonSerializer.Serialize(r);
							response.Code = 200;
							response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
							_log.LogDebug("Adding dashboard mapping => {0}", r.success);
						}
						break;
                    case "browsereferencetargets":
                        {
                            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
                            string referenceId = HttpUtility.UrlDecode(queryParams["referenceId"]);
                            var nId = Converter.GetNodeId(nodeId, nsTable);
                            var rId = Converter.GetNodeId(referenceId, nsTable);
                            connection.Browse(null, null, nId, int.MaxValue, Opc.Ua.BrowseDirection.Forward, rId, true, 
                                (uint)(Opc.Ua.NodeClass.ObjectType | Opc.Ua.NodeClass.VariableType), out byte[] cont, out Opc.Ua.ReferenceDescriptionCollection targets);
                            var result = JsonSerializer.Serialize(targets.Select(a => Converter.ConvertToBrowseResult(a, nsTable)).ToArray());
                            response.Code = 200;
                            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
                            _log.LogDebug("We got a result from browse ref targets => {0}", result);
                        }
                        break;


                }

            }
            catch(Exception ex)
            {
                _log.LogError("Got browse exception {0}", ex);
                throw ex;
            }
             
            responseStream.WriteAsync(response);
            return Task.CompletedTask;
        }

		private static string ConvertNodeIdToJson(string nodeId)
		{
			var expandedId = JsonSerializer.Deserialize<NSExpandedNodeId>(nodeId);
			var uaNodeId = NodeId.Parse(expandedId.id);
			var targetNodeId = new NsNodeIdentifier { NamespaceUrl = expandedId.namespaceUrl, Identifier = uaNodeId.Identifier.ToString() };
			var targetNodeIdJson = JsonSerializer.Serialize(targetNodeId);
			return targetNodeIdJson;
		}

		private void SubscriptionCallback(string refId, MonitoredItem item, MonitoredItemNotificationEventArgs eventArgs) {
            CallResourceResponse response = new CallResourceResponse();
            
            string jsonData = JsonSerializer.Serialize<MonitoredItem>(item);
            _log.LogDebug("Subscription Response: {0}", jsonData);
            response.Code = 200;
            response.Body = ByteString.CopyFrom(jsonData, Encoding.ASCII);
            responseStream.WriteAsync(response);
        }
    }
}
