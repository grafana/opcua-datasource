using System;
using System.Text;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Data.Common;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Collections.Generic;

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

		public bool QueryDashboard(string[] dashKeys, bool requireAllKeys, string perspective, out DashboardData dashboard)
		{
			if (dashKeys?.Length > 0)
			{
				var dashboardFound = requireAllKeys ? QueryDashboardAnd(dashKeys, perspective, out dashboard) : QueryDashboardOr(dashKeys, perspective, out dashboard);
				//var dashboardFound = requireAllKeys ? QueryDashboardOr(dashKeys, perspective, out dashboard) : QueryDashboardOr(dashKeys, perspective, out dashboard);

				return dashboardFound;
			}
			else
			{
				var msg = "Dashboard query failed: at least one NodeId must be supplied";
				_logger.LogError(msg);
				throw new ArgumentException(msg);
			}
		}

		private bool QueryDashboardAnd(string[] dashKeys, string perspective, out DashboardData dashboard)
		{
			_logger.LogDebug($"AND Query for dashboard for nodeid: '{ AddUpString(dashKeys) }' and perspective: '{perspective}'");

			var dashboardFound = false;
			dashboard = null;

			List<DashboardData>[] foundDashboardSet = new List<DashboardData>[dashKeys.Length];

			using (var connection = Sqlite.ConnectDatabase(_filename))
			{

				for (int i = 0; i < dashKeys.Length; i++)
				{
					var sql = "SELECT DISTINCT DashboardId, Dashboard FROM DashboardRelations "
							//var sql = "SELECT Dashboard FROM DashboardRelations "
							+ "INNER JOIN Dashboards ON Dashboards.Id == DashboardRelations.DashboardId "
							+ "INNER JOIN Perspectives ON Dashboards.PerspectiveId == Perspectives.Id "
							+ "AND Perspectives.Name == $perspective WHERE NodeId == $nodeId";

					try
					{
						using (DbCommand cmd = connection.CreateCommand())
						{
							DbParameter parNodeId = new SQLiteParameter($"$nodeId", dashKeys[i]);
							cmd.Parameters.Add(parNodeId);
							DbParameter parPerspective = new SQLiteParameter("$perspective", perspective);
							cmd.Parameters.Add(parPerspective);
							cmd.CommandText = sql;
							cmd.CommandType = CommandType.Text;

							using (var rdr = cmd.ExecuteReader())
							{
								foundDashboardSet[i] = new List<DashboardData>();
								while (rdr.Read())
								{
									var id = rdr.GetInt32(0);
									var name = rdr.GetString(1);
									if (!string.IsNullOrEmpty(name))
									{
										var dashboardData = new DashboardData(id, name);
										foundDashboardSet[i].Add(dashboardData);
										dashboardFound = true;
										_logger.LogDebug($"Found dashboard '{dashboard}' for nodeid(s) '{dashKeys[i]}': ");
									}
									else
									{
										_logger.LogError($"Database returned unexpected empty result for nodeid(s) '{dashKeys[i]}'");
										dashboardFound = false;
										break;
									}
								}
							}
						}
					}
					catch (Exception e)
					{
						dashboardFound = false;
						_logger.LogError(e, $"Dashboard query failed for nodeid(s): '{dashKeys[i]}' and perspective: '{perspective}'");
					}
				}
			}

			if(dashboardFound)
			{
				var dashboardData = ReconcileDashboard(foundDashboardSet, dashKeys.Length);

				dashboardFound = dashboardData != null;

				if (dashboardFound)
					dashboard = dashboardData;
			}

			return dashboardFound;
		}

		private int GetDashRelationCount(int id)
		{
			using (var connection = Sqlite.ConnectDatabase(_filename))
			{
				var sql = $"SELECT COUNT(*) FROM DashboardRelations WHERE DashboardId == '{id}'";

				try
				{
					using (DbCommand cmd = connection.CreateCommand())
					{
						cmd.CommandText = sql;
						cmd.CommandType = CommandType.Text;

						using (var rdr = cmd.ExecuteReader())
						{
							if(rdr.Read())
							{
								var count = rdr.GetInt32(0);
								return count;
							}
						}
					}
				}
				catch (Exception e)
				{
					_logger.LogError(e, $"GetDashRelationCount failed for DashboardId: '{id}'");
				}
			}

			return -1;
		}

		private DashboardData ReconcileDashboard(List<DashboardData>[] dashboardSet, int relationCount)
		{
			if(dashboardSet.Length == 1 && dashboardSet[0].Count > 0)
			{
				for (int i = 0; i < dashboardSet[0].Count; i++)
				{
					var dashboardData = dashboardSet[0][i];
					var rCount = GetDashRelationCount(dashboardData.Id);

					if (rCount == relationCount)
						return dashboardData;
				}
			}
			else if(dashboardSet.Length > 1)
			{
				for(int i = 0; i < dashboardSet[0].Count; i++)
				{
					var dashCandidate = dashboardSet[0][i];

					var rCount = GetDashRelationCount(dashCandidate.Id);
					if (rCount == relationCount)
					{
						bool found = LookupDashboard(dashCandidate, dashboardSet);

						if (found)
							return dashCandidate;
					}
				}
			}

			return null;
		}

		private bool LookupDashboard(DashboardData dashCandidate, List<DashboardData>[] dashboardSet)
		{
			for (int i = 1; i < dashboardSet.Length; i++)
			{
				if (!dashboardSet[i].Any(d => d.Id == dashCandidate.Id))
					return false;
			}

			return true;
		}

		private bool QueryDashboardOr(string[] dashKeys, string perspective, out DashboardData dashboard)
		{
			var dashboardFound = false;
			dashboard = null;

			StringBuilder nodeIdsSql = new StringBuilder();

			for (int i = 0; i < dashKeys.Length; i++)
			{
				nodeIdsSql.Append($"NodeId == $nodeId{i} ");
				if (i < dashKeys.Length - 1)
					nodeIdsSql.Append(" OR ");
			}

			_logger.LogDebug($"OR Query for dashboard for nodeid: '{nodeIdsSql}' and perspective: '{perspective}'");
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
						for (int i = 0; i < dashKeys.Length; i++)
						{
							DbParameter parNodeId = new SQLiteParameter($"$nodeId{i}", dashKeys[i]);
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
								var id = rdr.GetInt32(0);
								var name = rdr.GetString(1);
								if (!string.IsNullOrEmpty(name))
								{
									dashboard = new DashboardData(id, name);
									_logger.LogDebug($"Found dashboard '{dashboard.Name}' for nodeid(s) '{nodeIdsSql}': ");

									dashboardFound = true;
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					_logger.LogError(e, $"Dashboard query failed for nodeid(s): '{nodeIdsSql}' and perspective: '{perspective}'");
				}
			}

			return dashboardFound;
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

								if (numRowsAffected == 0)
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

				return new UaResult() { success = true };
			}

			return new UaResult() { success = false, error = "No NodeIds specified" };
		}

		public void RemoveMapping(string nodeId, string perspective)
		{
			using (var connection = Sqlite.ConnectDatabase(_filename))
			{
				try
				{
					int dashboardId = GetDashboardId(new[] { nodeId }, perspective, connection);

					if (dashboardId > 0)
					{
						RemoveMappingForDashboard(dashboardId, connection);
					}
				}
				catch (Exception e)
				{
					_logger.LogError(e, $"RemoveIfExists query failed for nodeid: '{nodeId}' and perspective: '{perspective}'");
				}
			}
		}

		private void RemoveIfMappingExists(string[] nodeIds, string perspective)
		{
			if (nodeIds?.Length > 0 && !string.IsNullOrEmpty(perspective))
			{
				using (var connection = Sqlite.ConnectDatabase(_filename))
				{
					try
					{
						int dashboardId = GetDashboardId(nodeIds, perspective, connection);

						if (dashboardId > 0)
						{
							var relationsCount = GetDashRelationCount(dashboardId);
							if (relationsCount == nodeIds.Length)
								RemoveMappingForDashboard(dashboardId, connection);
						}
					}
					catch (Exception e)
					{
						_logger.LogError(e, $"RemoveIfExists query failed for nodeid(s): '{ToTabString(nodeIds)}' and perspective: '{perspective}'");
					}
				}
			}

		}

		private void RemoveMappingForDashboard(int dashboardId, SQLiteConnection connection)
		{
			if (!DeleteFromDashboardRelations(dashboardId, connection))
				_logger.LogError($"Unable to delete row(s) from DashboardRelations where DashboardId = '{dashboardId}'");

			if (!DeleteFromDashboard(dashboardId, connection))
				_logger.LogError($"Unable to delete row from Dashboards where DashboardId = '{dashboardId}'");
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

			_logger.LogDebug($"GetDashboardId for nodeid: '{nodeIdsSql}' and perspective: '{perspective}'");

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

		private object AddUpString(string[] sArr)
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < sArr.Length; i++)
			{
				sb.Append(sArr[i]);
				if (i < sArr.Length - 1)
					sb.Append(", ");
			}

			return sb.ToString();
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
