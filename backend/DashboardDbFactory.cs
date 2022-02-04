using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

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
                
                dbFolder = Environment.GetEnvironmentVariable("GF_PLUGIN_LIB_PATH"); //Path.Combine("/var", "lib", "grafana-opcua-datasource");
                logger.LogDebug($"Found path for db '{dbFolder}'");
            }

            try
            {                
                if(!Directory.Exists(dbFolder))
                    Directory.CreateDirectory(dbFolder);
            }
            catch(Exception e)
            {
                logger.LogError($"Creating directory '{dbFolder}' failed: '{e.Message}'", e);
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
                    logger.LogError($"Moving old dashboardmapping.db failed: '{e.Message}'", e);
                }
            }
        }
    }
}
