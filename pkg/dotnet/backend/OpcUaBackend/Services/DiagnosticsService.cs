using System;
using System.Threading.Tasks;
using Grpc.Core;
using Pluginv2;

namespace OpcUaBackend.Services
{
    public class DiagnosticsService: Diagnostics.DiagnosticsBase
    {
        public DiagnosticsService()
        {
        }

        public override Task<CheckHealthResponse> CheckHealth(CheckHealthRequest request, ServerCallContext context)
        {
            return base.CheckHealth(request, context);
        }

        public override Task<CollectMetricsResponse> CollectMetrics(CollectMetricsRequest request, ServerCallContext context)
        {
            return base.CollectMetrics(request, context);
        }
    }
}
