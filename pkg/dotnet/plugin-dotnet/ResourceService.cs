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
using Org.BouncyCastle.Asn1.Crmf;

namespace plugin_dotnet
{
    public class ResourceService : Resource.ResourceBase
    {
        private readonly IConnections _connections;
        private readonly ILogger _log;
        private readonly IDashboardResolver _dashboardResolver;
        private IDictionary<string, Func<CallResourceRequest, Session, NameValueCollection, NamespaceTable, CallResourceResponse>> _resourceHandlers 
            = new Dictionary<string, Func<CallResourceRequest, Session, NameValueCollection, NamespaceTable, CallResourceResponse>>();

        public ResourceService(ILogger log, IConnections connections, IDashboardResolver dashboardResolver)
        {
            _connections = connections;
            _log = log;
            _dashboardResolver = dashboardResolver;
            InitResourceHandlers();
        }

        private void InitResourceHandlers()
		{
            _resourceHandlers.Add("readNode", ReadNode);
            _resourceHandlers.Add("browse", Browse);
            _resourceHandlers.Add("browseTypes", BrowseTypes);
            _resourceHandlers.Add("browseDataTypes", BrowseDataTypes);
            _resourceHandlers.Add("browseEventFields", BrowseEventFields);
            
            _resourceHandlers.Add("gettypedefinition", GetTypeDefinition);
            _resourceHandlers.Add("getdashboard", GetDashboard);
            _resourceHandlers.Add("adddashboardmapping", AddDashboardmapping);
            _resourceHandlers.Add("removedashboardmapping", RemoveDashboardmapping);
            _resourceHandlers.Add("removedashboardmappingbykeys", RemoveDashboardmappingByKeys);            
            _resourceHandlers.Add("browsereferencetargets", BrowseReferenceTargets);
            _resourceHandlers.Add("getnamespaces", GetNamespaces);
            _resourceHandlers.Add("isnodepresent", IsNodePresent);
        }

        private CallResourceResponse IsNodePresent(CallResourceRequest request, Session connection, NameValueCollection queryParams, NamespaceTable nsTable)
        {
            CallResourceResponse response = new CallResourceResponse();

            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);

            try
            {
                var nId = Converter.GetNodeId(nodeId, nsTable);

                var readRes = connection.ReadAttributes(nId, new[] { Opc.Ua.Attributes.BrowseName});

                bool present = readRes.Length > 0;

                var result = JsonSerializer.Serialize(present);
                response.Code = 200;
                response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
                _log.LogDebug($"We got a result for '{nodeId}' from IsNodePresent => {0}", result);

                return response;
            }
            catch(Exception e)
			{
                _log.LogDebug($"IsNodePresent for '{nodeId}' is false: ", e.Message);
                var result = JsonSerializer.Serialize(false);
                response.Code = 200;
                response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
                return response;
            }
        }

        private CallResourceResponse GetNamespaces(CallResourceRequest request, Session connection, NameValueCollection queryParams, NamespaceTable nsTable)
        {
            CallResourceResponse response = new CallResourceResponse();

            string[] namespaces = new string[nsTable.Count];
            for (uint i = 0; i < nsTable.Count; i++)
            {
                namespaces[i] = nsTable.GetString(i);
            }

            var result = JsonSerializer.Serialize(namespaces);
            response.Code = 200;
            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
            _log.LogDebug("We got a result from GetNamespaces => {0}", result);

            return response;
        }

        private CallResourceResponse BrowseReferenceTargets(CallResourceRequest request, Session connection, NameValueCollection queryParams, NamespaceTable nsTable)
        {
            CallResourceResponse response = new CallResourceResponse();
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

            return response;
        }

