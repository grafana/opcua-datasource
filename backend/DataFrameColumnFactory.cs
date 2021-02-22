using Apache.Arrow;
using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace plugin_dotnet
{

    internal static class DataFrameColumnFactory
    {
        internal static IDictionary<string, string> CreateMetaData(Field f)
        {
            if (f.Config != null)
            {
                var meta = new Dictionary<string, string>();
                meta.Add("config", System.Text.Json.JsonSerializer.Serialize(f.Config));
                return meta;
            }
            return null;

        }

        internal static DataFrameColumn Create(Field field)
        {
            switch (field.Type.Name)
            {
                case "double":
                case "Double":
                    return new OpcUaDataFrameColumn<double>(field.Name, field.DataAs<double?>(), CreateMetaData(field));
                case "float":
                case "Single":
                    return new OpcUaDataFrameColumn<float>(field.Name, field.DataAs<float?>(), CreateMetaData(field));
                case "ulong":
                case "UInt64":
                    return new OpcUaDataFrameColumn<ulong>(field.Name, field.DataAs<ulong?>(), CreateMetaData(field));
                case "long":
                case "Int64":
                    return new OpcUaDataFrameColumn<long>(field.Name, field.DataAs<long?>(), CreateMetaData(field));
                case "uint":
                case "UInt32":
                    return new OpcUaDataFrameColumn<uint>(field.Name, field.DataAs<uint?>(), CreateMetaData(field));
                case "int":
                case "Int32":
                    return new OpcUaDataFrameColumn<int>(field.Name, field.DataAs<int?>(), CreateMetaData(field));
                case "ushort":
                case "UInt16":
                    return new OpcUaDataFrameColumn<ushort>(field.Name, field.DataAs<ushort?>(), CreateMetaData(field));
                case "short":
                case "Int16":
                    return new OpcUaDataFrameColumn<short>(field.Name, field.DataAs<short?>(), CreateMetaData(field));
                case "byte":
                case "Byte":
                    return new OpcUaDataFrameColumn<byte>(field.Name, field.DataAs<byte?>(), CreateMetaData(field));
                case "sbyte":
                case "SByte":
                    return new OpcUaDataFrameColumn<sbyte>(field.Name, field.DataAs<sbyte?>(), CreateMetaData(field));
                case "decimal":
                case "Decimal":
                    return new OpcUaDataFrameColumn<decimal>(field.Name, field.DataAs<decimal?>(), CreateMetaData(field));
                case "bool":
                case "Boolean":
                    return new OpcUaDataFrameColumn<bool>(field.Name, field.DataAs<bool?>(), CreateMetaData(field));
                case "DateTime":
                    return new OpcUaDataFrameColumn<DateTime>(field.Name, field.DataAs<DateTime?>(), CreateMetaData(field));
                case "string":
                case "String":
                    var stringArray = CreateStringArray(field.DataAs<string>());
                    // TODO: add meta
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
