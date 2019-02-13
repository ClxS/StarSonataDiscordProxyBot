namespace StarSonataApi.Communication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;

    using global::StarSonataApi.Messages;

    internal class StarSonataCommClient
    {
        private const int ServerPort = 3030;

        // TODO[CJ] Bad.
        public static string ServerUrl = "localhost";

        private static StarSonataCommClient _client;

        private readonly CancellationTokenSource cancellationTokenSource;

        private readonly Subject<DataReceivedEvent> dataRecievedSubject = new Subject<DataReceivedEvent>();

        private readonly byte[] readBuffer = new byte[64000];

        private int current;

        private StarSonataCommClient()
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.WhenDataRecieved = this.dataRecievedSubject.AsObservable();
        }

        public static StarSonataCommClient Client => _client ?? (_client = new StarSonataCommClient());

        public Socket Socket { get; set; }

        public IObservable<DataReceivedEvent> WhenDataRecieved { get; }

        public void ConnectAsync()
        {
            this.Socket?.Dispose();
            this.Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            this.Socket.Connect(ServerUrl, ServerPort);
            this.StartListening();
        }

        public void SendMessage(IOutgoingMessage message)
        {
            this.Socket.Send(message.GetOutData());
        }

        private void DataHandler()
        {
            var availableBytes = this.Socket.Available;
            this.Socket.Receive(this.readBuffer, this.current, availableBytes, SocketFlags.None);
            this.current = this.current + availableBytes;
            while (this.current >= 3)
            {
                int packetSize = BitConverter.ToInt16(this.readBuffer, 0);
                var packetSizeIncHeader = packetSize + 3;
                if (this.current >= packetSizeIncHeader)
                {
                    var messageBuffer = new List<byte>();
                    messageBuffer.AddRange(this.readBuffer.Take(packetSizeIncHeader));
                    this.current = this.current - packetSizeIncHeader;
                    var offset = 0;
                    while (offset < this.current)
                    {
                        this.readBuffer[offset] = this.readBuffer[offset + packetSizeIncHeader];
                        offset++;
                    }

                    var args = new DataReceivedEvent(messageBuffer.ToArray());
                    this.dataRecievedSubject.OnNext(args);

                    continue;
                }

                break;
            }
        }

        private void StartListening()
        {
            Task.Run(
                async () =>
                {
                    while (!this.cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        this.DataHandler();
                        await Task.Delay(500).ConfigureAwait(false);
                    }
                },
                this.cancellationTokenSource.Token);
        }
    }
}
