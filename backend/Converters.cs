using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

namespace plugin_dotnet
{

    public static class Converter
	{
        private static IDictionary<NodeId, Func<string, NamespaceTable, object>> _dataTypeConverter = new Dictionary<NodeId, Func<string, NamespaceTable, object>>();
        static Converter()
        {
            _dataTypeConverter.Add(DataTypeIds.NodeId, (val, nsTable) => GetNodeId(val, nsTable));
            _dataTypeConverter.Add(DataTypeIds.Byte, (val, nsTable) => byte.Parse(val));
            _dataTypeConverter.Add(DataTypeIds.UInt16, (val, nsTable) => ushort.Parse(val));
            _dataTypeConverter.Add(DataTypeIds.UInt32, (val, nsTable) => uint.Parse(val));
            _dataTypeConverter.Add(DataTypeIds.UInt64, (val, nsTable) => ulong.Parse(val));
            _dataTypeConverter.Add(DataTypeIds.SByte, (val, nsTable) => sbyte.Parse(val));
            _dataTypeConverter.Add(DataTypeIds.Int16, (val, nsTable) => short.Parse(val));
            _dataTypeConverter.Add(DataTypeIds.Int32, (val, nsTable) => int.Parse(val));
            _dataTypeConverter.Add(DataTypeIds.Int64, (val, nsTable) => long.Parse(val));
            _dataTypeConverter.Add(DataTypeIds.Integer, (val, nsTable) => long.Parse(val));
            _dataTypeConverter.Add(DataTypeIds.Float, (val, nsTable) => float.Parse(val));
            _dataTypeConverter.Add(DataTypeIds.Double, (val, nsTable) => double.Parse(val));
            _dataTypeConverter.Add(DataTypeIds.Duration, (val, nsTable) => double.Parse(val));
            _dataTypeConverter.Add(DataTypeIds.Decimal, (val, nsTable) => decimal.Parse(val));
            _dataTypeConverter.Add(DataTypeIds.DateTime, (val, nsTable) => DateTime.Parse(val));
            _dataTypeConverter.Add(DataTypeIds.Boolean, (val, nsTable) => bool.Parse(val));
            _dataTypeConverter.Add(DataTypeIds.Guid, (val, nsTable) => Guid.Parse(val));
        }


        public static NodeInfo ConvertToNodeInfo(Opc.Ua.Node node, NamespaceTable namespaceTable)
        {
            string nsUrl = namespaceTable.GetString(node.NodeId.NamespaceIndex);
            NSNodeId nsNodeId = new NSNodeId() { id = node.NodeId.ToString(), namespaceUrl = nsUrl };
            string nid = System.Text.Json.JsonSerializer.Serialize(nsNodeId);
            return new NodeInfo() { browseName = GetQualifiedName(node.BrowseName, namespaceTable), 
                displayName = node.DisplayName?.Text, nodeClass = (uint)node.NodeClass, nodeId = nid };
        }


        public static BrowseResultsEntry ConvertToBrowseResult(ReferenceDescription referenceDescription, NamespaceTable namespaceTable)
		{
            string nsUrl = namespaceTable.GetString(referenceDescription.NodeId.NamespaceIndex);
            NSNodeId nsNodeId = new NSNodeId() { id = referenceDescription.NodeId.ToString(), namespaceUrl = nsUrl };
            string nid = System.Text.Json.JsonSerializer.Serialize(nsNodeId);
            return new BrowseResultsEntry(
				referenceDescription.DisplayName.ToString(),
                GetQualifiedName(referenceDescription.BrowseName, namespaceTable),
                nid,
				referenceDescription.TypeId,
				referenceDescription.IsForward,
				Convert.ToUInt32(referenceDescription.NodeClass));
		}

        public static string GetNodeIdAsJson(Opc.Ua.NodeId nodeId, NamespaceTable namespaceTable)
        {
            string nsUrl = namespaceTable.GetString(nodeId.NamespaceIndex);
            NSNodeId nsNodeId = new NSNodeId() { id = nodeId.ToString(), namespaceUrl = nsUrl };
            string nid = System.Text.Json.JsonSerializer.Serialize(nsNodeId);
            return nid;
        }


