using Centrifugo.Client.Abstractions;
using Centrifugo.Client.Enums;
using Centrifugo.Client.Events;
using Centrifugo.Client.Exceptions;
using Centrifugo.Client.Helpers;
using Google.Protobuf;
using Newtonsoft.Json;
using Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace Centrifugo.Client
{
#nullable enable

    // TODO: остальные методы, батчи, recovery, shutdown handle.
    // TODO: https://centrifugal.github.io/centrifugo/server/protocol/#rpc-like-calls-publish-history-presence
    // Subscribe должен вернуть объект Subscription с методами publish, unsubscribe, subscribe, history, presence, presense_stats
    // метод publish без подписки. rpc, send

    public sealed class CentrifugoClient : ICentrifugoClient
    {
        #region Поля

        private string? _token;
        private uint _nextOperationId = 1;
        private bool _authorized;
        //private Guid? _clientId;

        private readonly IWebsocketClient _ws;

        private readonly ConcurrentDictionary<uint, TaskCompletionSource<ByteString>> _operations;

        private readonly ConcurrentDictionary<string, Subscription> _channels;

        private readonly IObservable<ConnectedEvent> _connectedEvents;
        private readonly Subject<ConnectedEvent> _connectedEventSource = new Subject<ConnectedEvent>();
        
        private readonly IObservable<ErrorEvent> _errorEvents;
        private readonly Subject<ErrorEvent> _errorEventsSource = new Subject<ErrorEvent>();

        private readonly IObservable<MessageEvent> _messageEvents;
        private readonly Subject<MessageEvent> _messageEventsSource = new Subject<MessageEvent>();

        private readonly Subject<long> _pingEventsSource = new Subject<long>();

        #endregion Поля

        #region Конструктор

        public CentrifugoClient(IWebsocketClient ws)
        {
            _ws = ws ?? throw new ArgumentNullException(nameof(ws));

            if (!_ws.Url.Query.Contains("format=protobuf"))
            {
                throw new NotSupportedException("формат общения Json не поддерживается. Используйте ?format=protobuf.");
            }
            _ws.IsTextMessageConversionEnabled = false;
            _connectedEvents = _connectedEventSource;
            _messageEvents = _messageEventsSource;
            _errorEvents = _errorEventsSource;

            //_pingEventsSource
            //    .Concat(Observable.Interval(TimeSpan.FromSeconds(10)))
            //    .SubscribeAsync(PingAsync);

            _operations = new ConcurrentDictionary<uint, TaskCompletionSource<ByteString>>();
            _channels = new ConcurrentDictionary<string, Subscription>();

            HandleConnection();
            HandleSocketMessages();
        }

        #endregion Конструктор

        #region Методы (public)

        #region Events

        // OnRefresh.. when need to refresh token.
        // OnMessage

        public void OnError(Func<ErrorEvent, Task> errorEventHandler)
        {
            _errorEvents.SubscribeAsync(errorEventHandler);
        }

        public void OnError(Action<ErrorEvent> errorEventHandler)
        {
            _errorEvents.Subscribe(errorEventHandler);
        }

        public void OnConnect(Func<ConnectedEvent, Task> connectedEventHandler)
        {
            _connectedEvents.SubscribeAsync(connectedEventHandler);
        }

        public void OnConnect(Action<ConnectedEvent> connectedEventHandler)
        {
            _connectedEvents.Subscribe(connectedEventHandler);
        }

        #endregion Events

        public void SetToken(string token)
        {
            _token = token;
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_token))
            {
                throw new InvalidOperationException("Токен не установлен");
            }

            if (_ws.NativeClient?.State == WebSocketState.Open && _authorized)
            {
                return;
            }

            await _ws.StartOrFail();

            var connectCommand = new Command
            {
                Id = InterlockedEx.Increment(ref _nextOperationId),
                Method = MethodType.Connect,
                Params = new ConnectRequest
                {
                    Token = _token
                }.ToByteString()
            };

            // TODO: retry with exponential backoff, reinit connection on connection lost
            var response = await HandleCommand(connectCommand, cancellationToken);

            var connectResult = ConnectResult.Parser.ParseFrom(response);
            _authorized = true;
            //_clientId = authResult.Client;

            _connectedEventSource.OnNext(new ConnectedEvent
            {
                Client = Guid.Parse(connectResult.Client),
                Expires = connectResult.Expires,
                Ttl = connectResult.Ttl,
                Version = connectResult.Version
            });
        }

        /// <summary>
        /// Создать новый канал.
        /// </summary>
        /// <param name="channel">Название канала.</param>
        /// <exception cref="ArgumentException">
        /// В случае, если наименование канала не соответствует требованиям.
        /// </exception>
        /// <returns>Канал, на который можно подписаться.</returns>
        public Subscription CreateNewSubscription(string channel)
        {
            if (_channels.TryGetValue(channel, out _))
            {
                // TODO: duplicate sub exception
                throw new Exception();
            }

            var subscription = new Subscription(channel);
            if (_channels.TryAdd(channel, subscription))
            {
                return subscription;
            }

            // TODO: не удалось добавить.
            throw new Exception();
        }

        public async Task SubscribeAsync(Subscription subscription, CancellationToken cancellationToken = default)
        {
            subscription.State = SubscriptionState.Subscribing;

            // если канал уже есть, нельзя давать повторно регаться.
            if (_ws.NativeClient?.State != WebSocketState.Open || !_authorized)
            {
                subscription.State = SubscriptionState.SubscriptionError;

                subscription.SubscriptionErrorEventSource?.OnNext(new SubscriptionErrorEvent(subscription.Channel));
                // TODO: exceptions
                throw new Exception();
            }

            var offset = subscription.Offset;
            subscription.Offset = InterlockedEx.Increment(ref offset);
            var subscribeCommand = new Command
            {
                Id = InterlockedEx.Increment(ref _nextOperationId),
                Method = MethodType.Subscribe,
                Params = new SubscribeRequest
                {
                    Channel = subscription.Channel,
                    Offset = subscription.Offset,
                    Recover = false,
                    Token = _token,
                }.ToByteString()
            };

            try
            {
                await HandleCommand(subscribeCommand, cancellationToken);

                subscription.State = SubscriptionState.Subscribed;

                subscription.SubscribedEventSource?.OnNext(new SubscribedEvent(subscription.Channel));
            }
            catch (CentrifugoException e)
            {
                subscription.State = SubscriptionState.SubscriptionError;

                subscription.SubscriptionErrorEventSource?.OnNext(new SubscriptionErrorEvent(subscription.Channel, e));

                throw;
            }
        }

        public async Task UnsubscribeAsync(string channel, CancellationToken cancellationToken = default)
        {
            if (_ws.NativeClient?.State != WebSocketState.Open)
            {
                // try reconnect or refresh, otherwise throw ex

                return;
            }

            if (!_authorized)
            {
                // try reconnect or refresh, otherwise throw ex

                return;
            }

            if (_channels.TryRemove(channel, out var subscription))
            {
                var unsubscribeCommand = new Command
                {
                    Id = InterlockedEx.Increment(ref _nextOperationId),
                    Method = MethodType.Unsubscribe,
                    Params = new UnsubscribeRequest
                    {
                        Channel = channel
                    }.ToByteString()
                };

                await HandleCommand(unsubscribeCommand, cancellationToken);

                subscription.State = SubscriptionState.Unsubscribed;
                subscription.UnsubscribedEventSource?.OnNext(new UnsubscribedEvent(subscription.Channel, false));
                subscription.Dispose();
            }
        }

        /// <summary>
        /// Опубликовать в канал массив байт сообщения.
        /// </summary>
        /// <param name="channel">Наименование канала.</param>
        /// <param name="payload">Тело сообщения.</param>
        public async Task PublishAsync(string channel, ReadOnlyMemory<byte> payload)
        {
            var publishCommand = new Command
            {
                Id = InterlockedEx.Increment(ref _nextOperationId),
                Method = MethodType.Publish,
                Params = new PublishRequest
                {
                    Channel = channel,
                    Data = ByteString.CopyFrom(payload.Span)
                }.ToByteString()
            };

            await HandleCommand(
                publishCommand,
                CancellationToken.None
            );
        }

        // Only for centrifuge library
        public Task SendAsync(string channel, ReadOnlyMemory<byte> payload)
        {
            throw new NotSupportedException();
            //var sendCommand = new Command
            //{
            //    Id = 0, // not tracked.
            //    Method = MethodType.Send,
            //    Params = ByteString.CopyFrom(payload.Span)
            //};

            //return HandleCommand(sendCommand, CancellationToken.None);
        }

        #region IDisposable support

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _ws?.Dispose();

            _pingEventsSource?.OnCompleted();
            _pingEventsSource?.Dispose();

            _connectedEventSource?.OnCompleted();
            _connectedEventSource?.Dispose();

            _messageEventsSource?.OnCompleted();
            _messageEventsSource?.Dispose();

            _errorEventsSource?.OnCompleted();
            _errorEventsSource?.Dispose();

            foreach (var taskCompletionSource in _operations)
            {
                // TODO: custom exception
                taskCompletionSource.Value.TrySetException(new Exception("Client disposed."));
            }
            _operations.Clear();

            foreach (var channel in _channels)
            {
                channel.Value.Dispose();
            }
            _channels.Clear();
        }

        #endregion IDisposable support

        #endregion Методы (public)

        private Task<ByteString> HandleCommand(Command command, CancellationToken cancellationToken)
        {
            if (_ws.NativeClient?.State != WebSocketState.Open)
            {
                return NullTaskResult.NotConnected;
            }

            if (!command.IsAwaitable())
            {
                Send(command, _ws);

                return NullTaskResult.Instance;
            }

            if (command.Id == default)
            {
                throw new Exception();
            }

            var tcs = new TaskCompletionSource<ByteString>(TaskCreationOptions.RunContinuationsAsynchronously);

            TrackOperation(tcs);
            Send(command, _ws);

            static void Send(IMessage message, IWebsocketClient ws)
            {
                using var ms = new MemoryStream();
                message.WriteDelimitedTo(ms);
                ws.Send(ms.GetBuffer());
            }

            void TrackOperation(TaskCompletionSource<ByteString> operation)
            {
                _operations[command.Id] = operation;
                cancellationToken.Register(() =>
                {
                    operation.TrySetCanceled(cancellationToken);
                    _operations.TryRemove(command.Id, out _);
                });
            }

            return tcs.Task;
        }

        private static byte[] SerializeJsonString(object obj)
        {
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new ProtoMessageConverter() // if obj is IMessage from protobuff, it will be serialized by protobuff.
                }
            };

            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, settings));
        }

        private void HandleConnection()
        {
            // reconnect -> auth and resub: _channels
            _ws.ReconnectionHappened.SubscribeAsync(r =>
            {
                //_clientId = null;

                switch (r.Type)
                {
                    case ReconnectionType.Initial:
                        break;
                    case ReconnectionType.Lost:
                        break;
                    case ReconnectionType.NoMessageReceived:
                        // ping pong нужен
                        break;
                    case ReconnectionType.Error:
                        break;
                    case ReconnectionType.ByUser:
                        break;
                    case ReconnectionType.ByServer:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return Task.CompletedTask;
            });
        }

        private void HandleSocketMessages()
        {
            _ws.MessageReceived.Subscribe(
                response =>
                {
                    Console.WriteLine();
                    Console.WriteLine("------------------------- frame start -------------------------");

                    Console.WriteLine(Encoding.UTF8.GetString(response.Binary));

                    Console.WriteLine("-------------------------- frame end --------------------------");

                    using var ms = new MemoryStream(response.Binary);

                    while (ms.Position < ms.Length)
                    {
                        Reply reply;
                        try
                        {
                            reply = Reply.Parser.ParseDelimitedFrom(ms);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);

                            continue;
                        }

                        if (reply.Error != null)
                        {
                            switch (reply.Error.Code)
                            {
                                case 100:

                                    // push refresh token

                                    break;

                                case 101:
                                {
                                    // unauthorized exception
                                    if (reply.Id != 0 && _operations.TryRemove(reply.Id, out var taskCompletionSource))
                                    {
                                        _authorized = false;

                                        taskCompletionSource.TrySetException(
                                            new UnauthorizedException(
                                                reply.Error.Code,
                                                reply.Error.Message
                                            )
                                        );
                                    }
                                    else
                                    {
                                        // warning...
                                    }

                                    break;
                                }
                                case 106:
                                {
                                    // длина канала исчерпана и прочие ошибки limit exceeded
                                    if (reply.Id != 0 && _operations.TryRemove(reply.Id, out var taskCompletionSource))
                                    {
                                        taskCompletionSource.TrySetException(
                                            new CentrifugoException(
                                                reply.Error.Code,
                                                reply.Error.Message
                                            )
                                        );
                                    }
                                    else
                                    {
                                        // warning...
                                    }

                                    break;
                                }
                                case 107: // bad request
                                {
                                    if (reply.Id != 0 && _operations.TryRemove(reply.Id, out var taskCompletionSource))
                                    {
                                        taskCompletionSource.TrySetException(
                                            new CentrifugoException(
                                                reply.Error.Code,
                                                reply.Error.Message
                                            )
                                        );
                                    }

                                    _errorEventsSource?.OnNext(new ErrorEvent(reply.Error.Code, reply.Error.Message));

                                    break;
                                }
                                case 109:
                                {
                                    // token expired
                                    if (reply.Id != 0 &&
                                        _operations.TryRemove(reply.Id, out var taskCompletionSource))
                                    {
                                        taskCompletionSource.TrySetException(
                                            new TokenExpiredException(
                                                reply.Error.Code,
                                                reply.Error.Message
                                            )
                                        );
                                    }
                                    else
                                    {
                                        // warning...
                                    }

                                    break;
                                }
                                default:
                                    break;
                            }
                        }
                        else if (reply.Result != ByteString.Empty)
                        {
                            if (reply.Id == 0) // если 0, то это push
                            {
                                var push = Push.Parser.ParseFrom(reply.Result);

                                // async messages from server
                                if (push.Type == PushType.Message)
                                {
                                    var message = Message.Parser.ParseFrom(push.Data);

                                    _messageEventsSource?.OnNext(new MessageEvent(push.Channel, message.Data));

                                    continue;
                                }

                                if (!_channels.TryGetValue(push.Channel, out var channel))
                                {
                                    continue;
                                }

                                switch (push.Type)
                                {
                                    case PushType.Publication:
                                    {
                                        var publication = Publication.Parser.ParseFrom(push.Data);

                                        channel.PublishEventSource?.OnNext(new PublishEvent(push.Channel,
                                            publication.Data));

                                        break;
                                    }
                                    case PushType.Join:

                                        var join = Join.Parser.ParseFrom(push.Data);

                                        channel.JoinEventSource?.OnNext(new JoinEvent(push.Channel, join.Info));

                                        break;
                                    case PushType.Leave:

                                        var leave = Leave.Parser.ParseFrom(push.Data);

                                        channel.LeaveEventSource?.OnNext(new LeaveEvent(push.Channel, leave.Info));

                                        break;
                                    case PushType.Unsub:
                                        var unsub = Unsub.Parser.ParseFrom(push.Data);

                                        channel.UnsubscribedEventSource?.OnNext(
                                            new UnsubscribedEvent(push.Channel, unsub.Resubscribe));

                                        break;
                                    case PushType.Sub:

                                        //var sub = Sub.Parser.ParseFrom(push.Data);

                                        channel.SubscribedEventSource?.OnNext(new SubscribedEvent(push.Channel));

                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }
                            else if (_operations.TryRemove(reply.Id, out var taskCompletionSource))
                            {
                                taskCompletionSource.SetResult(reply.Result);
                            }
                        }
                        else if (reply.Id != 0 && _operations.TryRemove(reply.Id, out var taskCompletionSource))
                        {
                            taskCompletionSource.TrySetResult(reply.Result);
                        }
                    }
                });
        }

        private async Task<object?> PingAsync(long value)
        {
            if (_ws.NativeClient?.State != WebSocketState.Open || !_authorized)
            {
                return NullTaskResult.Instance;
            }

            var pingCommand = new Command
            {
                Id = InterlockedEx.Increment(ref _nextOperationId),
                Method = MethodType.Ping
            };

            var timeout = TimeSpan.FromSeconds(5);
            try
            {
                var tokenSource = new CancellationTokenSource(timeout);

                return await
                    HandleCommand(pingCommand, tokenSource.Token)
                        .TimeoutAfterAsync(timeout, cancellationToken: tokenSource.Token);
            }
            // исключения в observable кидать нельзя - останавливаются.
            // TODO: если пинг не прошел, нужно реконнектиться заново
            catch (Exception)
            {
                return null;
            }
        }
    }
#nullable disable
    // channel name 255 symbols, only ascii
    // : -> namespaces (настройки подхватятся с неймспейса)
    // $ private
    // # user boundary  news#42,43 (юзеры 42 и 43 только получат)
    // *, &, / -> reserved
}