using System;
using System.Text;
using System.IO;
using System.Data.Common;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Grpc.Core.Logging;

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
			logger.Debug("DashboardDb starting");

			_filename = filename;

			CreateIfNotExists(filename);
		}

		private void CreateIfNotExists(string filename)
		{
			if (!File.Exists(filename))
			{
				_logger.Debug($"Create db starting: {filename}");

				using (var connection = Sqlite.CreateDatabase(filename, new[] { createPerspectivesDb })) { }
				using (var connection = Sqlite.CreateDatabase(filename, new[] { createDashboardDb })) { }
				using (var connection = Sqlite.CreateDatabase(filename, new[] { createRelationsDb })) { }

				using (var connection = Sqlite.ConnectDatabase(filename))
				{
					var sql = "INSERT INTO Perspectives (Name) VALUES ('Operator')";
					try
					{
						using (DbCommand cmd = connection.CreateCommand())
						{
							cmd.CommandText = sql;
							cmd.CommandType = CommandType.Text;
							cmd.ExecuteNonQuery();
						}
					}
					catch (Exception e)
					{
						_logger.Error(e, "Inserting default perspective failed");
						return;
					}

				}
				_logger.Debug("Create db success");
			}
		}

		public bool QueryDashboard(string[] dashKeys, string perspective, out DashboardData dashboard)
		{
			if (dashKeys?.Length > 0)
			{
				using (var connection = Sqlite.ConnectDatabase(_filename))
				{
					try
					{
						int dashboardId = GetDashboardId(dashKeys, perspective, connection);

						if (dashboardId > 0)
						{
							var dashboardName = GetDashboardName(dashboardId, connection);
							dashboard = new DashboardData(dashboardId, dashboardName);
							return true;
						}
					}
					catch (Exception e)
					{
						_logger.Error(e, $"QueryDashboard query failed for nodeid(s): '{ToTabString(dashKeys)}' and perspective: '{perspective}'");
					}
				}

				dashboard = null;
				return false;
			}
			else
			{
				var msg = "Dashboard query failed: at least one NodeId must be supplied";
				_logger.Error(msg);
				throw new ArgumentException(msg);
			}
		}

		private string GetDashboardName(int dashboardId, SQLiteConnection connection)
		{
			using (DbCommand cmd = connection.CreateCommand())
			{
				var sql = "SELECT Dashboard from Dashboards WHERE Id == $nodeId";

				DbParameter parNodeId = new SQLiteParameter($"$nodeId", dashboardId);
				cmd.Parameters.Add(parNodeId);
				cmd.CommandText = sql;
				cmd.CommandType = CommandType.Text;

				using (var rdr = cmd.ExecuteReader())
				{
					if (rdr.Read())
					{
						var name = rdr.GetString(0);
						return name;
					}
				}
			}

			return string.Empty;
		}

		private int GetDashboardIdCount(int id, SQLiteConnection connection)
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
				_logger.Error(e, $"GetDashRelationCount failed for DashboardId: '{id}'");
			}

			return -1;
		}

		public UaResult AddDashboardMapping(string[] nodeIds, string dashboard, string perspective)
		{
			if (nodeIds?.Length > 0)
			{
				RemoveDashboardMapping(nodeIds, perspective);

				var addRes = AddDashboard(dashboard, perspective);
				int dashboardId;
				if (addRes.Success)
					dashboardId = addRes.Value;
				else
					return new UaResult() { success = false, error = addRes.Error };

				for (int i = 0; i < nodeIds.Length; i++)
				{
					_logger.Debug($"AddDashboardMapping for nodeid: '{nodeIds[i]}' to dashboard '{dashboard}'");

					using (var connection = Sqlite.ConnectDatabase(_filename))
					{
						var sql = "INSERT INTO DashboardRelations (NodeId, DashboardId) "
							+ "VALUES ($nodeId, " + dashboardId + ")";

						_logger.Debug("AddDashboardMapping sql: " + sql);

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
									_logger.Error(msg);

									return new UaResult() { success = false, error = msg };
								}
							}
						}
						catch (Exception e)
						{
							var msg = $"AddDashboardMapping failed for nodeid = '{nodeIds[i]}', dashboard = '{dashboard}', perspective = '{perspective}'";
							_logger.Error(e, msg);
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
					_logger.Error(e, $"RemoveIfExists query failed for nodeid: '{nodeId}' and perspective: '{perspective}'");
				}
			}
		}

		public UaResult RemoveDashboardMapping(string[] nodeIds, string perspective)
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
							var relationsCount = GetDashboardIdCount(dashboardId, connection);
							if (relationsCount == nodeIds.Length)
								RemoveMappingForDashboard(dashboardId, connection);
						}
					}
					catch (Exception e)
					{
						var msg = $"RemoveMapping query failed for nodeid(s): '{ToTabString(nodeIds)}' and perspective: '{perspective}'";
						_logger.Error(e, msg);
						return new UaResult() { success = false, error = msg };
					}
				}
			}

			return new UaResult();
		}

		private void RemoveMappingForDashboard(int dashboardId, SQLiteConnection connection)
		{
			if (!DeleteFromDashboardRelations(dashboardId, connection))
				_logger.Error($"Unable to delete row(s) from DashboardRelations where DashboardId = '{dashboardId}'");

			if (!DeleteFromDashboard(dashboardId, connection))
				_logger.Error($"Unable to delete row from Dashboards where DashboardId = '{dashboardId}'");
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
			List<int>[] dashIds = new List<int>[nodeIds.Length];

			StringBuilder nodeIdsSql = new StringBuilder();

			var sql = "SELECT DashboardId FROM DashboardRelations "
						+ "INNER JOIN Dashboards ON Dashboards.Id == DashboardRelations.DashboardId "
						+ "INNER JOIN Perspectives ON Dashboards.PerspectiveId == Perspectives.Id "
						+ "AND Perspectives.Name == $perspective WHERE NodeId == $nodeId";

			for (int i = 0; i < nodeIds.Length; i++)
			{
				_logger.Debug($"GetDashboardId for nodeid: '{nodeIdsSql}' and perspective: '{perspective}'");

				dashIds[i] = new List<int>();


				using (DbCommand cmd = connection.CreateCommand())
				{
					DbParameter parNodeId = new SQLiteParameter($"$nodeId", nodeIds[i]);
					DbParameter parPerspective = new SQLiteParameter("$perspective", perspective);
					cmd.Parameters.Add(parNodeId);
					cmd.Parameters.Add(parPerspective);
					cmd.CommandText = sql;
					cmd.CommandType = CommandType.Text;

					using (var rdr = cmd.ExecuteReader())
					{
						while (rdr.Read())
						{
							int dashboardId = rdr.GetInt32(0);
							int relationsCount = GetDashboardIdCount(dashboardId, connection);
							if(relationsCount == nodeIds.Length)
								dashIds[i].Add(dashboardId);
						}
					}
				}

				if (dashIds[i].Count == 0)
					return -1;
			}

			return FindCommonNumber(dashIds);
		}

		private int FindCommonNumber(List<int>[] candidateList)
		{
			for (int i = 0; i < candidateList[0].Count; i++)
			{
				var found = true;

				var candidate = candidateList[0][i];
				for (int c = 1; c < candidateList.Length; c++)
				{
					if (!candidateList[c].Any(d => d == candidate)) 
					{
						found = false;
						break;
					}
				}

				if (found)
					return candidate;
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
					_logger.Error(e, msg);
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

	}
}
