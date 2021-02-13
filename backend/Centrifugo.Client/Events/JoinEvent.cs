using Protocol;

namespace Centrifugo.Client.Events
{
    public class JoinEvent
    {
        public string Channel { get; }
        public ClientInfo ClientInfo { get; }

        public JoinEvent(string channel, ClientInfo clientInfo)
        {
            Channel = channel;
            ClientInfo = clientInfo;
        }
    }
}