
using System;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pluginv2;

namespace plugin_dotnet
{
    class Plugin
    {
        public const int AppProtoVersion = 1;

        private static void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureLogging();

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();
            services.AddSingleton<IConfiguration>(configuration);
        }

        static async Task Main(string[] args)
        {

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            ILogger logger = serviceProvider.GetService<ILogger<Plugin>>();
            logger.LogDebug("Ua plugin starting");
            var configuration = serviceProvider.GetService<IConfiguration>();
            var servicePort = configuration.GetValue("ServicePort", 6061);
            var serviceHost = configuration.GetValue("ServiceHost", "localhost");
            logger.LogDebug("Port: " + servicePort);
            try
            {
                var traceLogConverter = new TraceLogConverter(logger);
                Prediktor.Log.LogManager.TraceLogFactory = (name => traceLogConverter);
                var nodeCacheFactory = new NodeCacheFactory(logger);
                var connections = new Connections(logger, new Prediktor.UA.Client.SessionFactory(c => true), 
                    nodeCacheFactory, ApplicationConfigurationFactory.CreateApplicationConfiguration);

                IDashboardDb dashboardDb = DashboardDbFactory.CreateDashboardDb(logger);
                IDashboardResolver dashboardResolver = new DashboardResolver(dashboardDb);

                // Build a server to host the plugin over gRPC
                Server server = new Server
                {
                    Ports = { { serviceHost, servicePort, ServerCredentials.Insecure } },
                    Services = {
                    { Diagnostics.BindService(new DiagnosticsService(logger, connections)) },
                    { Resource.BindService(new ResourceService(logger, connections, dashboardResolver)) },
                    { Data.BindService(new DataService(logger, connections, nodeCacheFactory)) },
                    { Pluginv2.Stream.BindService(new StreamService(logger, connections))},
                }
                };

                server.Start();

                // Part of the go-plugin handshake:
                //  https://github.com/hashicorp/go-plugin/blob/master/docs/guide-plugin-write-non-go.md#4-output-handshake-information
                await Console.Out.WriteAsync($"1|2|tcp|{serviceHost}:{servicePort}|grpc\n");
                await Console.Out.FlushAsync();

                while (Console.Read() == -1)
                    await Task.Delay(1000);

                await server.ShutdownAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ua Plugin stopped unexpectedly");
            }
        }
    }
}
