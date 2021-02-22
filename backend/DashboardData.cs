using System;
using System.Collections.Generic;
using System.Text;

namespace plugin_dotnet
{
	public class DashboardData
	{
		public DashboardData(int id, string name)
		{
			Id = id;
			Name = name;
		}

		public int Id { get; }
		public string Name { get; }
	}
}
