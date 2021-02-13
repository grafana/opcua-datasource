using Protocol;

namespace Centrifugo.Client.Events
{
    public class LeaveEvent
    {
        public string Channel { get; }
        public ClientInfo ClientInfo { get; }

        public LeaveEvent(string channel, ClientInfo clientInfo)
        {
            Channel = channel;
            ClientInfo = clientInfo;
        }
    }
}