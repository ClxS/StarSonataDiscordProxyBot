namespace StarSonataApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;

    using global::StarSonataApi.Communication;
    using global::StarSonataApi.Messages;
    using global::StarSonataApi.Messages.Incoming;
    using global::StarSonataApi.Messages.Outgoing;
    using global::StarSonataApi.Objects;

    public class StarSonataApi
    {
        private readonly Subject<IIncomingMessage> messageReceivedSubject;

        private Dictionary<byte, Func<byte[], IIncomingMessage>> registeredMessageTypes;

        public StarSonataApi()
        {
            this.messageReceivedSubject = new Subject<IIncomingMessage>();
            this.WhenMessageReceived = this.messageReceivedSubject.AsObservable();
        }

        public bool HasStarted { get; private set; }

        public bool IsConnected => StarSonataCommClient.Client.Socket.Connected;

        public IObservable<IIncomingMessage> WhenMessageReceived { get; }

        public void Initialise()
        {
            if (this.HasStarted)
            {
                throw new Exception("Start has already been called.");
            }

            this.HasStarted = true;
            this.registeredMessageTypes = new Dictionary<byte, Func<byte[], IIncomingMessage>>();

            this.AddDefaultSubscriptions();

            this.registeredMessageTypes[MessageConstants.SC_characterlist] = bytes => new CharacterList(bytes);
            this.registeredMessageTypes[MessageConstants.SC_ping] = bytes => new Ping(bytes);
            this.registeredMessageTypes[MessageConstants.SC_textmessage] = bytes => new TextMessage(bytes);
            this.registeredMessageTypes[MessageConstants.SC_loginfail] = bytes => new LoginFail(bytes);
            this.registeredMessageTypes[MessageConstants.SC_disconnect] = bytes => new Disconnect();

            StarSonataCommClient.Client.WhenDataRecieved.Subscribe(
                e =>
                {
                    if (e.Bytes.Length <= 3)
                    {
                        return;
                    }

                    if (!this.registeredMessageTypes.ContainsKey(e.Bytes[2]))
                    {
                        return;
                    }

                    var constructor = this.registeredMessageTypes[e.Bytes[2]];
                    var msg = constructor(e.Bytes.Skip(3).ToArray());
                    this.messageReceivedSubject.OnNext(msg);
                });
        }

        public void SendChatAsync(string message, MessageChannel channel)
        {
            StarSonataCommClient.Client.SendMessage(
                new TextMessageOut(new ChatMessage { Channel = channel, Message = message }));
        }

        public void SendMessage(IOutgoingMessage message)
        {
            StarSonataCommClient.Client.SendMessage(message);
        }

        public void SendImpersonationChatAsync(string channel, string username, string message)
        {
            StarSonataCommClient.Client.SendMessage(
                new ImpersonationMessage(
                    new ChatMessage { Channel = GetChannelForString(channel), Message = message },
                    username));
        }

        public void SetServerUrl(string serverUrl)
        {
            StarSonataCommClient.ServerUrl = serverUrl;
        }

        public async Task TryLoginAsync(string username, string password)
        {
            StarSonataCommClient.Client.ConnectAsync();
            await Task.Delay(500).ConfigureAwait(false);
            StarSonataCommClient.Client.SendMessage(
                new ChatClientLogin(new User { Username = username, Password = password }));
        }

        private static MessageChannel GetChannelForString(string channel)
        {
            if (channel == MessageChannel.All.ToString())
            {
                return MessageChannel.All;
            }

            if (channel == MessageChannel.Trade.ToString())
            {
                return MessageChannel.Trade;
            }

            return MessageChannel.Event;
        }

        private void AddDefaultSubscriptions()
        {
            // Received a ping, send a pong
            this.WhenMessageReceived.Where(msg => msg is Ping).Subscribe(
                msg =>
                {
                    var ping = (Ping)msg;
                    StarSonataCommClient.Client.SendMessage(new Pong(ping.Sec, ping.USec));
                });

            // Login as the first available character
            this.WhenMessageReceived.Where(msg => msg is CharacterList).Subscribe(
                msg =>
                {
                    var characterList = (CharacterList)msg;
                    Console.WriteLine("Logging in as " + characterList.Characters.First().Name);
                    StarSonataCommClient.Client.SendMessage(new SelectCharacter(characterList.Characters.First()));
                });

            // Log Text Messages
            this.WhenMessageReceived.Where(msg => msg is TextMessage).Subscribe(
                msg =>
                {
                    var textMessage = (TextMessage)msg;
                    Console.WriteLine(
                        $"{textMessage.Message.Channel.ToString()} - {textMessage.Message.Username} - {textMessage.Message.Message}");
                });

            this.WhenMessageReceived.Where(msg => msg is LoginFail).Subscribe(
                msg =>
                {
                    var loginMsg = (LoginFail)msg;
                    Console.Error.WriteLine($"Star Sonata Login failed: {loginMsg.FailureReason}");
                    Console.Error.WriteLine("Check you have the correct account name");
                });
        }
    }
}
