using System;

namespace Centrifugo.Client.Events
{
    public class ConnectedEvent
    {
        public Guid? Client { get; set; }
#nullable enable
        public string? Version { get; set; }
#nullable disable
        public bool? Expires { get; set; }

        public uint? Ttl { get; set; }
    }
}