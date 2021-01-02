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
using MicrosoftOpcUa.Client.Core;

namespace plugin_dotnet
{
    class OpcUaNodeDefinition
    {
        public string name { get; set; }
        public string nodeId { get; set; }
    }

    class DataService : Data.DataBase
    {
        private readonly ILogger log;
        private Alias alias;

        public DataService(ILogger logIn)
        {
            log = logIn;
            log.Info("We are using the datasource");
            alias = new Alias();
        }

        public override async Task<QueryDataResponse> QueryData(QueryDataRequest request, ServerCallContext context)
        {
            QueryDataResponse response = new QueryDataResponse();
            OpcUAConnection connection = null;

            try
            {
                log.Debug("got a request: {0}", request);
                connection = Connections.Get(request.PluginContext.DataSourceInstanceSettings);
                
                foreach (DataQuery currRequest in request.Queries)
                {
                    try
                    {
                        DataResponse dataResponse = new DataResponse();
                        OpcUAQuery query = new OpcUAQuery(currRequest);
                        DataFrame dataFrame = new DataFrame(query.refId);

                        log.Debug("Processing a request: {0} {1} {2}", query.nodeId, query.value, query.readType);
                        log.Debug("Query: {0}", query);

                        switch (query.readType)
                        {
                            case "ReadNode":
                                {
                                    log.Debug("Reading node");
                                    DataValue value = connection.ReadNode(query.nodeId);
                                    log.Debug("Got a value {0}, {1}", value.Value, value.Value.GetType());

                                    Field timeField = dataFrame.AddField("Time", value.SourceTimestamp.GetType());
                                    Field valueField = dataFrame.AddField(String.Join(" / ", query.value), value.Value.GetType());
                                    timeField.Append(value.SourceTimestamp);
                                    valueField.Append(value.Value);
                                }
                                break;
                            case "Subscribe":
                                {
                                    connection.AddSubscription(query.refId, query.nodeId, SubscriptionCallback);
                                }
                                break;
                            case "ReadDataRaw":
                                {
                                    DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(query.timeRange.FromEpochMS).UtcDateTime;
                                    DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(query.timeRange.ToEpochMS).UtcDateTime;
                                    log.Debug("Parsed Time: {0} {1}", fromTime, toTime);

                                    IEnumerable<DataValue> readResults = connection.ReadHistoryRawDataValues(
                                        query.nodeId,
                                        fromTime,
                                        toTime,
                                        (uint)query.maxDataPoints,
                                        false);

                                    Field timeField = dataFrame.AddField<DateTime>("Time");
                                    Field valueField = null;
                                    foreach (DataValue entry in readResults)
                                    {
                                        if (valueField == null && entry.Value != null)
                                        {
                                            valueField = dataFrame.AddField(String.Join(" / ", query.value), entry.Value.GetType());
                                        }

                                        if (valueField != null)
                                        {
                                            valueField.Append(entry.Value);
                                            timeField.Append(entry.SourceTimestamp);
                                        }
                                    }
                                }
                                break;
                            case "ReadDataProcessed":
                                {
                                    DateTime fromTime = DateTimeOffset.FromUnixTimeMilliseconds(query.timeRange.FromEpochMS).UtcDateTime;
                                    DateTime toTime = DateTimeOffset.FromUnixTimeMilliseconds(query.timeRange.ToEpochMS).UtcDateTime;
                                    log.Debug("Parsed Time: {0} {1}, Aggregate {2}", fromTime, toTime, query.aggregate);
                                    OpcUaNodeDefinition aggregate = JsonSerializer.Deserialize<OpcUaNodeDefinition>(query.aggregate.ToString());

                                    log.Debug("Reading NodeID [{0}]", query.nodeId);
                                    IEnumerable<DataValue> readResults = connection.ReadHistoryProcessed(
                                        query.nodeId,
                                        fromTime,
                                        toTime,
                                        aggregate.nodeId,
                                        query.intervalMs,
                                        (uint)query.maxDataPoints,
                                        true);

                                    Field timeField = dataFrame.AddField<DateTime>("Time");
                                    Field valueField = null;
                                    foreach (DataValue entry in readResults)
                                    {
                                        if (valueField == null && entry.Value != null)
                                        {
                                            valueField = dataFrame.AddField(String.Join(" / ", query.value), entry.Value.GetType());
                                        }

                                        if (valueField != null)
                                        {
                                            valueField.Append(entry.Value);
                                            timeField.Append(entry.SourceTimestamp);
                                        }
                                    }
                                }
                                break;
                        }

                        dataResponse.Frames.Add(dataFrame.ToGprcArrowFrame());

                        // Handle the alias parsing

                        // Deliver the response
                        response.Responses[currRequest.RefId] = dataResponse;
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.ToString());
                        response.Responses[currRequest.RefId].Error = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
               // Close out the client connection.
               log.Debug("Error: {0}", ex);
               connection.Disconnect();
            }

            return await Task.FromResult(response);
        }

        private void SubscriptionCallback(string refId, MonitoredItem item, MonitoredItemNotificationEventArgs eventArgs)
        {
            QueryDataResponse response = new QueryDataResponse();
            log.Debug("Got a callback {0} - {1} - {2}", refId, item, eventArgs);
            
        }
    }
}
