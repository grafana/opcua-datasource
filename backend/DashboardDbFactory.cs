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
            string dbFile;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                dbFile = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.CommonApplicationData), "Grafana", "grafana-opcua-datasource", "dashboardmapping.db");
            }
            else
            {
                dbFile = Path.Combine(Path.PathSeparator + "var", "lib", "grafana-opcua-datasource");
            }

            var oldPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbs", "dashboardmapping.db");

            if (File.Exists(oldPath))
            {
                try
                {
                    var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbs");
                    File.Move(oldPath, dbFile);
                    if (Directory.Exists(directory) && Directory.GetFiles(directory).Length == 0)
                        Directory.Delete(directory);
                }
                catch (Exception e)
                {
                    logger.LogError($"Moving old dashboardmapping.db failed: '{e.Message}'", e);
                }
            }

            return new DashboardDb(logger, dbFile);
        }

    }
}
