using Grpc.Core.Logging;
using Opc.Ua;
using Pluginv2;
using Prediktor.UA.Client;
using System;
using System.Linq;
using System.Text;

namespace plugin_dotnet
{
    class ValueDataResponse
    {
        private static readonly DateTime _lowLimit = new DateTime(2, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime _highLimit = new DateTime(9998, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal static string GetFieldName(OpcUAQuery query, BrowsePath relativePath)
        {
            string fieldName = query.alias;
            if (string.IsNullOrEmpty(fieldName))
            {
                fieldName = string.Join(" / ", query.nodePath.browsePath.Select(a => a.name).ToArray());
                if (relativePath?.RelativePath?.Elements?.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(fieldName);
                    RelativePathElementCollection pelemt = relativePath.RelativePath.Elements;
                    for (int i = 0; i < pelemt.Count; i++)
                    {
                        _ = sb.Append(" / ");
                        sb.Append(pelemt[i].TargetName.Name);
                    }
                    fieldName = sb.ToString();
                }
            }
            return fieldName;
        }


        internal static Result<DataResponse> CreateHistoryDataResponse(Result<HistoryData> valuesResult, OpcUAQuery query, BrowsePath relativePath, Settings settings)
        {
            if (valuesResult.Success)
            {
                DataResponse dataResponse = new DataResponse();
                DataFrame dataFrame = new DataFrame(query.refId);
                Field timeField = dataFrame.AddField("Time", typeof(DateTime));
                Field valueField = null;
                foreach (DataValue entry in valuesResult.Value.DataValues)
                {
                    if (valueField == null && entry.Value != null)
                    {
                        string fieldName = GetFieldName(query, relativePath);
                        valueField = dataFrame.AddField(fieldName, entry.Value.GetType());
                    }

                    if (valueField != null)
                    {
                        valueField.Append(entry.Value);
                        switch (settings.TimestampSource)
                        {
                            case OPCTimestamp.Server:
                                timeField.Append(LimitDateTime(entry.ServerTimestamp));
                                break;
                            case OPCTimestamp.Source:
                                timeField.Append(LimitDateTime(entry.SourceTimestamp));
                                break;
                            default:
                                timeField.Append(LimitDateTime(entry.ServerTimestamp));
                                break;
                        }
                    }
                }
                dataResponse.Frames.Add(dataFrame.ToGprcArrowFrame());
                return new Result<DataResponse>(dataResponse);
            }
            else
            {
                return new Result<DataResponse>(valuesResult.StatusCode, valuesResult.Error);
            }
        }

        private static DateTime LimitDateTime(DateTime dt)
        {
            if (dt.CompareTo(_lowLimit) < 0)
                return _lowLimit;
            if (dt.CompareTo(_highLimit) > 0)
                return _highLimit;
            return dt;
        }

        internal static Result<DataResponse> GetDataResponseForDataValue(ILogger log, Settings settings, DataValue dataValue, NodeId nodeId, OpcUAQuery query, BrowsePath relativePath)
        {
            try
            {
                if (Opc.Ua.StatusCode.IsGood(dataValue.StatusCode))
                {
                    DataResponse dataResponse = new DataResponse();
                    DataFrame dataFrame = new DataFrame(query.refId);

                    Field timeField = dataFrame.AddField("Time", typeof(DateTime));
                    string fieldName = GetFieldName(query, relativePath);
                    Field valueField = dataFrame.AddField(fieldName, dataValue?.Value != null ? dataValue.Value.GetType() : typeof(string));
                    switch (settings.TimestampSource) {
                        case OPCTimestamp.Server:
                            timeField.Append(LimitDateTime(dataValue.ServerTimestamp));
                            break;
                        case OPCTimestamp.Source:
                            timeField.Append(LimitDateTime(dataValue.SourceTimestamp));
                            break;
                        default:
                            timeField.Append(LimitDateTime(dataValue.ServerTimestamp));
                            break;
                    }
                    valueField.Append(dataValue?.Value != null ? dataValue?.Value : "");
                    dataResponse.Frames.Add(dataFrame.ToGprcArrowFrame());
                    return new Result<DataResponse>(dataResponse);
                }
                else
                {
                    return new Result<DataResponse>(dataValue.StatusCode, string.Format("Error reading node with id {0}", nodeId.ToString()));
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return new Result<DataResponse>(dataValue.StatusCode, string.Format("Error reading node with id {0}: {1}", nodeId.ToString(), e.Message));
            }

        }
    }
}