        private CallResourceResponse AddDashboardmapping(CallResourceRequest request, Session connection, NameValueCollection queryParams, NamespaceTable nsTable)
        {
            CallResourceResponse response = new CallResourceResponse();
            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
            string typeNodeId = HttpUtility.UrlDecode(queryParams["typeNodeId"]);
            bool useType = bool.Parse(HttpUtility.UrlDecode(queryParams["useType"]));
            string[] interfaces = JsonSerializer.Deserialize<string[]>(HttpUtility.UrlDecode(queryParams["interfaces"]));
            string dashboard = HttpUtility.UrlDecode(queryParams["dashboard"]);
            string existingDashboard = HttpUtility.UrlDecode(queryParams["existingDashboard"]);
            string perspective = HttpUtility.UrlDecode(queryParams["perspective"]);

            var nodeIdJson = ConvertNodeIdToJson(nodeId, nsTable);
            var typeNodeIdJson = ConvertNodeIdToJson(typeNodeId, nsTable);

            var interfacesIds = Array.Empty<string>();
            if (interfaces != null)
                interfacesIds = interfaces.Select(a => ConvertNodeIdToJson(a, nsTable)).ToArray();

            var dashboardMappingData = new DashboardMappingData(connection, nodeId, nodeIdJson, typeNodeIdJson, useType, interfacesIds, dashboard, existingDashboard, perspective, nsTable);
            var r = _dashboardResolver.AddDashboardMapping(dashboardMappingData);
            var result = JsonSerializer.Serialize(r);
            response.Code = 200;
            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
            _log.LogDebug("Adding dashboard mapping => {0}", r.success);
            return response;
        }

        private CallResourceResponse RemoveDashboardmapping(CallResourceRequest request, Session connection, NameValueCollection queryParams, NamespaceTable nsTable)
        {
            CallResourceResponse response = new CallResourceResponse();
            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
            string typeNodeId = HttpUtility.UrlDecode(queryParams["typeNodeId"]);
            bool useType = bool.Parse(HttpUtility.UrlDecode(queryParams["useType"]));
            string[] interfaces = JsonSerializer.Deserialize<string[]>(HttpUtility.UrlDecode(queryParams["interfaces"]));
            string perspective = HttpUtility.UrlDecode(queryParams["perspective"]);

            var nodeIdJson = ConvertNodeIdToJson(nodeId, nsTable);
            var typeNodeIdJson = ConvertNodeIdToJson(typeNodeId, nsTable);

            var interfacesIds = Array.Empty<string>();
            if (interfaces != null)
                interfacesIds = interfaces.Select(a => ConvertNodeIdToJson(a, nsTable)).ToArray();

            var dashboardMappingData = new DashboardMappingData(connection, nodeId, nodeIdJson, typeNodeIdJson, useType, interfacesIds, null, null, perspective, nsTable);
            var r = _dashboardResolver.RemoveDashboardMapping(dashboardMappingData);
            var result = JsonSerializer.Serialize(r);
            response.Code = 200;
            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
            _log.LogDebug("Remove dashboard mapping => {0}", r.success);
            return response;
        }

        private CallResourceResponse RemoveDashboardmappingByKeys(CallResourceRequest request, Session connection, NameValueCollection queryParams, NamespaceTable nsTable)
        {
            CallResourceResponse response = new CallResourceResponse();
            string[] dashKeys = JsonSerializer.Deserialize<string[]>(HttpUtility.UrlDecode(queryParams["dashKeys"]));
            string perspective = HttpUtility.UrlDecode(queryParams["perspective"]);

            var dashKeysIds = Array.Empty<string>();
            if (dashKeys != null)
                dashKeysIds = dashKeys.Select(a => ConvertNodeIdToJson(a, nsTable)).ToArray();

            var dashboardMappingData = new DashboardMappingData(connection, null, null, null, false, dashKeysIds, null, null, perspective, nsTable);
            var r = _dashboardResolver.RemoveDashboardMapping(dashboardMappingData);
            var result = JsonSerializer.Serialize(r);
            response.Code = 200;
            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
            _log.LogDebug("Remove dashboard mapping => {0}", r.success);
            return response;
        }



        private CallResourceResponse GetDashboard(CallResourceRequest request, Session connection, NameValueCollection queryParams, NamespaceTable nsTable)
		{
            CallResourceResponse response = new CallResourceResponse();
            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
            string perspective = HttpUtility.UrlDecode(queryParams["perspective"]);

            var targetNodeIdJson = ConvertNodeIdToJson(nodeId, nsTable);

            (string dashboard, string[] dashKeys) = _dashboardResolver.ResolveDashboard(connection, nodeId, targetNodeIdJson, perspective, nsTable);
            var dashboardInfo = new DashboardInfo() { name = dashboard, dashKeys = dashKeys };
            var result = JsonSerializer.Serialize(dashboardInfo);
            response.Code = 200;
            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
            _log.LogDebug("We got a dash => {0}", dashboard);

            return response;
        }


