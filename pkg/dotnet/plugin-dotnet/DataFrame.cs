using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using Apache.Arrow;
using Apache.Arrow.Ipc;
using Google.Protobuf;
using Microsoft.Data.Analysis;
using System.Collections;
using Microsoft.VisualBasic;
using Apache.Arrow.Types;
using Microsoft.Extensions.Logging;

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

        public Field(ILogger log, string name, Type type)
        {
            _log = log;
            Name = name;
            Type = type;
            Data = new List<object>();
        }

        public List<T> DataAs<T>()
        {
            return Data.Cast<T>().ToList();
        }

        //public void Append<T>(T value)
        //{ 
        //    Data.Add(value);
        //}

        public void Append(object value)
        {
            try
            {
                Data.Add(Convert.ChangeType(value, Type));
            }
            catch (Exception e)
            {
                _log.LogError(e.ToString());
                //Data.
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

        public DataFrame(ILogger log, string name)
        {
            _log = log;
            Name = name;
            fields = new List<Field>();
        }

        public Field AddField<T>(string name)
        {
            return AddField(name, typeof(T));
        }

        public Field AddField(string name, Type type)
        {
            Field field = new Field(_log, name, type);
            fields.Add(field);
            return field;
        }


        public byte[] ToByteArray()
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, this);
                return ms.ToArray();
            }
        }

        public ByteString ToGprcArrowFrame()
        {
            var columns = fields.Select(field => DataFrameColumnFactory.Create(field));

            Microsoft.Data.Analysis.DataFrame dataFrame = new Microsoft.Data.Analysis.DataFrame(columns.ToArray());

            MemoryStream stream = new MemoryStream();

            foreach (RecordBatch recordBatch in dataFrame.ToArrowRecordBatches())
            {                
                ArrowStreamWriter writer = new ArrowStreamWriter(stream, recordBatch.Schema);
                writer.WriteRecordBatchAsync(recordBatch).GetAwaiter().GetResult();
            }

            stream.Position = 0;
            return ByteString.FromStream(stream);
        }

        public ByteString ToByteString()
        {
            return ByteString.CopyFrom(ToByteArray());
        }
    }


    public class OpcUaDataFrameColumn<T> : PrimitiveDataFrameColumn<T> where T : unmanaged
    {
        List<T> _values;
        IDictionary<string, string> _metadata = null;
        public OpcUaDataFrameColumn(string name, List<T> values, IDictionary<string, string> metadata = null) : base(name, values)
        {
            _values = values;
            _metadata = metadata;
        }

        private IArrowType GetArrowType()
        {
            if (typeof(T) == typeof(bool))
                return BooleanType.Default;
            else if (typeof(T) == typeof(double))
                return DoubleType.Default;
            else if (typeof(T) == typeof(float))
                return FloatType.Default;
            else if (typeof(T) == typeof(sbyte))
                return Int8Type.Default;
            else if (typeof(T) == typeof(int))
                return Int32Type.Default;
            else if (typeof(T) == typeof(long))
                return Int64Type.Default;
            else if (typeof(T) == typeof(short))
                return Int16Type.Default;
            else if (typeof(T) == typeof(byte))
                return UInt8Type.Default;
            else if (typeof(T) == typeof(uint))
                return UInt32Type.Default;
            else if (typeof(T) == typeof(ulong))
                return UInt64Type.Default;
            else if (typeof(T) == typeof(ushort))
                return UInt16Type.Default;
            else if (typeof(T) == typeof(DateTime))
                return TimestampType.Default;
            else if (typeof(T) == typeof(string))
                return StringType.Default;
            else
                throw new NotImplementedException(nameof(T));
        }

        protected override Apache.Arrow.Field GetArrowField() => new Apache.Arrow.Field(Name, GetArrowType(), NullCount != 0, _metadata);

        protected override Apache.Arrow.Array ToArrowArray(long startIndex, int numberOfRows)
        {
            try
            {
                return base.ToArrowArray(startIndex, numberOfRows);
            }
            catch(NotImplementedException ex)
            {
                if (DataType == typeof(DateTime))
                {
                    List<byte> raw = new List<byte>();
                    for (int i = 0; i < _values.Count; i++) 
                    {
                        DateTime dateTime = Convert.ToDateTime(_values[i]);
                        long unixTime = ((DateTimeOffset)dateTime).ToUnixTimeMilliseconds();
                        raw.AddRange(BitConverter.GetBytes(unixTime));
                    }

                    //log.Debug("Start {0}, numRows {1}", startIndex, numberOfRows);
                    //byte[] result = new byte[numberOfRows];
                    //System.Array.Copy(raw.ToArray(), startIndex, result, 0, numberOfRows);
                    ArrowBuffer valueBuffer = new ArrowBuffer(raw.ToArray());
                    ArrowBuffer nullBitmapBuffer = new ArrowBuffer();
                    return new TimestampArray(TimestampType.Default, valueBuffer, nullBitmapBuffer, _values.Count, 0, 0);
                }

                throw ex;
            }
        }
    }
}
