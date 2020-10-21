using Newtonsoft.Json;
using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace plugin_dotnet
{
	public interface IDashboardResolver
	{
		string ResolveDashboard(Session uaConnection, string nodeId, string targetNodeIdJson, string perspective, Opc.Ua.NamespaceTable nsTable);
		UaResult AddDashboardMapping(Session uaConnection, string nodeId, string targetNodeIdJson, bool useType, string dashboard, string perspective, Opc.Ua.NamespaceTable nsTable);
	}


	public class DashboardResolver : IDashboardResolver
	{

		private IDashboardDb _dashboardDb;

		public DashboardResolver(IDashboardDb dashboardDb)
		{
			_dashboardDb = dashboardDb;
		}


		private string GetTypeDashboard(Session uaConnection, Opc.Ua.ExpandedNodeId typeNodeId, string perspective, Opc.Ua.NamespaceTable nsTable)
		{
			string typeNodeIdJson = ConvertNodeIdToJson(typeNodeId, nsTable);

			if (_dashboardDb.QueryDashboard(new[] { typeNodeIdJson }, perspective, out string dashboard))
				return dashboard;

			return string.Empty;
		}

		private static string ConvertNodeIdToJson(ExpandedNodeId typeNodeId, NamespaceTable nsTable)
		{
			var nodeIdPart = ExpandedNodeId.ToNodeId(typeNodeId, nsTable);
			var nsIdNodeId = new NsNodeIdentifier() { NamespaceUrl = nsTable.GetString(typeNodeId.NamespaceIndex), Identifier = nodeIdPart.Identifier.ToString() };
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



		public string ResolveDashboard(Session uaConnection, string nodeId, string targetNodeIdJson, string perspective, Opc.Ua.NamespaceTable nsTable)
		{
			if (_dashboardDb.QueryDashboard(new[] { targetNodeIdJson }, perspective, out string dashboard))
			{
				return dashboard;
			}


			// Get type 
			var nId = Converter.GetNodeId(nodeId, nsTable);
			uaConnection.Browse(null, null, nId, 1, Opc.Ua.BrowseDirection.Forward, "i=40", true, (uint)(Opc.Ua.NodeClass.ObjectType | Opc.Ua.NodeClass.VariableType), out byte[] cont, out Opc.Ua.ReferenceDescriptionCollection references);
			if (references.Count > 0)
			{
				var typeNodeId = references[0].NodeId;
				dashboard = GetTypeDashboard(uaConnection, typeNodeId, perspective, nsTable);
				if (!string.IsNullOrWhiteSpace(dashboard))
					return dashboard;

				bool getSuperType = true;
				while (getSuperType)
				{
					typeNodeId = GetSuperType(uaConnection, typeNodeId, nsTable);
					if (typeNodeId != null)
					{
						dashboard = GetTypeDashboard(uaConnection, typeNodeId, perspective, nsTable);
						if (!string.IsNullOrWhiteSpace(dashboard))
							return dashboard;
					}
					else
						getSuperType = false;
				}
			}
			return string.Empty;

		}

		public UaResult AddDashboardMapping(Session uaConnection, string nodeId, string targetNodeIdJson, bool useType, string dashboard, string perspective, NamespaceTable nsTable)
		{
			NodeClass nodeClass = NodeClass.Object;
			if (useType)
			{
				var nId = Converter.GetNodeId(nodeId, nsTable);
				_ = uaConnection.Browse(null, null, nId, 1, Opc.Ua.BrowseDirection.Forward, "i=40", true, (uint)(Opc.Ua.NodeClass.ObjectType | Opc.Ua.NodeClass.VariableType), out byte[] cont, out Opc.Ua.ReferenceDescriptionCollection references);
				if (references.Count > 0)
				{
					targetNodeIdJson = ConvertNodeIdToJson(references[0].NodeId, nsTable);
					nodeClass = NodeClass.ObjectType;
				}
			}

			return _dashboardDb.AddDashboardMapping(new[] { targetNodeIdJson }, new[] { nodeClass }, dashboard, perspective);
		}
	}
}
