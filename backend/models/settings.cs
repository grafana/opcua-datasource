
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using Grpc.Core.Logging;
using Pluginv2;

namespace plugin_dotnet {
    public class Settings {
        public string ID { get; set; }
        public string URL { get; set; }
        #nullable enable
        public string? TLSClientCert { get; set; }
        public string? TLSClientKey { get; set; }
        public Exception? Error { get; set; }
        #nullable disable
        public OPCTimestamp TimestampSource { get; set; }
        
    }

    public class JSONSettings {
        public string TimestampSource { get; set; }
    }

    public static class RawSettingsParser {
        private static ILogger logger = new ConsoleLogger();
        public static Settings Parse(DataSourceInstanceSettings rawSettings) {
            Settings s = new Settings();
            try {
                string decodedJsonData = rawSettings.JsonData.ToString(Encoding.UTF8);
                JSONSettings js = JsonSerializer.Deserialize<JSONSettings>(decodedJsonData);
                s.URL = rawSettings.Url;
                s.ID = rawSettings.Url + decodedJsonData;

                if (js.TimestampSource == "source") {
                    s.TimestampSource = OPCTimestamp.Source;
                } else {
                    s.TimestampSource = OPCTimestamp.Server;
                }

                if (rawSettings.DecryptedSecureJsonData.ContainsKey("tlsClientCert")) {
                    s.TLSClientCert = rawSettings.DecryptedSecureJsonData["tlsClientCert"];
                }
                
                if (rawSettings.DecryptedSecureJsonData.ContainsKey("tlsClientKey")) {
                    s.TLSClientKey = rawSettings.DecryptedSecureJsonData["tlsClientKey"];
                }
            } catch(Exception ex) {
                logger.Error("Tried parsing settings but failed: {0}", ex);
                s.Error = ex;
            }
            return s;
        }
    }
}