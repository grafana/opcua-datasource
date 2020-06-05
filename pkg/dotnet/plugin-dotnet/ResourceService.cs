using System;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Logging;
using Pluginv2;

namespace plugin_dotnet
{
    public class ResourceService : Resource.ResourceBase
    {
        private ILogger log;
        public ResourceService()
        {
            log = new ConsoleLogger();
        }

        public override Task CallResource(CallResourceRequest request, IServerStreamWriter<CallResourceResponse> responseStream, ServerCallContext context)
        {
            log.Debug("Call Resource {0} | {1}", request, context);
            CallResourceResponse response = new CallResourceResponse();
            OpcUAConnection connection = Connections.Get(request.PluginContext.DataSourceInstanceSettings.Url);
            
            try
            {
                string result = connection.Browse(request.Path);
                response.Code = 200;
                response.Body = ByteString.CopyFrom(result, Encoding.ASCII);
                log.Debug("We got a result from browse => {0}", result);
            }
            catch(Exception ex)
            {
                log.Debug("Got browse exception {0}", ex);
                throw ex;
            }
             
            responseStream.WriteAsync(response);
            return Task.CompletedTask;
        }
    }
}
