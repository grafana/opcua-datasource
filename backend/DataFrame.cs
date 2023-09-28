using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Apache.Arrow;
using Apache.Arrow.Ipc;
using Google.Protobuf;
using Apache.Arrow.Types;
using Grpc.Core.Logging;
using System.ComponentModel;

namespace plugin_dotnet
{
    [Serializable]
    class ValueMapping
    {
        public Int16 ID { get; set; }
        public string Operator { get; set; }
        public string Text { get; set; }
        public sbyte MappingType { get; set; }

        // Only valid for MappingType == ValueMap
        public string Value { get; set; }

        // Only valid for MappingType == RangeMap
        public string From { get; set; }
        public string To { get; set; }

        public ValueMapping() { }
    }

    [Serializable]
    class Threshold
    {
        public double Value { get; set; }
        public string Color { get; set; }
        public string State { get; set; }

        public Threshold() { }
    }

    [Serializable]
    class ThresholdsConfig
    {
        public string Mode { get; set; }

        // Must be sorted by 'value', first value is always -Infinity
        public Threshold[] Steps { get; set; }
    }

    [Serializable]
    class DataLink
    {
        public string Title { get; set; }
        public bool TargetBlank { get; set; }
        public string URL { get; set; }

        public DataLink() { }
    }

    [Serializable]
    class FieldConfig
    {
        public string DisplayName { get; set; }
        public bool Filterable { get; set; }

        // Numeric Options
        public string Unit { get; set; }
        public UInt16 Decimals { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }

        // Convert input values into a display string
        public ValueMapping[] Mappings { get; set; }

        // Map numeric values to states
        public ThresholdsConfig Thresholds { get; set; }

        // Map values to a display color
        // NOTE: this interface is under development in the frontend... so simple map for now
        public Dictionary<string, object> Color { get; set; }

        // Used when reducing field values
        public string NullValueMode { get; set; }

        // The behavior when clicking on a result
        public DataLink[] Links { get; set; }

        // Alternative to empty string
        public string NoValue { get; set; }

        // Panel Specific Values
        public Dictionary<string, object> Custom { get; set; }

        public FieldConfig() { }
    }

    [Serializable]
    class Notice
    {
        // Severity is the severity level of the notice: info, warning, or error.
        public int Severity { get; set; }

        // Text is freeform descriptive text for the notice.
        public string Text { get; set; }

        // Link is an optional link for display in the user interface and can be an
        // absolute URL or a path relative to Grafana's root url.
        public string Link { get; set; }

        // Inspect is an optional suggestion for which tab to display in the panel inspector
        // in Grafana's User interface. Can be meta, error, data, or stats.
        public int Inspect { get; set; }

        public Notice() { }
    }

    [Serializable]
    class FrameMeta
    {
        // Datasource specific values
        public Dictionary<string, object> Custom { get; set; }

        // Stats is TODO
        public object Stats { get; set; }

        // Notices provide additional information about the data in the Frame that
        // Grafana can display to the user in the user interface.
        public Notice[] Notices { get; set; }

        public FrameMeta() { }
    }

    [Serializable]
    class Field
    {
        private ILogger _log;
        public string Name { get; set; }
        public Dictionary<string, string> Labels { get; set; }

        public FieldConfig Config { get; set; }
        public Type Type { get;  }
        public List<object> Data { get; set; }

        private bool _allowNull;

        public Field(string name, Type type, bool allowNull = true)
        {
            _log = new ConsoleLogger();
            Name = name;
            Type = type;
            _allowNull = allowNull;
            Data = new List<object>();
            Config = new FieldConfig();
        }

        public List<T> DataAs<T>()
        {
            return Data.Cast<T>().ToList();
        }

