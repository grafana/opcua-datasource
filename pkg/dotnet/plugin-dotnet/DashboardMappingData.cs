using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace plugin_dotnet
{
	public class DashboardMappingData
	{
		public DashboardMappingData(Session uaConnection, string nodeId, string targetNodeIdJson, bool useType, 
			string dashboard, string existingDashboard, string perspective, Opc.Ua.NamespaceTable nsTable)
		{
			UaConnection = uaConnection;
			NodeId = nodeId;
			TargetNodeIdJson = targetNodeIdJson;
			UseType = useType;
			Dashboard = dashboard;
			Perspective = perspective;
			ExisingDashboard = existingDashboard;
			NsTable = nsTable;
		}

		internal Session UaConnection { get; }
		internal string NodeId { get; }
		internal string TargetNodeIdJson { get; set; }
		internal bool UseType { get; }
		internal string Dashboard { get; }
		internal string Perspective { get; }
		internal string ExisingDashboard { get; }
		internal NamespaceTable NsTable { get; }
	}
}
