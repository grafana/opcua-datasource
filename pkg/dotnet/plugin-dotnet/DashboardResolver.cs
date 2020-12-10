using Opc.Ua;
using Opc.Ua.Client;
using Prediktor.UA.Client;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace plugin_dotnet
{
	public interface IDashboardResolver
	{
		(string dashboard, string[] dashKeys) ResolveDashboard(Session uaConnection, string nodeId, string targetNodeIdJson, string perspective, NamespaceTable nsTable);
		UaResult AddDashboardMapping(DashboardMappingData dashboardMappingData);
		UaResult RemoveDashboardMapping(DashboardMappingData dashboardMappingData);
	}

	public class DashboardResolver : IDashboardResolver
	{

		private IDashboardDb _dashboardDb;

		public DashboardResolver(IDashboardDb dashboardDb)
		{
			_dashboardDb = dashboardDb;
		}

		private (DashboardData dashboard, string[] dashKeys) ResolveDashboard(ExpandedNodeId nodeId, string perspective, NamespaceTable nsTable)
		{
			string nodeIdJson = ConvertNodeIdToJson(nodeId, nsTable);
			string[] dashKeys = new[] { nodeIdJson };
			if (_dashboardDb.QueryDashboard(dashKeys, perspective, out DashboardData dashboard))
				return (dashboard, new string[] { ConvertToNodeIdKey(nodeId, nsTable) });

			return (null, Array.Empty<string>());
		}

		private (DashboardData dashboard, string[] dashKeys) ResolveDashboard(ExpandedNodeId[] nodeIds, string perspective, NamespaceTable nsTable)
		{
			string[] dashKeys = nodeIds.Select(n => ConvertNodeIdToJson(n, nsTable)).ToArray();

			if (_dashboardDb.QueryDashboard(dashKeys, perspective, out DashboardData dashboard))
				return (dashboard, nodeIds.Select(n => ConvertToNodeIdKey(n, nsTable)).ToArray());

			return (null, Array.Empty<string>());
		}

		private static string ConvertToNodeIdKey(ExpandedNodeId nodeId, NamespaceTable nsTable)
		{
			var nodeIdPart = ExpandedNodeId.ToNodeId(nodeId, nsTable);
			var nsNodeId = new NSNodeId() { id = nodeIdPart.ToString(), namespaceUrl = nsTable.GetString(nodeId.NamespaceIndex) };
			string nsNodeIdJson = System.Text.Json.JsonSerializer.Serialize(nsNodeId);

			return nsNodeIdJson;
		}

		private static string ConvertNodeIdToJson(ExpandedNodeId nodeId, NamespaceTable nsTable)
		{
			var nodeIdPart = ExpandedNodeId.ToNodeId(nodeId, nsTable);
			var nsIdNodeId = new NsNodeIdentifier() { NamespaceUrl = nsTable.GetString(nodeId.NamespaceIndex), IdType = nodeIdPart.IdType, Identifier = nodeIdPart.Identifier.ToString() };
			string typeNodeIdJson = System.Text.Json.JsonSerializer.Serialize(nsIdNodeId);
			return typeNodeIdJson;
		}

		private Opc.Ua.ExpandedNodeId GetSuperType(Session uaConnection,  Opc.Ua.ExpandedNodeId typeNodeId, Opc.Ua.NamespaceTable nsTable)
		{
			var nId = ExpandedNodeId.ToNodeId(typeNodeId, nsTable);
			_ = uaConnection.Browse(null, null, nId, 1, Opc.Ua.BrowseDirection.Inverse, "i=45", false, (uint)(Opc.Ua.NodeClass.ObjectType | Opc.Ua.NodeClass.VariableType),
				out _, out Opc.Ua.ReferenceDescriptionCollection references);

			return references.Count > 0 ? references[0].NodeId : null;
		}



		public (string dashboard, string[] dashKeys) ResolveDashboard(Session uaConnection, string nodeId, string targetNodeIdJson, string perspective, NamespaceTable nsTable)
		{
			//Get for instance
			string[] dashKeys = new[] { targetNodeIdJson };
			if (_dashboardDb.QueryDashboard(dashKeys, perspective, out DashboardData dashboard))
				return (dashboard.Name, new string[] { nodeId });

			var nId = Converter.GetNodeId(nodeId, nsTable);

			//Get interface(s)
			List<ExpandedNodeId> interfaces = new List<ExpandedNodeId>();
			if (IsNodePresent(uaConnection, UaConstants.HasInterface, nsTable))
			{
				uaConnection.Browse(null, null, nId, int.MaxValue, BrowseDirection.Forward, UaConstants.HasInterface, true, (uint)(NodeClass.ObjectType), out _, out ReferenceDescriptionCollection intReferences);
				if (intReferences.Count > 0)
				{
					for (int i = 0; i < intReferences.Count; i++)
						interfaces.Add(intReferences[i].NodeId);
				}
			}

			//Get ClassType(s)
			List<ExpandedNodeId> classTypes = new List<ExpandedNodeId>();
			if (IsNodePresent(uaConnection, UaConstants.DefinedByEquipmentClass, nsTable))
			{
				var definedByEquipmentClass = Converter.GetNodeId(UaConstants.DefinedByEquipmentClass, nsTable);
				uaConnection.Browse(null, null, nId, int.MaxValue, BrowseDirection.Forward, definedByEquipmentClass, true, (uint)(NodeClass.ObjectType), out _, out ReferenceDescriptionCollection classReferences);
				if (classReferences.Count > 0)
				{

					for (int i = 0; i < classReferences.Count; i++)
						classTypes.Add(classReferences[i].NodeId);
				}
			}

			// Get type
			ExpandedNodeId typeNodeId = null;
			uaConnection.Browse(null, null, nId, 1, BrowseDirection.Forward, "i=40", true, (uint)(NodeClass.ObjectType | NodeClass.VariableType), out byte[] cont, out ReferenceDescriptionCollection typeReferences);
			if (typeReferences.Count > 0)
			{
				typeNodeId = typeReferences[0].NodeId;
			}

			if(interfaces.Count > 0 || classTypes.Count > 0)
			{
				if(typeNodeId != null)
				{
					List<ExpandedNodeId> typeAndInterfaces = new List<ExpandedNodeId>(interfaces);
					typeAndInterfaces.AddRange(classTypes);
					typeAndInterfaces.Add(typeNodeId);

					(dashboard, dashKeys) = ResolveDashboard(typeAndInterfaces.ToArray(), perspective, nsTable);

					if (dashboard != null)
						return (dashboard.Name, dashKeys);
				}

				List<ExpandedNodeId> allInterfaces = new List<ExpandedNodeId>(interfaces);
				allInterfaces.AddRange(classTypes);

				(dashboard, dashKeys) = ResolveDashboard(allInterfaces.ToArray(), perspective, nsTable);

				if (dashboard != null)
					return (dashboard.Name, dashKeys);
			}


			// Get for type or subtype
			if (typeNodeId != null)
			{
				(dashboard, dashKeys) = ResolveDashboard(typeNodeId, perspective, nsTable);
				if (dashboard != null)
					return (dashboard.Name, dashKeys);

				bool getSuperType = true;
				while (getSuperType)
				{
					typeNodeId = GetSuperType(uaConnection, typeNodeId, nsTable);
					if (typeNodeId != null)
					{
						(dashboard, dashKeys) = ResolveDashboard(typeNodeId, perspective, nsTable);
						if (dashboard != null)
							return (dashboard.Name, dashKeys);
					}
					else
						getSuperType = false;
				}
			}

			return (string.Empty, Array.Empty<string>());
		}

		private bool IsNodePresent(Session uaConnection, string nodeId, NamespaceTable nsTable)
		{
			try
			{
				var nId = Converter.GetNodeId(nodeId, nsTable);
				var readRes = uaConnection.ReadAttributes(nId, new[] { Opc.Ua.Attributes.BrowseName });

				return readRes.Length > 0;
			}
			catch
			{
				return false;
			}
		}

		public UaResult AddDashboardMapping(DashboardMappingData data)
		{
			if(data.InterfacesIds?.Length > 0)
			{
				var nIdJsons = new List<string>(data.InterfacesIds);
				if (data.UseType)
					nIdJsons.Add(data.TypeNodeIdJson);

				return _dashboardDb.AddDashboardMapping(nIdJsons.ToArray(), data.Dashboard, data.Perspective);
			}
			else
			{
				string nIdJson = data.UseType ? data.TypeNodeIdJson : data.NodeIdJson;

				return _dashboardDb.AddDashboardMapping(new[] { nIdJson }, data.Dashboard, data.Perspective);
			}
		}

		public UaResult RemoveDashboardMapping(DashboardMappingData data)
		{
			var nIdJsons = new List<string>();

			if (data.InterfacesIds?.Length > 0)
				nIdJsons.AddRange(data.InterfacesIds);

			nIdJsons.Add(data.TypeNodeIdJson);

			return _dashboardDb.RemoveDashboardMapping(nIdJsons.ToArray(), data.Perspective);
		}
	}
}
