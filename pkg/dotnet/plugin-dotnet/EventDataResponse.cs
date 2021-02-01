using Microsoft.Extensions.Logging;
using Opc.Ua;
using Pluginv2;
using Prediktor.UA.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace plugin_dotnet
{
    public class EventDataResponse
    {

        private static readonly Dictionary<string, Type> _typeForFieldName;
        private static readonly Dictionary<string, Func<object, object>> _converter;
        static EventDataResponse()
        {
            _typeForFieldName = new Dictionary<string, Type>();
            _typeForFieldName.Add("Time", typeof(DateTime));
            _typeForFieldName.Add("EventId", typeof(string));
            _typeForFieldName.Add("EventType", typeof(string));
            _typeForFieldName.Add("SourceName", typeof(string));
            _typeForFieldName.Add("SourceNode", typeof(string));
            _typeForFieldName.Add("Message", typeof(string));
            _typeForFieldName.Add("Severity", typeof(ushort));

            _converter = new Dictionary<string, Func<object, object>>();
            _converter.Add("EventId", o => ByteArrayToHexViaLookup32((byte[])o));
        }

        private static readonly uint[] _lookup32 = CreateLookup32();

        private static uint[] CreateLookup32()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        }

        private static string ByteArrayToHexViaLookup32(byte[] bytes)
        {
            var lookup32 = _lookup32;
            var result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                var val = lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }
            return new string(result);
        }


        public static Type GetTypeForField(QualifiedName[] browsePath)
        {
            if (browsePath == null || browsePath.Length == 0)
                throw new ArgumentException(nameof(browsePath));

            if (browsePath.Length == 1 && (string.Compare(browsePath[0].namespaceUrl, "http://opcfoundation.org/UA/") == 0))
            {
                if (_typeForFieldName.TryGetValue(browsePath[0].name, out Type type))
                    return type;
            }
            return typeof(string);
        }

        public static object GetValueForField(QualifiedName[] browsePath, object value)
        {
            if (browsePath != null && browsePath.Length == 1 && (string.Compare(browsePath[0].namespaceUrl, "http://opcfoundation.org/UA/") == 0))
            {
                var fieldName = browsePath[0].name;
                if (_converter.TryGetValue(fieldName, out Func<object, object> conv))
                    return conv(value);
            }
            return value;
        }

        internal static string GetFieldName(QualifiedName[] browsePath)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < browsePath.Length; i++)
            {
                if (sb.Length > 0)
                    sb.Append("/");
                sb.Append(browsePath[i].name);
            }
            return sb.ToString();
        }


        internal static Dictionary<int, Field> AddEventFields(DataFrame dataFrame, OpcUAQuery query)
        {
            var fields = new Dictionary<int, Field>();
            for (int i = 0; i < query.eventQuery.eventColumns.Length; i++)
            {
                var col = query.eventQuery.eventColumns[i];
                var type = GetTypeForField(col.browsePath);
                var field = dataFrame.AddField(string.IsNullOrEmpty(col.alias) ? GetFieldName(col.browsePath) : col.alias, type);
                field.Config.Filterable = true;
                fields.Add(i, field);

            }
            return fields;
        }

        internal static object GetDataFieldValue(Field dataField, object val)
        {
            if (val != null)
            {
                if (dataField.Type.Equals(val.GetType()))
                    return val;
                else
                {
                    try
                    {
                        return Convert.ChangeType(val, dataField.Type);
                    }
                    catch
                    {
                        if (dataField.Type.IsValueType)
                            return Activator.CreateInstance(dataField.Type);
                        else if (dataField.Type.Equals(typeof(string)))
                            return val.ToString();
                        else
                            return null;
                    }
                }
            }
            else
            {
                if (dataField.Type.IsValueType)
                    return Activator.CreateInstance(dataField.Type);
                else if (dataField.Type.Equals(typeof(string)))
                    return string.Empty;
                else
                    return null;
            }
        }

        internal static void FillEventDataFrame(Dictionary<int, Field> fields, VariantCollection eventFields, OpcUAQuery query)
        {
            for (int k = 0; k < eventFields.Count; k++)
            {
                var field = eventFields[k];
                if (fields.TryGetValue(k, out Field dataField))
                {
                    var path = query.eventQuery.eventColumns[k].browsePath;
                    dataField.Append(GetDataFieldValue(dataField, GetValueForField(path, field.Value)));
                }
            }
        }


        internal static Result<DataResponse> CreateEventSubscriptionDataResponse(ILogger log, ICollection<VariantCollection> events, OpcUAQuery query)
        {
            var dataResponse = new DataResponse();
            var dataFrame = new DataFrame(log, query.refId);
            var fields = AddEventFields(dataFrame, query);
            foreach (var ev in events)
            {
                FillEventDataFrame(fields, ev, query);
            }
            dataResponse.Frames.Add(dataFrame.ToGprcArrowFrame());
            return new Result<DataResponse>(dataResponse);
        }



        internal static Result<DataResponse> CreateEventDataResponse(ILogger log, Result<HistoryEvent> historyEventResult, OpcUAQuery query)
        {
            if (historyEventResult.Success)
            {
                var historyEvent = historyEventResult.Value;
                var dataResponse = new DataResponse();
                var dataFrame = new DataFrame(log, query.refId);
                if (historyEvent.Events.Count > 0)
                {
                    var fields = AddEventFields(dataFrame, query);
                    foreach (var e in historyEvent.Events)
                    {
                        FillEventDataFrame(fields, e.EventFields, query);
                    }
                }
                dataResponse.Frames.Add(dataFrame.ToGprcArrowFrame());
                return new Result<DataResponse>(dataResponse);
            }
            else
                return new Result<DataResponse>(historyEventResult.StatusCode, historyEventResult.Error);
        }
    }
}
