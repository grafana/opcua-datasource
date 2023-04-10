using Grpc.Core.Logging;
using Opc.Ua;
using Opcv1;
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
                    var pelemt = relativePath.RelativePath.Elements;
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

        internal static Opcv1.DataValue ParseOpcUaDataValue(Opc.Ua.DataValue entry) {
            Opcv1.DataValue responseEntry = new Opcv1.DataValue();
            responseEntry.ServerMillisecondEpoch = (uint)new DateTimeOffset(entry.ServerTimestamp).ToUnixTimeMilliseconds();
            responseEntry.SourceMillisecondEpoch =  (uint)new DateTimeOffset(entry.SourceTimestamp).ToUnixTimeMilliseconds();
            responseEntry.StatusCode = entry.StatusCode.Code;
            switch (Type.GetTypeCode(responseEntry.GetType())) {
                case TypeCode.Boolean:
                    responseEntry.Type = Opcv1.DataValue.Types.ValueType.Bool;
                    break;
                case TypeCode.Byte:
                    responseEntry.Type = Opcv1.DataValue.Types.ValueType.Byte;
                    break;
                case TypeCode.Int16:
                    responseEntry.Type = Opcv1.DataValue.Types.ValueType.Int16;
                    break;
                case TypeCode.UInt16:
                    responseEntry.Type = Opcv1.DataValue.Types.ValueType.Uint16;
                    break;
                case TypeCode.Int32:
                    responseEntry.Type = Opcv1.DataValue.Types.ValueType.Int32;
                    break;
                case TypeCode.UInt32:
                    responseEntry.Type = Opcv1.DataValue.Types.ValueType.Uint32;
                    break;
                case TypeCode.Int64:
                    responseEntry.Type = Opcv1.DataValue.Types.ValueType.Int64;
                    break;
                case TypeCode.UInt64:
                    responseEntry.Type = Opcv1.DataValue.Types.ValueType.Uint64;
                    break;
                case TypeCode.Single:
                    responseEntry.Type = Opcv1.DataValue.Types.ValueType.Float;
                    break;
                case TypeCode.Double:
                    responseEntry.Type = Opcv1.DataValue.Types.ValueType.Double;
                    break;
                case TypeCode.String:
                    responseEntry.Type = Opcv1.DataValue.Types.ValueType.String;
                    break;
            }
            responseEntry.Value = Google.Protobuf.ByteString.CopyFrom(entry.GetValue<byte[]>(null));

            return responseEntry;
        }


        internal static Result<DataResponse> CreateHistoryDataResponse(Result<HistoryData> valuesResult, OpcUAQuery query, BrowsePath relativePath, Settings settings)
        {
            if (valuesResult.Success)
            {
                var dataResponse = new DataResponse();

                foreach (Opc.Ua.DataValue entry in valuesResult.Value.DataValues)
                {
                    Opcv1.DataValue responseEntry = ParseOpcUaDataValue(entry);
                    dataResponse.DataValues.Add(responseEntry);
                }
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

        internal static Result<DataResponse> GetDataResponseForDataValue(ILogger log, Settings settings, Opc.Ua.DataValue dataValue, NodeId nodeId, OpcUAQuery query, BrowsePath relativePath)
        {
            try
            {
                if (Opc.Ua.StatusCode.IsGood(dataValue.StatusCode))
                {
                    DataResponse dataResponse = new DataResponse();
                    Opcv1.DataValue responseEntry = ParseOpcUaDataValue(dataValue);
                    dataResponse.DataValues.Add(responseEntry);
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
