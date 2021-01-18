using System;
using System.Collections.Generic;
using System.Text;

namespace plugin_dotnet
{
    public static class UaEvents
    {
        private static readonly Dictionary<string, Type> _typeForFieldName;
        private static readonly Dictionary<string, Func<object, object>> _converter;
        static UaEvents()
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
    }
}
