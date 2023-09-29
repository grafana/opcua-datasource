using System;
using System.Collections.Generic;
using System.Text;
using Pluginv2;
using Grpc.Core;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using Google.Protobuf;
using System.Globalization;
using Grpc.Core.Logging;

namespace plugin_dotnet
{
    public class DiagnosticsService : Diagnostics.DiagnosticsBase
    {
        private IConnections _connections;
        private ILogger log;
        public DiagnosticsService(ILogger logIn, IConnections connections)
        {
            log = logIn;
            _connections = connections;
        }

        public override Task<CheckHealthResponse> CheckHealth(CheckHealthRequest request, ServerCallContext context)
        {
            log.Debug("Check Health Request {0}", request);

            Settings settings = RawSettingsParser.Parse(request.PluginContext.DataSourceInstanceSettings);
            IConnection connection = _connections.Get(settings);
            log.Debug("Check Health Request Connection {0}", connection);

            CheckHealthResponse checkHealthResponse = new CheckHealthResponse
            {
                Status = connection.Session.Connected ? CheckHealthResponse.Types.HealthStatus.Ok : CheckHealthResponse.Types.HealthStatus.Error,
                Message = connection.Session.Connected ? "Connected Successfully" : "Connection Failed",
            };
            return Task.FromResult(checkHealthResponse);
        }

        public override Task<CollectMetricsResponse> CollectMetrics(CollectMetricsRequest request, ServerCallContext context)
        {
            log.Info(String.Format("Request Received: {0}", request));
            return base.CollectMetrics(request, context);
        }
    }
}