        public static NodeId GetNodeId(string nid, NamespaceTable namespaceTable)
        {
            NSNodeId nsNodeId;
            NodeId nId;
            try
            {
                nsNodeId = System.Text.Json.JsonSerializer.Deserialize<NSNodeId>(nid);
                nId = NodeId.Parse(nsNodeId.id);
            }
            catch
            {
                return NodeId.Parse(nid);
            }

            ushort idx = (ushort)namespaceTable.GetIndex(nsNodeId.namespaceUrl);
            if(idx < ushort.MaxValue)
                return new NodeId(nId.Identifier, idx);

            throw new ArgumentException($"Namespace '{nsNodeId.namespaceUrl}' not found");
        }

        internal static Opc.Ua.QualifiedName GetQualifiedName(QualifiedName qm, NamespaceTable namespaceTable)
        {
            ushort nsIdx;
            if (ushort.TryParse(qm.namespaceUrl, out nsIdx))
                return new Opc.Ua.QualifiedName(qm.name, nsIdx);
            int insIdx = string.IsNullOrWhiteSpace(qm.namespaceUrl) ? 0 : namespaceTable.GetIndex(qm.namespaceUrl);
            return new Opc.Ua.QualifiedName(qm.name, (ushort)insIdx);

        }

        internal static QualifiedName GetQualifiedName(Opc.Ua.QualifiedName qm, NamespaceTable namespaceTable)
        {
            string url = namespaceTable.GetString(qm.NamespaceIndex);
            return new QualifiedName() { name = qm.Name, namespaceUrl = url };

        }


        public static Opc.Ua.QualifiedName[] GetBrowsePath(QualifiedName[] browsePath, NamespaceTable namespaceTable)
        {
            Opc.Ua.QualifiedName[] qms = new Opc.Ua.QualifiedName[browsePath.Length];
            for (int i = 0; i < browsePath.Length; i++)
            {
                QualifiedName bp = browsePath[i];
                int nsIdx = string.IsNullOrWhiteSpace(bp.namespaceUrl) ? 0 : namespaceTable.GetIndex(bp.namespaceUrl);
                qms[i] = new Opc.Ua.QualifiedName(bp.name, (ushort)nsIdx); ;
            }
            return qms;
        }

        private static LiteralOperand GetLiteralOperand(LiteralOp literop, NamespaceTable namespaceTable)
        {
            NodeId nodeId = Converter.GetNodeId(literop.typeId, namespaceTable);
            if (_dataTypeConverter.TryGetValue(nodeId, out Func<string, NamespaceTable, object> converter))
            {
                object val = converter(literop.value, namespaceTable);
                return new LiteralOperand(val);
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
                        ElementOp elementOp = JsonSerializer.Deserialize<ElementOp>(operand.value);
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
            object[] operands = new object[f.operands.Length];
            for (int i = 0; i < f.operands.Length; i++)
                operands[i] = GetOperand(f.operands[i], namespaceTable);
            return operands;

        }

        internal static Opc.Ua.EventFilter GetEventFilter(OpcUAQuery query, NamespaceTable namespaceTable)
        {
            Opc.Ua.EventFilter eventFilter = new Opc.Ua.EventFilter();
            if (query.eventQuery?.eventColumns != null)
            {
                foreach (EventColumn column in query.eventQuery.eventColumns)
                {
                    Opc.Ua.QualifiedName[] bp = Converter.GetBrowsePath(column.browsePath, namespaceTable);
                    string path = SimpleAttributeOperand.Format(bp);
                    eventFilter.AddSelectClause(ObjectTypes.BaseEventType, path, Attributes.Value);
                }
            }


            if (query.eventQuery?.eventFilters != null)
            {
                for (int i = 0; i < query.eventQuery.eventFilters.Length; i++)
                {
                    EventFilter filter = query.eventQuery.eventFilters[i];
                    eventFilter.WhereClause.Push(filter.oper, GetOperands(filter, namespaceTable));
                }
            }
            return eventFilter;
        }




    }
}
