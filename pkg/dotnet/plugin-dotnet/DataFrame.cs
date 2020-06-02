using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Threading.Tasks;
using Apache.Arrow;
using Apache.Arrow.Ipc;
using Apache.Arrow.Memory;
using Google.Protobuf;
using Grpc.Core.Logging;
//using Microsoft.Data.Analysis;

namespace plugin_dotnet
{
    // [Serializable]
    // class ValueMapping
    // {
    //     public Int16 ID { get; set; }
    //     public string Operator { get; set; }
    //     public string Text { get; set; }
    //     public sbyte MappingType { get; set; }

    //     // Only valid for MappingType == ValueMap
    //     public string Value { get; set; }

    //     // Only valid for MappingType == RangeMap
    //     public string From { get; set; }
    //     public string To { get; set; }

    //     public ValueMapping() { }
    // }

    // [Serializable]
    // class Threshold
    // {
    //     public double Value { get; set; }
    //     public string Color { get; set; }
    //     public string State { get; set; }

    //     public Threshold() { }
    // }

    // [Serializable]
    // class ThresholdsConfig
    // {
    //     public string Mode { get; set; }

    //     // Must be sorted by 'value', first value is always -Infinity
    //     public Threshold[] Steps { get; set; }
    // }

    // [Serializable]
    // class DataLink
    // {
    //     public string Title { get; set; }
    //     public bool TargetBlank { get; set; }
    //     public string URL { get; set; }

    //     public DataLink() { }
    // }

    // [Serializable]
    // class FieldConfig
    // {
    //     public string DisplayName { get; set; }
    //     public bool Filterable { get; set; }

    //     // Numeric Options
    //     public string Unit { get; set; }
    //     public UInt16 Decimals { get; set; }
    //     public double Min { get; set; }
    //     public double Max { get; set; }

    //     // Convert input values into a display string
    //     public ValueMapping[] Mappings { get; set; }

    //     // Map numeric values to states
    //     public ThresholdsConfig Thresholds { get; set; }

    //     // Map values to a display color
    //     // NOTE: this interface is under development in the frontend... so simple map for now
    //     public Dictionary<string, object> Color { get; set; }

    //     // Used when reducing field values
    //     public string NullValueMode { get; set; }

    //     // The behavior when clicking on a result
    //     public DataLink[] Links { get; set; }

    //     // Alternative to empty string
    //     public string NoValue { get; set; }

    //     // Panel Specific Values
    //     public Dictionary<string, object> Custom { get; set; }

    //     public FieldConfig() { }
    // }

    // [Serializable]
    // class Notice
    // {
    //     // Severity is the severity level of the notice: info, warning, or error.
    //     public int Severity { get; set; }

    //     // Text is freeform descriptive text for the notice.
    //     public string Text { get; set; }

    //     // Link is an optional link for display in the user interface and can be an
    //     // absolute URL or a path relative to Grafana's root url.
    //     public string Link { get; set; }

    //     // Inspect is an optional suggestion for which tab to display in the panel inspector
    //     // in Grafana's User interface. Can be meta, error, data, or stats.
    //     public int Inspect { get; set; }

    //     public Notice() { }
    // }

    // [Serializable]
    // class FrameMeta
    // {
    //     // Datasource specific values
    //     public Dictionary<string, object> Custom { get; set; }

    //     // Stats is TODO
    //     public object Stats { get; set; }

    //     // Notices provide additional information about the data in the Frame that
    //     // Grafana can display to the user in the user interface.
    //     public Notice[] Notices { get; set; }

    //     public FrameMeta() { }
    // }

    // [Serializable]
    // class Field
    // {
    //     public string Name { get; set; }
    //     public Dictionary<string, string> Labels { get; set; }

    //     public FieldConfig Config { get; set; }


    //     public List<object> vector { get; set; }

    //     public Field()
    //     {
    //         vector = new List<object>();
    //     }

    //     public Field(string name) : this()
    //     {
    //         Name = name;
    //     }

    //     public void Append(object value)
    //     {
    //         vector.Add(value);
    //     }
    // }

    // [Serializable]
    // class DataFrame
    // {
    //     public string Name { get; set; }

    //     private List<Field> fields;

    //     // RefID is a property that can be set to match a Frame to its orginating query.
    //     public string RefID { get; set; }

    //     // Meta is metadata about the Frame, and includes space for custom metadata.
    //     public FrameMeta Meta { get; set; }

    //     public DataFrame(string name)
    //     {
    //         Name = name;
    //         fields = new List<Field>();
    //     }

    //     public Field AddField(string name)
    //     {
    //         Field field = new Field(name);
    //         fields.Add(field);
    //         return field;
    //     }

    //     public string ToJson()
    //     {
    //         return JsonSerializer.Serialize<DataFrame>(this);
    //     }

    //     public byte[] ToByteArray()
    //     {
    //         BinaryFormatter bf = new BinaryFormatter();
    //         using (var ms = new MemoryStream())
    //         {
    //             bf.Serialize(ms, this);
    //             return ms.ToArray();
    //         }
    //     }

    //     public async Task<ByteString> ToArrow()
    //     {
    //         var memoryAllocator = new NativeMemoryAllocator(alignment: 64);

    //         // Build a record batch using the Fluent API

    //         var recordBatch = new RecordBatch.Builder(memoryAllocator)
    //             .Append("ServerTime", false, col => col.Date64(array => array.AppendRange(new DateTimeOffset[] { DateTimeOffset.MinValue, DateTimeOffset.MaxValue })))
    //             .Append("SystemTime", false, col => col.Date64(array => array.AppendRange(new DateTimeOffset[] { DateTimeOffset.MinValue, DateTimeOffset.MaxValue })))
    //             .Append("Status", false, col => col.Int64(array => array.AppendRange(new Int64[] { Int64.MinValue, Int64.MaxValue })))
    //             .Append("Value", false, col => col.Double(array => array.AppendRange(new Double[] { Double.MinValue, Double.MaxValue })))
    //             .Build();

            
    //         BinaryFormatter bf = new BinaryFormatter();
    //         using (var ms = new MemoryStream())
    //         {
    //             bf.Serialize(ms, this);
    //             using (var writer = new ArrowFileWriter(ms, recordBatch.Schema))
    //             {
    //                 await writer.WriteRecordBatchAsync(recordBatch);
    //                 await writer.WriteFooterAsync();
    //                 return ByteString.CopyFrom(ms.ToArray());
    //             }
    //         }
    //     }

    //     public ByteString ToByteString()
    //     {
    //         return ByteString.CopyFrom(ToByteArray());
    //     }

    //     public Field[] Fields => fields.ToArray();

    // }
}
