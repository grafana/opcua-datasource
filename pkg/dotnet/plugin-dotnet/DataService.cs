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

                                // DataFrame dataFrame = new DataFrame(query.nodeId);
                                // Field timeField = dataFrame.AddField("Time");
                                // Field valueField = dataFrame.AddField("Value");
                                // timeField.Append(DateTime.Now.ToUniversalTime().Subtract(
                                //     new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                // ).TotalMilliseconds);
                                // valueField.Append(value.GetValue<double>(0));

                                PrimitiveDataFrameColumn<long> timeColumn = new PrimitiveDataFrameColumn<long>("Time");
                                PrimitiveDataFrameColumn<double> valueColumn = new PrimitiveDataFrameColumn<double>("Value");
                                DataFrame dataFrame = new DataFrame(timeColumn, valueColumn);
                                timeColumn.Append(Convert.ToInt64(DateTime.Now.ToUniversalTime().Subtract(
                                   new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                ).TotalMilliseconds));
                                valueColumn.Append(value.GetValue<double>(0));
                                log.Debug("IIIII");
                                MemoryStream stream = new MemoryStream();

                                foreach (RecordBatch recordBatch in dataFrame.ToArrowRecordBatches())
                                {
                                   ArrowStreamWriter writer = new ArrowStreamWriter(stream, recordBatch.Schema);
                                   writer.WriteRecordBatchAsync(recordBatch).GetAwaiter().GetResult();

                                   stream.Position = 0;

                                   dataResponse.Frames.Add(ByteString.FromStream(stream));
                                }

                                //dataResponse.Frames.Add(await dataFrame.ToArrow());

                                log.Debug("Data Response {0}", dataResponse);
                                response.Responses[currRequest.RefId] = dataResponse;
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
    }
}
