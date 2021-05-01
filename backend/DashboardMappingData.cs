using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace plugin_dotnet
{
	public class DashboardMappingData
	{
		public DashboardMappingData(Session uaConnection, string nodeId, string nodeIdJson, string typeNodeIdJson, bool useType, string[] interfacesIds,
			string dashboard, string existingDashboard, string perspective, Opc.Ua.NamespaceTable nsTable)
		{
			UaConnection = uaConnection;
			NodeId = nodeId;
			NodeIdJson = nodeIdJson;
			TypeNodeIdJson = typeNodeIdJson;
			UseType = useType;
			InterfacesIds = interfacesIds;
			Dashboard = dashboard;
			Perspective = perspective;
			ExisingDashboard = existingDashboard;
			NsTable = nsTable;
		}

		internal Session UaConnection { get; }
		internal string NodeId { get; }
		internal string NodeIdJson { get; set; }
		internal string TypeNodeIdJson { get; set; }
		internal bool UseType { get; }
		public string[] InterfacesIds { get; }
		internal string Dashboard { get; }
		internal string Perspective { get; }
		internal string ExisingDashboard { get; }
		internal NamespaceTable NsTable { get; }
	}
}
