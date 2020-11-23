﻿using System;
using Pluginv2;
using Grpc.Core;
using Grpc.Core.Logging;
using System.Threading.Tasks;

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

            var connection = _connections.Get(request.PluginContext.DataSourceInstanceSettings);
            CheckHealthResponse checkHealthResponse = new CheckHealthResponse
            {
                Status = connection.Connected ? CheckHealthResponse.Types.HealthStatus.Ok : CheckHealthResponse.Types.HealthStatus.Error,
                Message = connection.Connected ? "Connected Successfully" : "Connection Failed",
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
