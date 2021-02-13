using Centrifugo.Client.Enums;
using Centrifugo.Client.Events;
using Centrifugo.Client.Helpers;
using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Centrifugo.Client
{
#nullable enable
    public sealed class Subscription : IDisposable
    {
        public string Channel { get; }

        public ulong Offset { get; internal set; }

        public string? Epoch { get; internal set; }

        public SubscriptionState State { get; internal set; }

        #region Internal channel event sources

        internal Subject<PublishEvent>? PublishEventSource { get; private set; }

        internal Subject<JoinEvent>? JoinEventSource { get; private set; }

        internal Subject<LeaveEvent>? LeaveEventSource { get; private set; }

        internal Subject<SubscribedEvent>? SubscribedEventSource { get; private set; }

        internal Subject<SubscriptionErrorEvent>? SubscriptionErrorEventSource { get; private set; }

        internal Subject<UnsubscribedEvent>? UnsubscribedEventSource { get; private set; }

        #endregion Internal channel event sources

        internal Subscription(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel) || channel.Length > 255)
            {
                throw new ArgumentException("Channel name must be non empty string, and length not longer that 255.");
            }
            Channel = channel;
            State = SubscriptionState.New;
        }

        #region Events

        public void OnPublish(Func<PublishEvent, Task> publishEventHandler)
        {
            PublishEventSource ??= new Subject<PublishEvent>();
            PublishEventSource.SubscribeAsync(publishEventHandler);
        }

        public void OnPublish(Action<PublishEvent> publishEventHandler)
        {
            PublishEventSource ??= new Subject<PublishEvent>();
            PublishEventSource.Subscribe(publishEventHandler);
        }

        public void OnSubscribe(Action<SubscribedEvent> subscribedEventHandler)
        {
            SubscribedEventSource ??= new Subject<SubscribedEvent>();
            SubscribedEventSource.Subscribe(subscribedEventHandler);
        }

        public void OnSubscribe(Func<SubscribedEvent, Task> subscribedEventHandler)
        {
            SubscribedEventSource ??= new Subject<SubscribedEvent>();
            SubscribedEventSource.SubscribeAsync(subscribedEventHandler);
        }

        public void OnSubscriptionError(Action<SubscriptionErrorEvent> subscriptionErrorEventHandler)
        {
            SubscriptionErrorEventSource ??= new Subject<SubscriptionErrorEvent>();
            SubscriptionErrorEventSource.Subscribe(subscriptionErrorEventHandler);
        }

        public void OnSubscriptionError(Func<SubscriptionErrorEvent, Task> subscriptionErrorEventHandler)
        {
            SubscriptionErrorEventSource ??= new Subject<SubscriptionErrorEvent>();
            SubscriptionErrorEventSource.SubscribeAsync(subscriptionErrorEventHandler);
        }

        public void OnUnsubscribe(Action<UnsubscribedEvent> unsubscribedEventHandler)
        {
            UnsubscribedEventSource ??= new Subject<UnsubscribedEvent>();
            UnsubscribedEventSource.Subscribe(unsubscribedEventHandler);
        }

        public void OnUnsubscribe(Func<UnsubscribedEvent, Task> unsubscribedEventHandler)
        {
            UnsubscribedEventSource ??= new Subject<UnsubscribedEvent>();
            UnsubscribedEventSource.SubscribeAsync(unsubscribedEventHandler);
        }

        public void OnJoin(Action<JoinEvent> joinEventHandler)
        {
            JoinEventSource ??= new Subject<JoinEvent>();
            JoinEventSource.Subscribe(joinEventHandler);
        }

        public void OnJoin(Func<JoinEvent, Task> joinEventHandler)
        {
            JoinEventSource ??= new Subject<JoinEvent>();
            JoinEventSource.SubscribeAsync(joinEventHandler);
        }

        public void OnLeave(Action<LeaveEvent> leaveEventHandler)
        {
            LeaveEventSource ??= new Subject<LeaveEvent>();
            LeaveEventSource.Subscribe(leaveEventHandler);
        }

        public void OnLeave(Func<LeaveEvent, Task> leaveEventHandler)
        {
            LeaveEventSource ??= new Subject<LeaveEvent>();
            LeaveEventSource.SubscribeAsync(leaveEventHandler);
        }

        #endregion Events

        // TODO: publish, unsubscribe, subscribe, history, presence, presense_stats

        #region Disposable support

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            PublishEventSource?.OnCompleted();
            PublishEventSource?.Dispose();

            JoinEventSource?.OnCompleted();
            JoinEventSource?.Dispose();

            LeaveEventSource?.OnCompleted();
            LeaveEventSource?.Dispose();

            SubscribedEventSource?.OnCompleted();
            SubscribedEventSource?.Dispose();

            UnsubscribedEventSource?.OnCompleted();
            UnsubscribedEventSource?.Dispose();
            
            SubscriptionErrorEventSource?.OnCompleted();
            SubscriptionErrorEventSource?.Dispose();
        }

        #endregion Disposable support
    }
#nullable disable
}