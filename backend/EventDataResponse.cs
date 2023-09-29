using Grpc.Core.Logging;
using Opc.Ua;
using Pluginv2;
using Prediktor.UA.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace plugin_dotnet
{
    public interface IEventDataResponse
    {
        Result<DataResponse> CreateEventSubscriptionDataResponse(ICollection<VariantCollection> events, OpcUAQuery query, INodeCache nodeCache);
        Result<DataResponse> CreateEventDataResponse(Result<HistoryEvent> historyEventResult, OpcUAQuery query, INodeCache nodeCache);
    }

    public class EventDataResponse : IEventDataResponse
    {
        private ILogger _logger;
        private static readonly Dictionary<string, Type> _typeForFieldName;
        private static readonly Dictionary<string, Func<INodeCache, object, object>> _converter;
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
            _typeForFieldName.Add("EnabledState/Id", typeof(bool));
            _typeForFieldName.Add("AckedState/Id", typeof(bool));
            _typeForFieldName.Add("ConfirmedState/Id", typeof(bool));
            _typeForFieldName.Add("ActiveState/Id", typeof(bool));
            _typeForFieldName.Add("SilenceState/Id", typeof(bool));
            _typeForFieldName.Add("SuppressedState/Id", typeof(bool));

            _converter = new Dictionary<string, Func<INodeCache, object, object>>();
            _converter.Add("EventId", (nodeCache, o) => ByteArrayToHexViaLookup32((byte[])o));
            _converter.Add("EventType",(nodeCache, o) => NodeToBrowseName(nodeCache, (NodeId)o));
        }

        private static object NodeToBrowseName(INodeCache nodeCache, NodeId nodeId)
        {
            try
            {
                Opc.Ua.QualifiedName node = nodeCache.GetBrowseName(nodeId);
                if (node != null)
                {
                    return node.ToString();
                }
            }
            catch {
            }
            return nodeId.ToString();
        }

        public EventDataResponse(ILogger logger)
        {
            _logger = logger;
        }


        private static readonly uint[] _lookup32 = CreateLookup32();

        private static uint[] CreateLookup32()
        {
            uint[] result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        }

        private static string ByteArrayToHexViaLookup32(byte[] bytes)
        {
            uint[] lookup32 = _lookup32;
            char[] result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                uint val = lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }
            return new string(result);
        }


        private static Type GetTypeForField(QualifiedName[] browsePath)
        {
            if (browsePath == null || browsePath.Length == 0)
                throw new ArgumentException(nameof(browsePath));
            StringBuilder path = new StringBuilder();
            for (int i = 0; i < browsePath.Length; i++)
            {
                if (string.Compare(browsePath[i].namespaceUrl, "http://opcfoundation.org/UA/") == 0)
                {
                    if (path.Length > 0)
                        path.Append("/");
                    path.Append(browsePath[i].name);
                }
                else
                    return typeof(string);
            }
            string simplifiedBrowsePath = path.ToString();
            if (_typeForFieldName.TryGetValue(simplifiedBrowsePath, out Type type))
                return type;
            return typeof(string);
        }

        private static object GetValueForField(QualifiedName[] browsePath, INodeCache nodeCache, object value)
        {
            if (browsePath != null && browsePath.Length == 1 && (string.Compare(browsePath[0].namespaceUrl, "http://opcfoundation.org/UA/") == 0))
            {
                string fieldName = browsePath[0].name;
                if (_converter.TryGetValue(fieldName, out Func<INodeCache, object, object> conv))
                    return conv(nodeCache, value);
            }
            return value;
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

        private static string GetFieldName(QualifiedName[] browsePath)
        {
            StringBuilder sb = new StringBuilder();
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
            Dictionary<int, Field> fields = new Dictionary<int, Field>();
            for (int i = 0; i < query.eventQuery.eventColumns.Length; i++)
            {
                EventColumn col = query.eventQuery.eventColumns[i];
                Type type = GetTypeForField(col.browsePath);
                Field field = dataFrame.AddField(string.IsNullOrEmpty(col.alias) ? GetFieldName(col.browsePath) : col.alias, type);
                field.Config.Filterable = true;
                fields.Add(i, field);

            }
            return fields;
        }



        internal static void FillEventDataFrame(Dictionary<int, Field> fields, VariantCollection eventFields, OpcUAQuery query, INodeCache nodeCache)
        {
            for (int k = 0; k < eventFields.Count; k++)
            {
                Variant field = eventFields[k];
                if (fields.TryGetValue(k, out Field dataField))
                {
                    QualifiedName[] path = query.eventQuery.eventColumns[k].browsePath;
                    dataField.Append(GetDataFieldValue(dataField, GetValueForField(path, nodeCache, field.Value)));
                }
            }
        }


        public Result<DataResponse> CreateEventSubscriptionDataResponse(ICollection<VariantCollection> events, OpcUAQuery query, INodeCache nodeCache)
        {
            DataResponse dataResponse = new DataResponse();
            DataFrame dataFrame = new DataFrame(query.refId);
            Dictionary<int, Field> fields = AddEventFields(dataFrame, query);
            foreach (VariantCollection ev in events)
            {
                FillEventDataFrame(fields, ev, query, nodeCache);
            }
            dataResponse.Frames.Add(dataFrame.ToGprcArrowFrame());
            return new Result<DataResponse>(dataResponse);
        }



        public Result<DataResponse> CreateEventDataResponse(Result<HistoryEvent> historyEventResult, OpcUAQuery query, INodeCache nodeCache)
        {
            if (historyEventResult.Success)
            {
                HistoryEvent historyEvent = historyEventResult.Value;
                DataResponse dataResponse = new DataResponse();
                DataFrame dataFrame = new DataFrame(query.refId);
                Dictionary<int, Field> fields = AddEventFields(dataFrame, query);
                if (historyEvent.Events.Count > 0)
                {
                    foreach (HistoryEventFieldList e in historyEvent.Events)
                    {
                        FillEventDataFrame(fields, e.EventFields, query, nodeCache);
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