        public IArrowArray ToArrowArray() {
            if (Type == typeof(bool)) {
                Apache.Arrow.BooleanArray.Builder builder = new Apache.Arrow.BooleanArray.Builder();
                builder.AppendRange(DataAs<bool>());
                return builder.Build();
            }
            else if (Type == typeof(double)) {
                Apache.Arrow.DoubleArray.Builder builder = new Apache.Arrow.DoubleArray.Builder();
                builder.AppendRange(DataAs<double>());
                return builder.Build();
            }
                
            else if (Type == typeof(float)) {
                Apache.Arrow.FloatArray.Builder builder = new Apache.Arrow.FloatArray.Builder();
                builder.AppendRange(DataAs<float>());
                return builder.Build();
            }
                
            else if (Type == typeof(sbyte)) {
                Apache.Arrow.BinaryArray.Builder builder = new Apache.Arrow.BinaryArray.Builder();
                builder.AppendRange(DataAs<byte>());
                return builder.Build();
            }
                
            else if (Type == typeof(int)) {
                Apache.Arrow.Int32Array.Builder builder = new Apache.Arrow.Int32Array.Builder();
                builder.AppendRange(DataAs<int>());
                return builder.Build();
            }
                
            else if (Type == typeof(long)) {
                Apache.Arrow.Int64Array.Builder builder = new Apache.Arrow.Int64Array.Builder();
                builder.AppendRange(DataAs<long>());
                return builder.Build();
            }
                
            else if (Type == typeof(short)) {
                Apache.Arrow.Int16Array.Builder builder = new Apache.Arrow.Int16Array.Builder();
                builder.AppendRange(DataAs<short>());
                return builder.Build();
            }
                
            else if (Type == typeof(byte)) {
                Apache.Arrow.BinaryArray.Builder builder = new Apache.Arrow.BinaryArray.Builder();
                builder.AppendRange(DataAs<byte>());
                return builder.Build();
            }
                
            else if (Type == typeof(uint)) {
                Apache.Arrow.UInt32Array.Builder builder = new Apache.Arrow.UInt32Array.Builder();
                builder.AppendRange(DataAs<uint>());
                return builder.Build();
            }
                
            else if (Type == typeof(ulong)) {
                Apache.Arrow.UInt64Array.Builder builder = new Apache.Arrow.UInt64Array.Builder();
                builder.AppendRange(DataAs<ulong>());
                return builder.Build();
            }
                
            else if (Type == typeof(ushort)) {
                Apache.Arrow.UInt16Array.Builder builder = new Apache.Arrow.UInt16Array.Builder();
                builder.AppendRange(DataAs<ushort>());
                return builder.Build();
            }
                
            else if (Type == typeof(string)) {
                Apache.Arrow.StringArray.Builder builder = new Apache.Arrow.StringArray.Builder();
                builder.AppendRange(DataAs<string>());
                return builder.Build();
            }

            else if (Type == typeof(DateTime)) {
                NanoTimestampArrayBuiler builder = new NanoTimestampArrayBuiler();
                foreach (DateTime dt in DataAs<DateTime>()) {
                    DateTimeOffset offset = new DateTimeOffset(dt);
                    builder.Append(offset);
                }
                return builder.Build();
            }

            else if (Type == typeof(DateTimeOffset)) {
                Apache.Arrow.TimestampArray.Builder builder = new Apache.Arrow.TimestampArray.Builder(TimeUnit.Millisecond);
                builder.AppendRange(DataAs<DateTimeOffset>());
                return builder.Build();
            }
                
            else
                throw new NotImplementedException(String.Format("Cannot handle type {0}", Type.Name));
        }



        private static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public void Append(object value)
        {
            try
            {
                if (value != null)
                {
                    Data.Add(Convert.ChangeType(value, Type));
                }
                else 
                {
                    if (_allowNull)
                        Data.Add(null);
                    else
                        Data.Add(GetDefault(Type));
                }
            }
            catch (Exception e)
            {
                _log.Error(e.ToString() + " type: " + Type.FullName);
                Data.Add(GetDefault(Type));
            }
        }
    }

    [Serializable]
    class DataFrame
    {
        private ILogger _log;
        public string Name { get; set; }

        protected List<Field> fields;

        // RefID is a property that can be set to match a Frame to its orginating query.
        public string RefID { get; set; }

        // Meta is metadata about the Frame, and includes space for custom metadata.
        public FrameMeta Meta { get; set; }

        public DataFrame(string name)
        {
            _log = new ConsoleLogger();
            Name = name;
            fields = new List<Field>();
        }

        public Field AddField<T>(string name)
        {
            return AddField(name, typeof(T));
        }

        public Field AddField(string name, Type type)
        {
            Field field = new Field(name, type);
            fields.Add(field);
            return field;
        }


        public byte[] ToByteArray()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    TypeConverter objConverter = TypeDescriptor.GetConverter(GetType());
                    byte[] data = (byte[])objConverter.ConvertTo(this, typeof(byte[]));
                    bw.Write(data);
                    return ms.ToArray();
                }
            }
        }

        public ByteString ToGprcArrowFrame()
        {
            MemoryStream stream = new MemoryStream();

            RecordBatch.Builder recordBatchBuilder = new RecordBatch.Builder();
            foreach (Field field in fields)
            {                
                recordBatchBuilder.Append(field.Name, true, field.ToArrowArray());
            }

            RecordBatch recordBatch = recordBatchBuilder.Build();
            ArrowFileWriter writer = new ArrowFileWriter(stream, recordBatch.Schema);
            writer.WriteRecordBatch(recordBatch);
            writer.WriteEnd();

            stream.Position = 0;
            
            return ByteString.FromStream(stream);
        }

        public ByteString ToByteString()
        {
            return ByteString.CopyFrom(ToByteArray());
        }
    }
}
