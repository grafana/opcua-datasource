﻿using Apache.Arrow;
using Microsoft.Data.Analysis;
using Opc.Ua;
using Pluginv2;
using Prediktor.UA.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace plugin_dotnet
{

	public static class Converter
	{

        private static readonly DateTime _lowLimit = new DateTime(2, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime _highLimit = new DateTime(9998, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static NodeInfo ConvertToNodeInfo(Opc.Ua.Node node, NamespaceTable namespaceTable)
        {
            var nsUrl = namespaceTable.GetString(node.NodeId.NamespaceIndex);
            var nsNodeId = new NSNodeId() { id = node.NodeId.ToString(), namespaceUrl = nsUrl };
            var nid = System.Text.Json.JsonSerializer.Serialize(nsNodeId);
            return new NodeInfo() { browseName = GetQualifiedName(node.BrowseName, namespaceTable), 
                displayName = node.DisplayName?.Text, nodeClass = (uint)node.NodeClass, nodeId = nid };
        }


        public static BrowseResultsEntry ConvertToBrowseResult(ReferenceDescription referenceDescription, NamespaceTable namespaceTable)
		{
            var nsUrl = namespaceTable.GetString(referenceDescription.NodeId.NamespaceIndex);
            var nsNodeId = new NSNodeId() { id = referenceDescription.NodeId.ToString(), namespaceUrl = nsUrl };
            var nid = System.Text.Json.JsonSerializer.Serialize(nsNodeId);
            return new BrowseResultsEntry(
				referenceDescription.DisplayName.ToString(),
                GetQualifiedName(referenceDescription.BrowseName, namespaceTable),
                nid,
				referenceDescription.TypeId,
				referenceDescription.IsForward,
				Convert.ToUInt32(referenceDescription.NodeClass));
		}


        public static NodeId GetNodeId(string nid, NamespaceTable namespaceTable)
        {
            try
            {
                var nsNodeId = System.Text.Json.JsonSerializer.Deserialize<NSNodeId>(nid);
                NodeId nId = NodeId.Parse(nsNodeId.id);
                var idx = (ushort)namespaceTable.GetIndex(nsNodeId.namespaceUrl);
                return new NodeId(nId.Identifier, idx);
            }
            catch
            {
                return NodeId.Parse(nid);
            }
        }

        internal static Opc.Ua.QualifiedName GetQualifiedName(QualifiedName qm, NamespaceTable namespaceTable)
        {
            ushort nsIdx;
            if (ushort.TryParse(qm.namespaceUrl, out nsIdx))
                return new Opc.Ua.QualifiedName(qm.name, nsIdx);
            var insIdx = string.IsNullOrWhiteSpace(qm.namespaceUrl) ? 0 : namespaceTable.GetIndex(qm.namespaceUrl);
            return new Opc.Ua.QualifiedName(qm.name, (ushort)insIdx);

        }

        internal static QualifiedName GetQualifiedName(Opc.Ua.QualifiedName qm, NamespaceTable namespaceTable)
        {
            var url = namespaceTable.GetString(qm.NamespaceIndex);
            return new QualifiedName() { name = qm.Name, namespaceUrl = url };

        }

        internal static string GetFieldName(QualifiedName[] browsePath)
        {
            var sb = new StringBuilder();
            for (int i = 0; i <  browsePath.Length; i++)
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

                fields.Add(i, dataFrame.AddField<string>(string.IsNullOrEmpty(col.alias) ? GetFieldName(col.browsePath) : col.alias));
            }
            return fields;
        }

        internal static void FillEventDataFrame(Dictionary<int, Field> fields, VariantCollection eventFields)
        {
            for (int k = 0; k < eventFields.Count; k++)
            {
                var field = eventFields[k];
                if (fields.TryGetValue(k, out Field dataField))
                {

                    if (field.Value != null)
                    {
                        if (dataField.Type.Equals(field.Value.GetType()))
                            dataField.Append(field.Value);
                        else
                            dataField.Append(field.Value.ToString());
                    }
                    else
                    {
                        if (dataField.Type.IsValueType)
                            dataField.Append(Activator.CreateInstance(dataField.Type));
                        else if (dataField.Type.Equals(typeof(string)))
                            dataField.Append(string.Empty);
                        else
                            dataField.Append(null);
                    }
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
                FillEventDataFrame(fields, ev);
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
                        FillEventDataFrame(fields, e.EventFields);
                    }
                }
                dataResponse.Frames.Add(dataFrame.ToGprcArrowFrame());
                return new Result<DataResponse>(dataResponse);
            }
            else
                return new Result<DataResponse>(historyEventResult.StatusCode, historyEventResult.Error);
        }

        private static LiteralOperand GetLiteralOperand(LiteralOp literop, NamespaceTable namespaceTable)
        {
            var nodeId = Converter.GetNodeId(literop.typeId, namespaceTable);
            if (nodeId.NamespaceIndex == 0 && nodeId.IdType == IdType.Numeric)
            {
                var id = Convert.ToInt32(nodeId.Identifier);
                if (id == 17)  // NodeId: TODO use constant.
                {
                    var nodeIdVal = Converter.GetNodeId(literop.value, namespaceTable);
                    return new LiteralOperand(nodeIdVal);
                }
            }
            return new LiteralOperand(literop.value);
        }

        private static SimpleAttributeOperand GetSimpleAttributeOperand(SimpleAttributeOp literop, NamespaceTable namespaceTable)
        {
            NodeId typeId = null;
            if (!string.IsNullOrWhiteSpace(literop.typeId))
            {
                typeId = Converter.GetNodeId(literop.typeId, namespaceTable);
            }
            return new SimpleAttributeOperand(typeId, literop.browsePath.Select(a => Converter.GetQualifiedName(a, namespaceTable)).ToList());
        }


        private static object GetOperand(FilterOperand operand, NamespaceTable namespaceTable)
        {
            
            switch (operand.type)
            {
                case FilterOperandEnum.Literal:
                    return GetLiteralOperand(JsonSerializer.Deserialize<LiteralOp>(operand.value), namespaceTable);
                case FilterOperandEnum.Element:
                    {
                        var elementOp = JsonSerializer.Deserialize<ElementOp>(operand.value);
                        return new ElementOperand(elementOp.index);
                    }
                case FilterOperandEnum.SimpleAttribute:
                    return GetSimpleAttributeOperand(JsonSerializer.Deserialize<SimpleAttributeOp>(operand.value), namespaceTable);
                default:
                    throw new ArgumentException();
            }
        }

        internal static object[] GetOperands(EventFilter f, NamespaceTable namespaceTable)
        {
            var operands = new object[f.operands.Length];
            for (int i = 0; i < f.operands.Length; i++)
                operands[i] = GetOperand(f.operands[i], namespaceTable);
            return operands;

        }

        private static Opc.Ua.QualifiedName[] GetBrowsePath(QualifiedName[] browsePath, NamespaceTable namespaceTable)
        {
            var qms = new Opc.Ua.QualifiedName[browsePath.Length];
            for (int i = 0; i < browsePath.Length; i++)
            {
                var bp = browsePath[i];
                var nsIdx = string.IsNullOrWhiteSpace(bp.namespaceUrl) ? 0 : namespaceTable.GetIndex(bp.namespaceUrl);
                qms[i] = new Opc.Ua.QualifiedName(bp.name, (ushort)nsIdx); ;
            }
            return qms;
        }

        internal static Opc.Ua.EventFilter GetEventFilter(OpcUAQuery query, NamespaceTable namespaceTable)
        {
            var eventFilter = new Opc.Ua.EventFilter();
            if (query.eventQuery?.eventColumns != null)
            {
                foreach (var column in query.eventQuery.eventColumns)
                {
                    var bp = GetBrowsePath(column.browsePath, namespaceTable);
                    var path = SimpleAttributeOperand.Format(bp);
                    eventFilter.AddSelectClause(ObjectTypes.BaseEventType, path, Attributes.Value);
                }
            }


            if (query.eventQuery?.eventFilters != null)
            {
                for (int i = 0; i < query.eventQuery.eventFilters.Length; i++)
                {
                    var filter = query.eventQuery.eventFilters[i];
                    eventFilter.WhereClause.Push(filter.oper, GetOperands(filter, namespaceTable));
                }
            }
            return eventFilter;
        }

        internal static Result<DataResponse> CreateHistoryDataResponse(ILogger log, Result<HistoryData> valuesResult, OpcUAQuery query)
        {
            if (valuesResult.Success)
            {
                var dataResponse = new DataResponse();
                var dataFrame = new DataFrame(log, query.refId);
                var timeField = dataFrame.AddField("Time", typeof(DateTime));
                Field valueField = null;
                foreach (DataValue entry in valuesResult.Value.DataValues)
                {
                    if (valueField == null && entry.Value != null)
                    {
                        string fieldName = string.IsNullOrEmpty(query.alias) ? String.Join(" / ", query.value) : query.alias;
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

        internal static Result<DataResponse> GetDataResponseForDataValue(ILogger log, DataValue dataValue, NodeId nodeId, OpcUAQuery query)
        {
            if (Opc.Ua.StatusCode.IsGood(dataValue.StatusCode))
            {
                DataResponse dataResponse = new DataResponse();
                DataFrame dataFrame = new DataFrame(log, query.refId);

                var timeField = dataFrame.AddField("Time", typeof(DateTime));
                string fieldName = string.IsNullOrEmpty(query.alias) ? String.Join(" / ", query.value) : query.alias;
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
    }

	internal static class DataFrameColumnFactory
	{
        internal static DataFrameColumn Create(Field field)
		{
            switch (field.Type.Name)
            {
                case "double":
                case "Double":
                    return new OpcUaDataFrameColumn<double>(field.Name, field.DataAs<double>());
                case "float":
                case "Single":
                    return new OpcUaDataFrameColumn<float>(field.Name, field.DataAs<float>());
                case "ulong":
                case "UInt64":
                    return new OpcUaDataFrameColumn<ulong>(field.Name, field.DataAs<ulong>());
                case "long":
                case "Int64":
                    return new OpcUaDataFrameColumn<long>(field.Name, field.DataAs<long>());
                case "uint":
                case "UInt32":
                    return new OpcUaDataFrameColumn<uint>(field.Name, field.DataAs<uint>());
                case "int":
                case "Int32":
                    return new OpcUaDataFrameColumn<int>(field.Name, field.DataAs<int>());
                case "ushort":
                case "UInt16":
                    return new OpcUaDataFrameColumn<ushort>(field.Name, field.DataAs<ushort>());
                case "short":
                case "Int16":
                    return new OpcUaDataFrameColumn<short>(field.Name, field.DataAs<short>());
                case "byte":
                case "Byte":
                    return new OpcUaDataFrameColumn<byte>(field.Name, field.DataAs<byte>());
                case "sbyte":
                case "SByte":
                    return new OpcUaDataFrameColumn<sbyte>(field.Name, field.DataAs<sbyte>());
                case "decimal":
                case "Decimal":
                    return new OpcUaDataFrameColumn<decimal>(field.Name, field.DataAs<decimal>());
                case "bool":
                case "Bool":
                    return new OpcUaDataFrameColumn<bool>(field.Name, field.DataAs<bool>());
                case "DateTime":
                    return new OpcUaDataFrameColumn<DateTime>(field.Name, field.DataAs<DateTime>());
                case "string":
                case "String":
                    var stringArray = CreateStringArray(field.DataAs<string>());
                    return new ArrowStringDataFrameColumn(field.Name, stringArray.ValueBuffer.Memory, stringArray.ValueOffsetsBuffer.Memory, stringArray.NullBitmapBuffer.Memory, stringArray.Length, stringArray.NullCount);
                default:
                    throw new Exception(String.Format("Could not match type [{0}]", field.Type.Name));
            }
        }

        private static StringArray CreateStringArray(IList<string> values)
        {
            var builder = new StringArray.Builder();
            builder.AppendRange(values, Encoding.UTF8);
            return builder.Build();
        }
    }
}
