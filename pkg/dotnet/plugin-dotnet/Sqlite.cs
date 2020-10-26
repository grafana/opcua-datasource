using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;

namespace plugin_dotnet
{
	public class Sqlite
	{

		public const string _connectionStringTemplate = "Data Source={0}; Version=3; JournalMode = Off; Synchronous=Off; Enlist=N;foreign keys=true;";
		public const string _requireExisting = " FailIfMissing=True;";

		public static SQLiteConnection ConnectDatabase(string connectionStringTemplate, string filename, string requireExisting)
		{
			var connectionString = string.Format(connectionStringTemplate, filename) + requireExisting;
			return new SQLiteConnection(connectionString, true).OpenAndReturn();
		}

		public static SQLiteConnection ConnectDatabase(string filename)
		{
			return ConnectDatabase(_connectionStringTemplate, filename, _requireExisting);
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

				connection.Open();
				foreach (var create_table_statement in tableCreates)
				{
					ExecuteVoidCommand(connection, create_table_statement);
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

}
