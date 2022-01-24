using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace plugin_dotnet
{
    public static class ServicesExtensions
    {
        public static void ConfigureLogging(this IServiceCollection services)
        {
            var logConfigFile = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "log4net.win.config" : "log4net.config";

            var pathLogConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logConfigFile);

            services.AddLogging(configure => configure.AddLog4Net(pathLogConfig))
                .AddTransient<Plugin>();
            services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Trace);

            var oldLogpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

            if (Directory.Exists(oldLogpath))
                Directory.Delete(oldLogpath, true);
        }
    }
}
