using Grpc.Core.Logging;
using Opc.Ua;
using Pluginv2;
using Prediktor.UA.Client;
using System;
using System.Collections.Generic;
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
                fieldName = String.Join(" / ", query.nodePath.browsePath.Select(a => a.name).ToArray());
                if (relativePath?.RelativePath?.Elements?.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(fieldName);
                    var pelemt = relativePath.RelativePath.Elements;
                    for (int i = 0; i < pelemt.Count; i++)
                    {
                        sb.Append(" / ");
                        sb.Append(pelemt[i].TargetName.Name);
                    }
                    fieldName = sb.ToString();
                }
            }
            return fieldName;
        }


        internal static Result<DataResponse> CreateHistoryDataResponse(Result<HistoryData> valuesResult, OpcUAQuery query, BrowsePath relativePath)
        {
            if (valuesResult.Success)
            {
                var dataResponse = new DataResponse();
                var dataFrame = new DataFrame(query.refId);
                var timeField = dataFrame.AddField("Time", typeof(DateTime));
                Field valueField = null;
                foreach (DataValue entry in valuesResult.Value.DataValues)
                {
                    if (valueField == null && entry.Value != null)
                    {
                        var fieldName = GetFieldName(query, relativePath);
                        valueField = dataFrame.AddField(fieldName, entry.Value.GetType());
                    }

                    if (valueField != null)
                    {
                        valueField.Append(entry.Value);
                        timeField.Append(entry.SourceTimestamp);
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

        internal static Result<DataResponse> GetDataResponseForDataValue(ILogger log, DataValue dataValue, NodeId nodeId, OpcUAQuery query, BrowsePath relativePath)
        {
            try
            {
                if (Opc.Ua.StatusCode.IsGood(dataValue.StatusCode))
                {
                    DataResponse dataResponse = new DataResponse();
                    DataFrame dataFrame = new DataFrame(query.refId);

                    var timeField = dataFrame.AddField("Time", typeof(DateTime));
                    var fieldName = GetFieldName(query, relativePath);
                    Field valueField = dataFrame.AddField(fieldName, dataValue?.Value != null ? dataValue.Value.GetType() : typeof(string));
                    timeField.Append(LimitDateTime(dataValue.SourceTimestamp));
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
