using Google.Protobuf;

namespace Centrifugo.Client.Events
{
    public class PublishEvent
    {
        public string Channel { get; }

        public ByteString Data { get; }

        public PublishEvent(string channel, ByteString data)
        {
            Channel = channel;
            Data = data;
        }
    }
}