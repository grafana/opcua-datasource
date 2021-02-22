using System;
using System.Collections.Generic;
using System.Text;

namespace plugin_dotnet
{
	public interface IDashboardDb
	{
		bool QueryDashboard(string[] dashKeys, string perspective, out DashboardData dashboard);
		UaResult AddDashboardMapping(string[] dashKeys, string dashboard, string perspective);
		UaResult RemoveDashboardMapping(string[] dashKeys, string perspective);
		void RemoveMapping(string dashKey, string perspective);
	}

}