        private CallResourceResponse GetTypeDefinition(CallResourceRequest request, Session connection, NameValueCollection queryParams, NamespaceTable nsTable)
        {
            CallResourceResponse response = new CallResourceResponse();
            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
            var nId = Converter.GetNodeId(nodeId, nsTable);
            connection.Browse(null, null, nId, 1, Opc.Ua.BrowseDirection.Forward, "i=40", true, (uint) (Opc.Ua.NodeClass.ObjectType | Opc.Ua.NodeClass.VariableType), out byte[] cont, out Opc.Ua.ReferenceDescriptionCollection references);
            var result = JsonSerializer.Serialize(Converter.ConvertToBrowseResult(references[0], nsTable));
            response.Code = 200;
            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
            _log.LogDebug("We got a result from browse => {0}", result);
            return response;
        }


        // Move to Opc.Ua.Client
        private static Opc.Ua.NodeId GetSuperType(Session connection, NodeId id, NamespaceTable nsTable)
        {
            connection.Browse(null, null, id, 1, BrowseDirection.Inverse, ReferenceTypeIds.HasSubtype, false, (uint)NodeClass.ObjectType, out byte[] contPoint, out ReferenceDescriptionCollection references);
            if (references.Count > 0)
            {
                return ExpandedNodeId.ToNodeId(references[0].NodeId, nsTable);
            }
            return null;
        }

        private CallResourceResponse BrowseEventFields(CallResourceRequest request, Session connection, NameValueCollection queryParams, NamespaceTable nsTable)
        {
            CallResourceResponse response = new CallResourceResponse();
            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
            var nId = Converter.GetNodeId(nodeId, nsTable);
            var browseResult = connection.Browse(nId, (uint)(Opc.Ua.NodeClass.Variable));
            NodeId superType = null;
            while ((superType = GetSuperType(connection, nId, nsTable)) != null)
            {
                var superBrowseResult = connection.Browse(superType, (uint)(Opc.Ua.NodeClass.Variable));
                browseResult.AddRange(superBrowseResult);
                nId = superType;
            }
            var ordered = browseResult.Select(a => Converter.ConvertToBrowseResult(a, nsTable)).OrderBy(a => a.browseName.name).ToArray();
            var result = JsonSerializer.Serialize(ordered);
            response.Code = 200;
            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
            _log.LogDebug("We got a result from browse => {0}", result);
            return response;
        }


        private CallResourceResponse BrowseDataTypes(CallResourceRequest request, Session connection, NameValueCollection queryParams, NamespaceTable nsTable)
        {
            CallResourceResponse response = new CallResourceResponse();
            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
            var nId = Converter.GetNodeId(nodeId, nsTable);
            var browseResult = connection.Browse(nId, (uint)(Opc.Ua.NodeClass.ObjectType | Opc.Ua.NodeClass.VariableType));
            var result = JsonSerializer.Serialize(browseResult.Select(a => Converter.ConvertToBrowseResult(a, nsTable)).ToArray());
            response.Code = 200;
            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
            _log.LogDebug("We got a result from browse => {0}", result);
            return response;
        }

        private CallResourceResponse BrowseTypes(CallResourceRequest request, Session connection, NameValueCollection queryParams, NamespaceTable nsTable)
        {
            CallResourceResponse response = new CallResourceResponse();
            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
            var nId = Converter.GetNodeId(nodeId, nsTable);
            var browseResult = connection.Browse(nId, (uint)(Opc.Ua.NodeClass.ObjectType | Opc.Ua.NodeClass.VariableType));
            var result = JsonSerializer.Serialize(browseResult.Select(a => Converter.ConvertToBrowseResult(a, nsTable)).ToArray());
            response.Code = 200;
            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
            _log.LogDebug("We got a result from browse => {0}", result);
            return response;
        }

