using System;
using System.Collections.Generic;
using System.Text;
using Pluginv2;
using Serilog;
using Grpc.Core;
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
    class CoreService : Core.CoreBase
    {
        private readonly Serilog.Core.Logger log;
        
        public CoreService(Serilog.Core.Logger logIn)
        {
            log = logIn;
        }

        
        public async override Task<DataQueryResponse> DataQuery(DataQueryRequest request, ServerCallContext context)
        {
            return null;
        }

    }
}
