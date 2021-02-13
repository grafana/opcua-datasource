using System;

namespace Centrifugo.Client.Options
{
    public class CentrifugoClientOptions
    {
#nullable enable

        public Uri Uri { get; set; }

        public string? ClientProvidedInfo { get; set; }

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public CentrifugoClientOptions(Uri uri, string clientProvidedInfo)
        {
            Uri = uri;
            ClientProvidedInfo = clientProvidedInfo;
        }

        public CentrifugoClientOptions()
        {
            Uri = new Uri("ws://localhost:8000/connection/websocket");
        }
    }
#nullable disable
}