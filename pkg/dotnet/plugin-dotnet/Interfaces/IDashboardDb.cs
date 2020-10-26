using System;
using System.Collections.Generic;
using System.Text;

namespace plugin_dotnet
{
	public interface IDashboardDb
	{
		bool QueryDashboard(string[] nodeIds, string perspective, out string dashboard);
		UaResult AddDashboardMapping(string[] nodeIds, string dashboard, string perspective);
	}

}
