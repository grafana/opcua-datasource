
using System.Text.Json;
using System.Text.Json.Serialization;
using Pluginv2;

namespace plugin_dotnet {
    public class Settings {
        public string URL { get; set; }
        #nullable enable
        public string? TLSClientCert { get; set; }
        public string? TLSClientKey { get; set; }
        #nullable disable
        public OPCTimestamp TimestampSource { get; set; }
    }

    public static class RawSettingsParser {
        public static Settings Parse(DataSourceInstanceSettings rawSettings) {
            Settings s = JsonSerializer.Deserialize<Settings>(rawSettings.JsonData.ToString());
            s.URL = rawSettings.Url;

            if (rawSettings.DecryptedSecureJsonData.ContainsKey("tlsClientCert")) {
                s.TLSClientCert = rawSettings.DecryptedSecureJsonData["tlsClientCert"];
            }
            
            if (rawSettings.DecryptedSecureJsonData.ContainsKey("tlsClientKey")) {
                s.TLSClientKey = rawSettings.DecryptedSecureJsonData["tlsClientKey"];
            }
            
            return s;
        }
    }
}