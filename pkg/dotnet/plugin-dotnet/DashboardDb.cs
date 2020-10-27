using System;
using System.Text;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Data.Common;
using System.Data.SQLite;
using System.Data;

namespace plugin_dotnet
{
	public class DashboardDb : IDashboardDb
	{
		private readonly ILogger _logger;
		private readonly string _filename;
		private static readonly string _lastRowIdQuery = "SELECT last_insert_rowid()";


		private static readonly string createPerspectivesDb = @"CREATE TABLE ""Perspectives"" (
																""Id""	INTEGER,
																""Name""	TEXT NOT NULL UNIQUE,
																PRIMARY KEY(""Id"")
															)";

		private static readonly string createDashboardDb = @"CREATE TABLE ""Dashboards"" (
																""Id""	INTEGER,
																""Dashboard""	TEXT NOT NULL,
																""PerspectiveId""	INTEGER,
																PRIMARY KEY(""Id"" AUTOINCREMENT),
																FOREIGN KEY(""PerspectiveId"") REFERENCES ""Perspectives""
															)";

		private static readonly string createRelationsDb = @"CREATE TABLE ""DashboardRelations"" (
																""Id""	INTEGER,
																""NodeId""	TEXT NOT NULL,
																""DashboardId""	INTEGER NOT NULL,
																FOREIGN KEY(""DashboardId"") REFERENCES ""Dashboards"",
																PRIMARY KEY(""Id"")
															)";

		public DashboardDb(ILogger logger, string filename)
		{
			_logger = logger;
			logger.LogDebug("DashboardDb starting");

			_filename = filename;

			CreateIfNotExists(filename);
		}

		private void CreateIfNotExists(string filename)
		{
			if (!File.Exists(filename))
			{
				_logger.LogDebug($"Create db starting: {filename}");

				using (var connection = Sqlite.CreateDatabase(filename, new[] { createPerspectivesDb })) { }
				using (var connection = Sqlite.CreateDatabase(filename, new[] { createDashboardDb })) { }
				using (var connection = Sqlite.CreateDatabase(filename, new[] { createRelationsDb })) { }

				_logger.LogDebug("Create db success");
			}
		}

		public bool QueryDashboard(string[] nodeIds, string perspective, out string dashboard)
		{
			if (nodeIds?.Length > 0)
			{
				StringBuilder nodeIdsSql = new StringBuilder();

				for (int i = 0; i < nodeIds.Length; i++)
				{
					nodeIdsSql.Append($"NodeId == $nodeId{i} ");
					if (i < nodeIds.Length - 1)
						nodeIdsSql.Append(" OR ");
				}

				_logger.LogDebug($"Query for dashboard for nodeid: '{nodeIdsSql}' and perspective: '{perspective}'");
				using (var connection = Sqlite.ConnectDatabase(_filename))
				{
					var sql = "SELECT DISTINCT Dashboard FROM DashboardRelations "
							+ "INNER JOIN Dashboards ON Dashboards.Id == DashboardRelations.DashboardId "
							+ "INNER JOIN Perspectives ON Dashboards.PerspectiveId == Perspectives.Id "
							+ "AND Perspectives.Name == $perspective WHERE "
							+ nodeIdsSql;

					try
					{
						using (DbCommand cmd = connection.CreateCommand())
						{
							for (int i = 0; i < nodeIds.Length; i++)
							{
								DbParameter parNodeId = new SQLiteParameter($"$nodeId{i}", nodeIds[i]);
								cmd.Parameters.Add(parNodeId);
							}
							DbParameter parPerspective = new SQLiteParameter("$perspective", perspective);
							cmd.Parameters.Add(parPerspective);
							cmd.CommandText = sql;
							cmd.CommandType = CommandType.Text;

							using (var rdr = cmd.ExecuteReader())
							{
								if (rdr.Read())
								{
									var r = rdr.GetString(0);
									if (!string.IsNullOrEmpty(r))
									{
										dashboard = r;
										_logger.LogDebug($"Found dashboard '{dashboard}' for nodeid(s) '{nodeIdsSql}': ");

										return true;
									}
								}
							}
						}
					}
					catch (Exception e)
					{
						dashboard = string.Empty;
						_logger.LogError(e, $"Dashboard query failed for nodeid(s): '{nodeIdsSql}' and perspective: '{perspective}'");
					}
				}
			}
			else
			{
				var msg = "Dashboard query failed: at least one NodeId must be supplied";
				_logger.LogError(msg);
				throw new ArgumentException(msg);
			}

			dashboard = string.Empty;
			return false;
		}

		public UaResult AddDashboardMapping(string[] nodeIds, string dashboard, string perspective)
		{
			if (nodeIds?.Length > 0)
			{
				RemoveIfMappingExists(nodeIds, perspective);

				var addRes = AddDashboard(dashboard, perspective);
				int dashboardId;
				if (addRes.Success)
					dashboardId = addRes.Value;
				else
					return new UaResult() { success = false, error = addRes.Error };

				for (int i = 0; i < nodeIds.Length; i++)
				{
					_logger.LogDebug($"AddDashboardMapping for nodeid: '{nodeIds[i]}' to dashboard '{dashboard}'");

					using (var connection = Sqlite.ConnectDatabase(_filename))
					{
						var sql = "INSERT INTO DashboardRelations (NodeId, DashboardId) "
							+ "VALUES ($nodeId, " + dashboardId + ")";

						_logger.LogDebug("AddDashboardMapping sql: " + sql);

						try
						{
							using (DbCommand cmd = connection.CreateCommand())
							{
								DbParameter parNodeId = new SQLiteParameter("$nodeId", nodeIds[i]);
								cmd.Parameters.Add(parNodeId);
								cmd.CommandText = sql;
								cmd.CommandType = CommandType.Text;
								var numRowsAffected = cmd.ExecuteNonQuery();

								if (numRowsAffected > 0)
								{
									return new UaResult();
								}
								else
								{
									var msg = $"AddDashboardMapping failed for nodeid = '{nodeIds[i]}', dashboard = '{dashboard}', perspective = '{perspective}'";
									_logger.LogError(msg);

									return new UaResult() { success = false, error = msg };
								}
							}
						}
						catch (Exception e)
						{
							var msg = $"AddDashboardMapping failed for nodeid = '{nodeIds[i]}', dashboard = '{dashboard}', perspective = '{perspective}'";
							_logger.LogError(e, msg);
						}
					}
				}
			}
			return new UaResult() { success = false, error = "No NodeIds specified" };
		}

		private void RemoveIfMappingExists(string[] nodeIds, string perspective)
		{
			using (var connection = Sqlite.ConnectDatabase(_filename))
			{
				try
				{
					int dashboardId = GetDashboardId(nodeIds, perspective, connection);

					if (dashboardId > 0)
					{
						if (!DeleteFromDashboardRelations(dashboardId, connection))
							_logger.LogError($"Unable to delete row(s) from DashboardRelations where DashboardId = '{dashboardId}'");

						if(!DeleteFromDashboard(dashboardId, connection))
							_logger.LogError($"Unable to delete row from Dashboards where DashboardId = '{dashboardId}'");
					}
				}
				catch (Exception e)
				{
					_logger.LogError(e, $"RemoveIfExists query failed for nodeid(s): '{ToTabString(nodeIds)}' and perspective: '{perspective}'");
				}
			}
		}

		private bool DeleteFromDashboardRelations(int dashboardId, SQLiteConnection connection)
		{
			var sql = $"DELETE FROM DashboardRelations WHERE DashboardId == {dashboardId}";

			using (DbCommand cmd = connection.CreateCommand())
			{
				cmd.CommandText = sql;
				cmd.CommandType = CommandType.Text;
				var numRowsAffected = cmd.ExecuteNonQuery();

				return numRowsAffected > 0;
			}
		}

		private bool DeleteFromDashboard(int dashboardId, SQLiteConnection connection)
		{
			var sql = $"DELETE FROM Dashboards WHERE Id == {dashboardId}";

			using (DbCommand cmd = connection.CreateCommand())
			{
				cmd.CommandText = sql;
				cmd.CommandType = CommandType.Text;
				var numRowsAffected = cmd.ExecuteNonQuery();

				return numRowsAffected > 0;
			}
		}

		private int GetDashboardId(string[] nodeIds, string perspective, SQLiteConnection connection)
		{
			StringBuilder nodeIdsSql = new StringBuilder();

			for (int i = 0; i < nodeIds.Length; i++)
			{
				nodeIdsSql.Append($"NodeId == $nodeId{i} ");
				if (i < nodeIds.Length - 1)
					nodeIdsSql.Append(" OR ");
			}

			_logger.LogDebug($"RemoveIfExists for nodeid: '{nodeIdsSql}' and perspective: '{perspective}'");

			var sql = "SELECT DISTINCT DashboardId FROM DashboardRelations "
					+ "INNER JOIN Dashboards ON Dashboards.Id == DashboardRelations.DashboardId "
					+ "INNER JOIN Perspectives ON Dashboards.PerspectiveId == Perspectives.Id "
					+ "AND Perspectives.Name == $perspective WHERE "
					+ nodeIdsSql;

			using (DbCommand cmd = connection.CreateCommand())
			{
				for (int i = 0; i < nodeIds.Length; i++)
				{
					DbParameter parNodeId = new SQLiteParameter($"$nodeId{i}", nodeIds[i]);
					cmd.Parameters.Add(parNodeId);
				}
				DbParameter parPerspective = new SQLiteParameter("$perspective", perspective);
				cmd.Parameters.Add(parPerspective);
				cmd.CommandText = sql;
				cmd.CommandType = CommandType.Text;

				using (var rdr = cmd.ExecuteReader())
				{
					if (rdr.Read())
					{
						int dashboardId = rdr.GetInt32(0);
						return dashboardId;
					}
				}
			}

			return -1;
		}

		private UaResult<int> AddDashboard(string dashboard, string perspective)
		{
			using (var connection = Sqlite.ConnectDatabase(_filename))
			{
				var sql = "INSERT INTO Dashboards (Dashboard, PerspectiveId) VALUES ($dashboard, (SELECT Id FROM Perspectives WHERE Perspectives.Name == $perspective))";

				try
				{
					using (DbCommand cmd = connection.CreateCommand())
					{
						DbParameter parDashboard = new SQLiteParameter("$dashboard", dashboard);
						DbParameter parPerspective = new SQLiteParameter("$perspective", perspective);
						cmd.Parameters.Add(parDashboard);
						cmd.Parameters.Add(parPerspective);
						cmd.CommandText = sql;
						cmd.CommandType = CommandType.Text;
						var numRowsAffected = cmd.ExecuteNonQuery();

						if (numRowsAffected > 0)
						{
							cmd.Parameters.Clear();
							cmd.CommandText = _lastRowIdQuery;

							using (var rdr = cmd.ExecuteReader())
							{
								if (rdr.Read())
								{
									var pk = rdr.GetInt32(0);
									return new UaResult<int>() { Value = pk, Success = true, Error = string.Empty };
								}
							}

						}

						return new UaResult<int>() { Value = 0, Success = false, Error = "Adding dashboard failed" };
					}
				}
				catch (Exception e)
				{
					var msg = $"AddDashboard failed for dashboard = '{dashboard}' and perspective = '{perspective}': '{e.Message}'";
					_logger.LogError(e, msg);
					return new UaResult<int>() { Value = 0, Success = false, Error = msg };
				}
			}
		}

		private static string ToTabString(string[] stringArr)
		{
			StringBuilder tabSep = new StringBuilder();

			if(stringArr != null)
			{
				foreach(var s in stringArr)
				{
					tabSep.Append(s);
					tabSep.Append('\t');
				}
			}

			return tabSep.ToString();
		}

		//private bool DoesDashboardExist(string dashboard, string perspective)
		//{
		//	using (var connection = Sqlite.ConnectDatabase(_filename))
		//	{
		//		var sql = "SELECT Dashboards.Id "
		//			+ "FROM Dashboards, Perspectives "
		//			+ "WHERE Dashboards.Dashboard == $dashboard "
		//			+ "AND Dashboards.PerspectiveId == Perspectives.Id "
		//			+ "AND Perspectives.Name == $perspective";

		//		try
		//		{
		//			using (DbCommand cmd = connection.CreateCommand())
		//			{
		//				DbParameter parDashboard = new SQLiteParameter("$dashboard", dashboard);
		//				DbParameter parPerspective = new SQLiteParameter("$perspective", perspective);
		//				cmd.Parameters.Add(parDashboard);
		//				cmd.Parameters.Add(parPerspective);
		//				cmd.CommandText = sql;
		//				cmd.CommandType = CommandType.Text;

		//				bool exists = false;
		//				using (var rdr = cmd.ExecuteReader())
		//				{
		//					exists = rdr.Read();
		//				}
		//				return exists;
		//			}
		//		}
		//		catch (Exception e)
		//		{
		//			_logger.LogError(e, $"DoesDashboardExist failed for dashboard: '{dashboard}'");
		//		}
		//	}

		//	return false;
		//}
	}
}
