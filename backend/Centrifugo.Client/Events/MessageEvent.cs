using Google.Protobuf;

namespace Centrifugo.Client.Events
{
    public class MessageEvent
    {
        public string Channel { get; set; }

        public ByteString Data { get; }

        public MessageEvent(string channel, ByteString data)
        {
            Data = data;
            Channel = channel;
        }
    }
}