
using System;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Serilog;
using Models;
using Pluginv2;


namespace plugin_dotnet
{
    class Plugin
    {
        public const string ServiceHost = "localhost";
        public const int ServicePort = 6061;
        public const int AppProtoVersion = 1;

        static async Task Main(string[] args)
        {
            // go-plugin semantics depend on the Health Check service from gRPC
            var health = HealthService.Get();
            health.SetStatus("plugin", HealthStatus.Serving);

            // ILoggerFactory loggerFactory = LoggerFactory.Create(builder => {
            //     builder.AddDebug(); 
            //     builder.AddConsole();
            // });
            // ILogger<Plugin> logger = loggerFactory.CreateLogger<Plugin>();
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("/tmp/log.txt")
                .CreateLogger();

            // Build a server to host the plugin over gRPC
            var server = new Server
            {
                Ports = { { ServiceHost, ServicePort, ServerCredentials.Insecure } },
                Services = {
                    { HealthService.BindService(health) },
                    { DatasourcePlugin.BindService(new OpcUaDatasource(logger)) }
                   // { Core.BindService(new CoreService(logger)) },
                   // { StreamingPlugin.BindService(new OpcUaStreaming(logger)) }
                },
            };

            server.Start();

            // Part of the go-plugin handshake:
            //  https://github.com/hashicorp/go-plugin/blob/master/docs/guide-plugin-write-non-go.md#4-output-handshake-information
            await Console.Out.WriteAsync($"1|1|tcp|{ServiceHost}:{ServicePort}|grpc\n");
            await Console.Out.FlushAsync();

            while (Console.Read() == -1)
                await Task.Delay(1000);
                
            await server.ShutdownAsync();
        }
    }
}
