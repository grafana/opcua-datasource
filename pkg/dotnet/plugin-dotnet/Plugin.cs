
using System;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Pluginv2;
using Grpc.Core.Logging;

namespace plugin_dotnet
{
    class Plugin
    {
        public const string ServiceHost = "localhost";
        public const int ServicePort = 6061;
        public const int AppProtoVersion = 1;

        static async Task Main(string[] args)
        {
            ILogger logger = new ConsoleLogger();

            // Build a server to host the plugin over gRPC
            Server server = new Server
            {
                Ports = { { ServiceHost, ServicePort, ServerCredentials.Insecure } },
                Services = {
                    { Diagnostics.BindService(new DiagnosticsService(logger)) },
                    { Resource.BindService(new ResourceService()) },
                    { Data.BindService(new DataService(logger)) }
                }
            };
            
            server.Start();

            // Part of the go-plugin handshake:
            //  https://github.com/hashicorp/go-plugin/blob/master/docs/guide-plugin-write-non-go.md#4-output-handshake-information
            await Console.Out.WriteAsync($"1|2|tcp|{ServiceHost}:{ServicePort}|grpc\n");
            await Console.Out.FlushAsync();

            while (Console.Read() == -1)
                await Task.Delay(1000);
                
            await server.ShutdownAsync();
        }
    }
}
