using Microsoft.Extensions.Logging;
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
		bool QueryDashboard(string nodeId, string perspective, out string dashboard);
	}

	public class DashboardDb : IDashboardDb
	{

		private readonly ILogger _logger;
		private string _filename;

		private static readonly string createDashboardDb = @"CREATE TABLE ""DashboardMap"" (
				""Id""	INTEGER,
				""NodeId""	TEXT NOT NULL,
				""NodeType""	INTEGER NOT NULL,
				""Dashboard""	TEXT NOT NULL,
				""Perspective""	TEXT,
				PRIMARY KEY(""Id"" AUTOINCREMENT),
				UNIQUE(""NodeId"",""NodeType"",""Dashboard"",""Perspective"")
			);";


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

				using (var connection = Sqlite.CreateDatabase(filename, new[] { createDashboardDb })) { }

				_logger.LogDebug("Create db success");
			}
		}

		public bool QueryDashboard(string nodeId, string perspective, out string dashboard)
		{
			_logger.LogDebug($"Query for dashboard for nodeid: '{nodeId}'");
			using (var connection = Sqlite.ConnectDatabase(_filename))
			{
				//TODO: use params
				var sql = "SELECT Dashboard from DashboardMap" +
					" where NodeId=$nodeId" +
					" AND (Perspective = $perspective Or Perspective is NULL) ORDER by Perspective DESC";

				try
				{
					using (DbCommand cmd = connection.CreateCommand())
					{
						DbParameter parNodeId = new SQLiteParameter("$nodeId", nodeId);
						DbParameter parPerspective = new SQLiteParameter("$perspective", perspective);
						cmd.Parameters.Add(parNodeId);
						cmd.Parameters.Add(parPerspective);
						cmd.CommandText = sql;
						cmd.CommandType = CommandType.Text;
						var result = cmd.ExecuteReader();

						if(result.Read())
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

					//var result = Sqlite.ExecuteScalar<string>(connection, sql);

					//if (!string.IsNullOrEmpty(result))
					//{
					//	dashboard = result;
					//	_logger.LogDebug($"Found dashboard for nodeid '{nodeId}': {dashboard}");

					//	return true;
					//}
				}
				catch (Exception e)
				{
					dashboard = string.Empty;
					_logger.LogError(e, $"Dashboard query failed for nodeid: '{nodeId}'");
				}
			}
			dashboard = string.Empty;
			return false;
		}


	}
}
