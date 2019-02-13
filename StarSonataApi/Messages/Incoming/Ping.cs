namespace StarSonataApi.Messages.Incoming
{
    using System;

    public class Ping : IIncomingMessage
    {
        public int Sec;

        public int USec;

        public Ping(byte[] data)
        {
            this.Sec = BitConverter.ToInt32(data, 0);
            this.USec = data[4];
        }
    }
}
