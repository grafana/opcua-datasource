using Opc.Ua;
using Opc.Ua.Client;
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
			if (_dashboardDb.QueryDashboard(dashKeys, true, perspective, out DashboardData dashboard))
				return (dashboard, new string[] { ConvertToNodeIdKey(nodeId, nsTable) });

			return (null, Array.Empty<string>());
		}

		private (DashboardData dashboard, string[] dashKeys) ResolveDashboard(ExpandedNodeId[] nodeIds, bool requireAllKeys, string perspective, NamespaceTable nsTable)
		{
			string[] dashKeys = nodeIds.Select(n => ConvertNodeIdToJson(n, nsTable)).ToArray();

			if (_dashboardDb.QueryDashboard(dashKeys, requireAllKeys, perspective, out DashboardData dashboard))
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
			var nsIdNodeId = new NsNodeIdentifier() { NamespaceUrl = nsTable.GetString(nodeId.NamespaceIndex), Identifier = nodeIdPart.Identifier.ToString() };
			string typeNodeIdJson = System.Text.Json.JsonSerializer.Serialize(nsIdNodeId);
			return typeNodeIdJson;
		}

		private Opc.Ua.ExpandedNodeId GetSuperType(Session uaConnection,  Opc.Ua.ExpandedNodeId typeNodeId, Opc.Ua.NamespaceTable nsTable)
		{
			var nId = ExpandedNodeId.ToNodeId(typeNodeId, nsTable);
			uaConnection.Browse(null, null, nId, 1, Opc.Ua.BrowseDirection.Inverse, "i=45", false, (uint)(Opc.Ua.NodeClass.ObjectType | Opc.Ua.NodeClass.VariableType),
				out byte[] cont, out Opc.Ua.ReferenceDescriptionCollection references);

			return references.Count > 0 ? references[0].NodeId : null;
		}



		public (string dashboard, string[] dashKeys) ResolveDashboard(Session uaConnection, string nodeId, string targetNodeIdJson, string perspective, NamespaceTable nsTable)
		{
			//Get for intance
			string[] dashKeys = new[] { targetNodeIdJson };
			if (_dashboardDb.QueryDashboard(dashKeys, true, perspective, out DashboardData dashboard))
				return (dashboard.Name, new string[] { nodeId });

			var nId = Converter.GetNodeId(nodeId, nsTable);

			//Get for interface(s)
			var hasInterface = "i=17603";
			uaConnection.Browse(null, null, nId, int.MaxValue, BrowseDirection.Forward, hasInterface, true, (uint)(NodeClass.ObjectType), out _, out ReferenceDescriptionCollection intReferences);
			if (intReferences.Count > 0)
			{
				List<ExpandedNodeId> interfaces = new List<ExpandedNodeId>();

				for (int i = 0; i < intReferences.Count; i++)
					interfaces.Add(intReferences[i].NodeId);

				(dashboard, dashKeys) = ResolveDashboard(interfaces.ToArray(), true, perspective, nsTable);

				if (dashboard != null)
					return (dashboard.Name, dashKeys);
			}

			//Get for ClassType(s)
			var definedByEquipmentClass = Converter.GetNodeId("{\"namespaceUrl\":\"http://www.OPCFoundation.org/UA/2013/01/ISA95\",\"id\":\"i=4919\"}", nsTable);
			uaConnection.Browse(null, null, nId, int.MaxValue, BrowseDirection.Forward, definedByEquipmentClass, true, (uint)(NodeClass.ObjectType), out _, out ReferenceDescriptionCollection classReferences);
			if (classReferences.Count > 0)
			{
				List<ExpandedNodeId> classTypes = new List<ExpandedNodeId>();

				for (int i = 0; i < classReferences.Count; i++)
					classTypes.Add(classReferences[i].NodeId);

				(dashboard, dashKeys) = ResolveDashboard(classTypes.ToArray(), true, perspective, nsTable);

				if (dashboard != null)
					return (dashboard.Name, dashKeys);
			}

			// Get for type or subtype
			uaConnection.Browse(null, null, nId, 1, BrowseDirection.Forward, "i=40", true, (uint)(NodeClass.ObjectType | NodeClass.VariableType), out byte[] cont, out ReferenceDescriptionCollection typeReferences);
			if (typeReferences.Count > 0)
			{
				var typeNodeId = typeReferences[0].NodeId;
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

		public UaResult AddDashboardMapping(DashboardMappingData data)
		{
			if(data.InterfacesIds?.Length > 0)
			{
				return _dashboardDb.AddDashboardMapping(data.InterfacesIds, data.Dashboard, data.Perspective);
			}
			else
			{
				string nIdJson;
				if (data.UseType)
				{
					_dashboardDb.RemoveMapping(data.NodeIdJson, data.Perspective);
					nIdJson = data.TypeNodeIdJson;
				}
				else
				{
					nIdJson = data.NodeIdJson;
				}

				return _dashboardDb.AddDashboardMapping(new[] { nIdJson }, data.Dashboard, data.Perspective);

			}
		}
	}
}