        private CallResourceResponse Browse(CallResourceRequest request, Session connection, NameValueCollection queryParams, NamespaceTable nsTable)
        {
            CallResourceResponse response = new CallResourceResponse();
            string nodeId = HttpUtility.UrlDecode(queryParams["nodeId"]);
            var mask = queryParams["nodeClassMask"];

            string filter = queryParams["browseFilter"];
            BrowseFilter browseFilter = null;
            if (!string.IsNullOrEmpty(filter))
            {
                browseFilter = JsonSerializer.Deserialize<BrowseFilter>(filter);
            }

            var hasMask = !string.IsNullOrWhiteSpace(mask);
            uint nodeClassMask = 0;
            if (hasMask)
            {
                if (!uint.TryParse(mask, out nodeClassMask))
                    hasMask = false;
            }
            var nId = Converter.GetNodeId(nodeId, nsTable);
            IEnumerable<ReferenceDescription> browseResult = connection.Browse(nId, hasMask ? nodeClassMask : (int)(NodeClass.Object | NodeClass.Variable), browseFilter != null ? (uint)browseFilter.maxResults : uint.MaxValue);
            if (!string.IsNullOrEmpty(browseFilter?.browseName))
                browseResult = browseResult.Where(a => a.BrowseName.Name.Contains(browseFilter.browseName));

            var result = JsonSerializer.Serialize(browseResult.Select(a => Converter.ConvertToBrowseResult(a, nsTable)).OrderBy(refdesc => refdesc.displayName).ToArray());
            response.Code = 200;
            response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
            _log.LogDebug("We got a result from browse => {0}", result);
            return response;
        }

        private CallResourceResponse ReadNode(CallResourceRequest request, Session connection, NameValueCollection queryParams, NamespaceTable nsTable)
        {
            CallResourceResponse response = new CallResourceResponse();
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
            return response;
        }



        public override Task CallResource(CallResourceRequest request, IServerStreamWriter<CallResourceResponse> responseStream, ServerCallContext context)
        {
            _log.LogDebug("Call Resource {0} | {1}", request, context);
            
            try
            {

                var resourceUrl = request.Url;
                if (!string.IsNullOrEmpty(request.Url) && !request.Url.StartsWith("/"))
                    resourceUrl = string.Concat("/", request.Url);

                string fullUrl = HttpUtility.UrlDecode(request.PluginContext.DataSourceInstanceSettings.Url + resourceUrl);
                Uri uri = new Uri(fullUrl);
                NameValueCollection queryParams = HttpUtility.ParseQueryString(uri.Query);
                var connection = _connections.Get(request.PluginContext.DataSourceInstanceSettings).Session;

                connection.FetchNamespaceTables();
                var nsTable = connection.NamespaceUris;
                Func<CallResourceRequest, Session, NameValueCollection, NamespaceTable, CallResourceResponse> resourceHandler;
                if (_resourceHandlers.TryGetValue(request.Path, out resourceHandler))
                {
                    var response = resourceHandler(request, connection, queryParams, nsTable);
                    responseStream.WriteAsync(response);
                    return Task.CompletedTask;
                }
                throw new ArgumentException(string.Format("Invalid path {0}", request.Path));
            }
            catch(Exception ex)
            {
                _log.LogError("Got resource exception {0}", ex);
                throw ex;
            }
        }

        private static string ConvertNodeIdToJson(string nodeId, NamespaceTable nsTable)
        {
            var uaNodeId = Converter.GetNodeId(nodeId, nsTable);
            var url = nsTable.GetString(uaNodeId.NamespaceIndex);

            string identifier;
            if (uaNodeId.IdType == IdType.Opaque)
            {
                var opaque = (byte[])uaNodeId.Identifier;
                identifier = Encoding.UTF8.GetString(opaque, 0, opaque.Length);
            }
            else {
                identifier = uaNodeId.Identifier.ToString();
            }

            var targetNodeId = new NsNodeIdentifier { NamespaceUrl = url, IdType = uaNodeId.IdType, Identifier = identifier };
            var targetNodeIdJson = JsonSerializer.Serialize(targetNodeId);
            return targetNodeIdJson;
        }

    }
}
