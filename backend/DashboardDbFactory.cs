using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Grpc.Core.Logging;

namespace plugin_dotnet
{
    internal class DashboardDbFactory
    {
        public static IDashboardDb CreateDashboardDb(ILogger logger)
        {
            string dbFolder;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                dbFolder = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.CommonApplicationData), "Grafana", "grafana-opcua-datasource");
            }
            else
            {
                dbFolder = Environment.GetEnvironmentVariable("GF_PLUGIN_DATA_DIR");
                if (dbFolder.Length == 0)
                {
                    dbFolder = Path.Combine("/var", "lib", "grafana-opcua-datasource");
                    try
                    {
                        if (!Directory.Exists(dbFolder))
                        {
                            dbFolder = Path.Combine("/tmp", "grafana_opcua_datasource");
                            if (!Directory.Exists(dbFolder))
                            {
                                Directory.CreateDirectory(dbFolder);
                            }
                            // After every other reasonable effort, default to a path in the working directoy
                            dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbs");
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Creating directory '{dbFolder}' failed: '{e.Message}'", e);
                    }
                }
                logger.Debug($"Found path for db '{dbFolder}'");
            }

            

            var dbFile = Path.Combine(dbFolder, "dashboardmapping.db");

            MoveOldFiles(dbFile, logger);

            return new DashboardDb(logger, dbFile);
        }

        private static void MoveOldFiles(string dbFile, ILogger logger)
        {
            var oldFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbs");
            var oldPath = Path.Combine(oldFolder, "dashboardmapping.db");

            if (File.Exists(oldPath))
            {
                try
                {
                    if(!File.Exists(dbFile))
                    {
                        File.Move(oldPath, dbFile);
                    }
                    else
                    {
                        File.Delete(oldPath);
                    }

                    if (Directory.Exists(oldFolder) && Directory.GetFiles(oldFolder).Length == 0)
                        Directory.Delete(oldFolder);
                }
                catch (Exception e)
                {
                    logger.Error($"Moving old dashboardmapping.db failed: '{e.Message}'", e);
                }
            }
        }
    }
}
