using System;
using Websocket.Client;

namespace plugin_dotnet
{
    public class Subscriptions 
    {
        public Subscriptions() {
            var ws = new WebsocketClient(new Uri("ws://localhost:3000/live/ws?format=protobuf"));
        }
    }
}