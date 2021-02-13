using Centrifugo.Client.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Centrifugo.Client.Abstractions
{
    public interface ICentrifugoClient : IDisposable
    {
        void OnConnect(Func<ConnectedEvent, Task> connectedEventHandler);

        void OnConnect(Action<ConnectedEvent> connectedEventHandler);

        void SetToken(string token);

        Task ConnectAsync(CancellationToken cancellationToken = default);

        Task SubscribeAsync(Subscription subscription, CancellationToken cancellationToken = default);

        Task UnsubscribeAsync(string channel, CancellationToken cancellationToken = default);

        Task PublishAsync(string channel, ReadOnlyMemory<byte> payload);

        Subscription CreateNewSubscription(string channel);

        void OnError(Func<ErrorEvent, Task> errorEventHandler);

        void OnError(Action<ErrorEvent> errorEventHandler);
    }
}