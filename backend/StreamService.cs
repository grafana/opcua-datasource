using Pluginv2;
using Grpc.Core.Logging;
using System.Threading.Tasks;
using Grpc.Core;

namespace plugin_dotnet
{
    public class StreamService : Stream.StreamBase
    {
        private Grpc.Core.Logging.ILogger log = new Grpc.Core.Logging.ConsoleLogger();
        private Connections connections;
        public StreamService(ILogger logger, Connections connections)
        {
            this.connections = connections;
        }

        public override Task<PublishStreamResponse> PublishStream(PublishStreamRequest request, ServerCallContext context)
        {
            log.Debug(string.Format("We got a Publish stream request [{0}] Context [{1}]", request, context));
            return base.PublishStream(request, context);
        }

        public override Task RunStream(RunStreamRequest request, IServerStreamWriter<StreamPacket> responseStream, ServerCallContext context)
        {
            log.Debug(string.Format("We got a Run Stream stream request [{0}] ResonseStream [{1}] Context [{2}]", request, responseStream, context));
            return base.RunStream(request, responseStream, context);
        }

        public override Task<SubscribeStreamResponse> SubscribeStream(SubscribeStreamRequest request, ServerCallContext context)
        {
            log.Debug(string.Format("We got a Subscribe stream request [{0}] Context [{1}]", request, context));
            return base.SubscribeStream(request, context);
        }

    }
}