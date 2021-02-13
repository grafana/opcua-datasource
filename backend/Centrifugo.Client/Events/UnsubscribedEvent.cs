namespace Centrifugo.Client.Events
{
    public class UnsubscribedEvent
    {
        public bool ShouldResubscribe { get; }

        public string Channel { get; }

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public UnsubscribedEvent(string channel, bool shouldResubscribe)
        {
            ShouldResubscribe = shouldResubscribe;
            Channel = channel;
        }
    }
}