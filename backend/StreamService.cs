using Pluginv2;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Grpc.Core;

namespace plugin_dotnet
{
    public class StreamService : Stream.StreamBase
    {
        public StreamService(ILogger logger, Connections connections)
        {

        }

        public override Task<PublishStreamResponse> PublishStream(PublishStreamRequest request, ServerCallContext context)
        {
            return base.PublishStream(request, context);
        }

        public override Task RunStream(RunStreamRequest request, IServerStreamWriter<StreamPacket> responseStream, ServerCallContext context)
        {
            return base.RunStream(request, responseStream, context);
        }

        public override Task<SubscribeStreamResponse> SubscribeStream(SubscribeStreamRequest request, ServerCallContext context)
        {
            return base.SubscribeStream(request, context);
        }

    }
}