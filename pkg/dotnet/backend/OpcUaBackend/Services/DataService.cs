using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Pluginv2;

namespace OpcUaBackend.Services
{
    class DataService : Data.DataBase
    {
        public override Task<QueryDataResponse> QueryData(QueryDataRequest request, ServerCallContext context)
        {
            return base.QueryData(request, context);
        }
    }
}
