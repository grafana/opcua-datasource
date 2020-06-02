using System;
using System.Collections.Generic;
using System.Text;
using Pluginv2;
using Grpc.Core;
using Grpc.Core.Logging;
using Google.Protobuf;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using System.Globalization;
using Apache.Arrow;
using Apache.Arrow.Ipc;
using Apache.Arrow.Memory;
using System.Linq;
using Microsoft.Data.Analysis;
using System.IO;

namespace plugin_dotnet
{
    class DataService : Data.DataBase
    {
        private readonly ILogger log;

        public DataService(ILogger logIn)
        {
            log = logIn;
            log.Info("We are using the datasource");
        }

        public override async Task<QueryDataResponse> QueryData(QueryDataRequest request, ServerCallContext context)
        {
            QueryDataResponse response = new QueryDataResponse();

            try
            {
               log.Debug("got a request: {0}", request);
               OpcUAConnection connection = Connections.Get(request.PluginContext.DataSourceInstanceSettings.Url);

               foreach (DataQuery currRequest in request.Queries)
               {
                    DataResponse dataResponse = new DataResponse();
                    OpcUAQuery query = new OpcUAQuery(currRequest);
                    
                    string jsonMeta = "";

                    log.Debug("Processing a request: {0} {1} {2}", query.nodeId, query.value, query.readType);
                    log.Debug("Query: {0}", query);

                    switch (query.readType)
                    {
                        case "ReadNode":
                            {
                                log.Debug("Reading node");
                                DataValue value = connection.ReadNode(query.nodeId);
                                log.Debug("Got a value {0}, {1}", value.GetValue<double>(0), value.StatusCode);

                                List<double> valueColumnData = new List<double>();
                                valueColumnData.Add(value.GetValue<double>(0));

                                DoubleDataFrameColumn valueColumn = new DoubleDataFrameColumn("Value", valueColumnData);
                                DataFrame dataFrame = new DataFrame(valueColumn);

                                dataResponse.Frames.Add(dataFrame.ToGprcArrowFrame());
                                log.Debug("Data Response {0}", dataResponse);
                                response.Responses[currRequest.RefId] = dataResponse;
                            }
                            break;
                        case "Subscribe":
                            {
                                connection.AddSubscription(query.refId, query.nodeId, SubscriptionCallback);
                            }
                            break; 
                        case "ReadDataProcessed":
                            {
                                DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(query.timeRange.FromEpochMS).UtcDateTime;
                                DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(query.timeRange.ToEpochMS).UtcDateTime;
                                log.Debug("Parsed Time: {0} {1}", fromTime, toTime);

                                var readResults = connection.ReadHistoryRawDataValues(
                                    query.nodeId,
                                    fromTime,
                                    toTime,
                                    (uint)query.maxDataPoints,
                                    false);

                                jsonMeta = JsonSerializer.Serialize<IEnumerable<DataValue>>(readResults);
                            }
                            break;
                    }
               }
            }
            catch (Exception ex)
            {
               // Close out the client connection.
               log.Debug("Error: {0}", ex);
               //clientConnections[request.Datasource.Url].Disconnect();
               //clientConnections.Remove(request.Datasource.Url);
               //QueryResult queryResult = new QueryResult();
               //queryResult.Error = ex.ToString();
               //response.Results.Add(queryResult);
            }

            return await Task.FromResult(response);
        }

        private void SubscriptionCallback(string arg1, MonitoredItem arg2, MonitoredItemNotificationEventArgs arg3)
        {
            log.Debug("Got a callback {0} - {1} - {2}", arg1, arg2, arg3);
        }
    }
}
