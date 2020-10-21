using Microsoft.Extensions.Logging;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace plugin_dotnet
{
	public class Sqlite
	{

		public const string _connectionStringTemplate = "Data Source={0}; Version=3; JournalMode = Off; Synchronous=Off; Enlist=N;foreign keys=true;";
		public const string _requireExisting = " FailIfMissing=True;";
		

		public static SQLiteConnection ConnectDatabase(string connectionStringTemplate, string filename, string requireExisting)
		{
			var connectionString = string.Format(connectionStringTemplate, filename) + requireExisting;
			try
			{
				//_log.DebugFormat("ConnectDatabase for {0}", connectionString);
				return new SQLiteConnection(connectionString, true).OpenAndReturn();
			}
			catch (Exception e)
			{
				//_log.Error(string.Format("Unable to open database connection for file '{0}'", filename), e);
				throw;
			}
		}

		public static SQLiteConnection ConnectDatabase(string filename)
		{
			return ConnectDatabase(_connectionStringTemplate, filename, _requireExisting);
		}

		internal static T ExecuteScalar<T>(SQLiteConnection connection, string commandText)
		{
			using (DbCommand cmd = connection.CreateCommand())
			{

				cmd.CommandText = commandText;
				cmd.CommandType = CommandType.Text;
				var value = (T)cmd.ExecuteScalar();
				return value;
			}
		}

		internal static int ExecuteVoidCommand(SQLiteConnection connection, string commandText)
		{
			using (DbCommand cmd = connection.CreateCommand())
			{
				cmd.CommandText = commandText;
				cmd.CommandType = CommandType.Text;
				var numRowsAffected = cmd.ExecuteNonQuery();
				return numRowsAffected;
			}
		}

		internal static SQLiteConnection CreateDatabase(string filename, string[] tableCreates)
		{
			if (EnsurePath(filename))
			{
				var connectionString = string.Format(_connectionStringTemplate, filename);

				var connection = new SQLiteConnection(connectionString, true);

				try
				{
					connection.Open();
					foreach (var create_table_statement in tableCreates)
					{
						ExecuteVoidCommand(connection, create_table_statement);
					}

				}
				catch (Exception e)
				{
					//_log.Error(string.Format("Unable to create database for file '{0}'", filename), e);
					throw;
				}

				return connection;
			}

			throw new ArgumentException($"Unable to create database {filename}", nameof(filename));
		}

		internal static bool EnsurePath(string fileName)
		{

			int pathEnd = fileName.LastIndexOf(Path.DirectorySeparatorChar);
			string path = fileName.Substring(0, pathEnd);

			var info = Directory.CreateDirectory(path);
			return info.Exists;
		}

	}

	public interface IDashboardDb
	{
		bool QueryDashboard(string[] nodeIds, string perspective, out string dashboard);
		UaResult AddDashboardMapping(string[] nodeIds, NodeClass[] nodeClasses, string dashboard, string perspective);
	}

	public class DashboardDb : IDashboardDb
	{
		private readonly ILogger _logger;
		private readonly string _filename;

		private static readonly string createPerspectivesDb = @"CREATE TABLE ""Perspectives"" (
				""Id""	INTEGER,
				""Name""	TEXT NOT NULL UNIQUE,
				PRIMARY KEY(""Id"")
			)";

		private static readonly string createDashboardDb = @"CREATE TABLE ""Dashboards""(
				""Id""   INTEGER,
				""Dashboard""    TEXT NOT NULL,
				""PerspectiveId""  INTEGER,
				PRIMARY KEY(""Id"" AUTOINCREMENT),
				FOREIGN KEY(""PerspectiveId"") REFERENCES ""Perspectives""
			)";

		private static readonly string createRelationsDb = @"CREATE TABLE ""DashboardRelations"" (
				""NodeId""	TEXT NOT NULL,
				""NodeType""	INTEGER,
				""GroupCount""	INTEGER,
				""DashboardId""	INTEGER,
				FOREIGN KEY(""DashboardId"") REFERENCES ""Dashboards""
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
				//TODO: Iterate over nodeIds and find the distinct dashboard that matches for all nodeids for GroupCount (number of nodeids) and Perspective

				var nodeId = nodeIds[0];
				_logger.LogDebug($"Query for dashboard for nodeid: '{nodeId}'");
				using (var connection = Sqlite.ConnectDatabase(_filename))
				{
					var sql = "SELECT Dashboards.Dashboard "
							+ "FROM Dashboards "
							+ "INNER JOIN DashboardRelations "
							+ "ON DashboardRelations.NodeId == $nodeId "
							+ "AND DashboardRelations.GroupCount == $groupCount "
							+ "AND DashboardRelations.DashboardId == Dashboards.Id "
							+ "INNER JOIN Perspectives "
							+ "ON Dashboards.PerspectiveId == Perspectives.Id "
							+ "AND Perspectives.Name == $perspective ";

					try
					{
						using (DbCommand cmd = connection.CreateCommand())
						{
							DbParameter parNodeId = new SQLiteParameter("$nodeId", nodeId);
							DbParameter parGroupCount = new SQLiteParameter("$groupCount", nodeIds.Length);
							DbParameter parPerspective = new SQLiteParameter("$perspective", perspective);
							cmd.Parameters.Add(parNodeId);
							cmd.Parameters.Add(parGroupCount);
							cmd.Parameters.Add(parPerspective);
							cmd.CommandText = sql;
							cmd.CommandType = CommandType.Text;
							var result = cmd.ExecuteReader();

							if (result.Read())
							{
								var r = result.GetString(0);
								if (!string.IsNullOrEmpty(r))
								{
									dashboard = r;
									_logger.LogDebug($"Found dashboard for nodeid '{nodeId}': {dashboard}");

									return true;
								}
							}
						}
					}
					catch (Exception e)
					{
						dashboard = string.Empty;
						_logger.LogError(e, $"Dashboard query failed for nodeid: '{nodeId}'");
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

		public UaResult AddDashboardMapping(string[] nodeIds, NodeClass[] nodeClasses, string dashboard, string perspective)
		{
			if (nodeIds?.Length > 0)
			{
				if(nodeClasses?.Length == nodeIds.Length)
				{
					if(!DoesDashboardExist(dashboard, perspective))
					{
						var addRes = AddDashboard(dashboard, perspective);
						if (!addRes.success)
							return addRes;
					}

					for (int i = 0; i < nodeIds.Length; i++)
					{
						_logger.LogDebug($"AddDashboardMapping for nodeid: '{nodeIds[i]}' to dashboard '{dashboard}'");

						using (var connection = Sqlite.ConnectDatabase(_filename))
						{
							var sql = "INSERT INTO DashboardRelations (NodeId, NodeType, GroupCount, DashboardId) "
								+ "VALUES ($nodeId, $nodeClass, $groupCount, "
								+ "(SELECT Dashboards.Id FROM Dashboards "
								+ "INNER JOIN Perspectives "
								+ "ON Dashboards.Dashboard == $dashboard AND Dashboards.PerspectiveId == Perspectives.Id "
								+ "AND Perspectives.Name == $perspective)"
								+ ")";

							_logger.LogDebug("AddDashboardMapping sql: " + sql);

							try
							{
								using (DbCommand cmd = connection.CreateCommand())
								{
									DbParameter parNodeId = new SQLiteParameter("$nodeId", nodeIds[i]);
									DbParameter parNodeClass = new SQLiteParameter("$nodeClass", nodeClasses[i]);
									DbParameter parGroupCount = new SQLiteParameter("$groupCount", nodeIds.Length);
									DbParameter parDashboard = new SQLiteParameter("$dashboard", dashboard);
									DbParameter parPerspective = new SQLiteParameter("$perspective", perspective);
									cmd.Parameters.Add(parNodeId);
									cmd.Parameters.Add(parNodeClass);
									cmd.Parameters.Add(parGroupCount);
									cmd.Parameters.Add(parDashboard);
									cmd.Parameters.Add(parPerspective);
									cmd.CommandText = sql;
									cmd.CommandType = CommandType.Text;
									var result = cmd.ExecuteNonQuery();

									if (result > 0)
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
				return new UaResult() { success = false, error = "NodeIds must be accompanied with NodeClasses" };
			}
			return new UaResult() { success = false, error = "No NodeIds specified" };
		}

		private UaResult AddDashboard(string dashboard, string perspective)
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
						var result = cmd.ExecuteNonQuery();

						var success = result > 0;
						var error = success ? string.Empty : "Adding dashboard failed";
						return new UaResult() { success = result > 0, error = error };
					}
				}
				catch (Exception e)
				{
					var msg = $"AddDashboard failed for dashboard = '{dashboard}' and perspective = '{perspective}'";
					_logger.LogError(e, msg);
					return new UaResult() { success = false, error = msg };
				}
			}
		}

		private bool DoesDashboardExist(string dashboard, string perspective)
		{
			using (var connection = Sqlite.ConnectDatabase(_filename))
			{
				var sql = "SELECT Dashboards.Id "
					+ "FROM Dashboards, Perspectives "
					+ "WHERE Dashboards.Dashboard == $dashboard "
					+ "AND Dashboards.PerspectiveId == Perspectives.Id "
					+ "AND Perspectives.Name == $perspective";

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
						var result = cmd.ExecuteReader();

						return result.Read();
					}
				}
				catch (Exception e)
				{
					_logger.LogError(e, $"DoesDashboardExist failed for dashboard: '{dashboard}'");
				}
			}

			return false;
		}
	}
}
