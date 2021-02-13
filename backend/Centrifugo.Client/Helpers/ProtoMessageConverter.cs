using System;
using Google.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Centrifugo.Client.Helpers
{
#nullable disable
    /// <summary>
    /// Lets Newtonsoft.Json and Protobuf's json converters play nicely
    /// together.  The default Netwtonsoft.Json Deserialize method will
    /// not correctly deserialize proto messages.
    /// </summary>
    internal class ProtoMessageConverter : JsonConverter
    {
        /// <summary>
        /// Called by NewtonSoft.Json's method to ask if this object can serialize
        /// an object of a given type.
        /// </summary>
        /// <returns>True if the objectType is a Protocol Message.</returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(IMessage).IsAssignableFrom(objectType);
        }

        /// <summary>
        /// Reads the json representation of a Protocol Message and reconstructs
        /// the Protocol Message.
        /// </summary>
        /// <param name="objectType">The Protocol Message type.</param>
        /// <returns>An instance of objectType.</returns>
        public override object ReadJson(JsonReader reader,
            System.Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            // The only way to find where this json object begins and ends is by
            // reading it in as a generic ExpandoObject.
            // Read an entire object from the reader.
            var converter = new ExpandoObjectConverter();
            var o = converter.ReadJson(reader, objectType, existingValue,
                serializer);
            // Convert it back to json text.
            var text = JsonConvert.SerializeObject(o);
            // And let protobuf's parser parse the text.
            var message = (IMessage)Activator
                .CreateInstance(objectType);
            return JsonParser.Default.Parse(text, message.Descriptor);
        }

        /// <summary>
        /// Writes the json representation of a Protocol Message.
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value,
            JsonSerializer serializer)
        {
            // Let Protobuf's JsonFormatter do all the work.
            writer.WriteRawValue(JsonFormatter.Default.Format((IMessage)value));
        }
    }
}