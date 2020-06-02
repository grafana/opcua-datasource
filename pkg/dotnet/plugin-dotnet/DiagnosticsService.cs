using System;
using System.Collections.Generic;
using System.Text;
using Pluginv2;
using Grpc.Core;
using Grpc.Core.Logging;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using MicrosoftOpcUa.Client.Utility;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using Google.Protobuf;
using System.Globalization;

namespace plugin_dotnet
{
    public class DiagnosticsService : Diagnostics.DiagnosticsBase
    {
        private ILogger log;
        private OpcUaDatasource datasource;
        public DiagnosticsService(ILogger logIn)
        {
            log = logIn;
            datasource = new OpcUaDatasource(logIn);
        }

        public override Task<CheckHealthResponse> CheckHealth(CheckHealthRequest request, ServerCallContext context)
        {
            log.Debug("Check Health Request {0}", request);

            OpcUAConnection connection = Connections.Get(request.PluginContext.DataSourceInstanceSettings.Url);
            string browseResult = connection.Browse();
            CheckHealthResponse checkHealthResponse = new CheckHealthResponse
            {
                Status = CheckHealthResponse.Types.HealthStatus.Ok,
                Message = browseResult
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
