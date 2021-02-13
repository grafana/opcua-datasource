namespace Centrifugo.Client.Events
{
    public class SubscribedEvent
    {
        public string Channel { get; }

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public SubscribedEvent(string channel)
        {
            Channel = channel;
        }
    }
}