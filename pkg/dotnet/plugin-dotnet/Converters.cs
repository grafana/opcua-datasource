using Apache.Arrow;
using Microsoft.Data.Analysis;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Text;

namespace plugin_dotnet
{
	public static class Converter
	{
		public static BrowseResultsEntry ConvertToBrowseResult(ReferenceDescription referenceDescription)
		{
			return new BrowseResultsEntry(
				referenceDescription.DisplayName.ToString(),
				referenceDescription.BrowseName.ToString(),
				referenceDescription.NodeId.ToString(),
				referenceDescription.TypeId,
				referenceDescription.IsForward,
				Convert.ToUInt32(referenceDescription.NodeClass));
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
