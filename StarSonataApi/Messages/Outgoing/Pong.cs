namespace StarSonataApi.Messages.Outgoing
{
    using System;
    using System.Collections.Generic;

    internal class Pong : IOutgoingMessage
    {
        public Pong(int sec, int usec)
        {
            this.Sec = sec;
            this.USec = usec;
        }

        public int Sec { get; set; }

        public int USec { get; set; }

        public byte[] GetOutData()
        {
            var secBytes = BitConverter.GetBytes(this.Sec);
            var usecBytes = BitConverter.GetBytes(this.USec);
            var bytes = new List<byte> { MessageConstants.CS_pong };
            bytes.AddRange(secBytes);
            bytes.AddRange(usecBytes);
            bytes.AddRange(BitConverter.GetBytes((short)30));
            bytes.Add(65);
            bytes.AddRange(BitConverter.GetBytes((short)32));
            bytes.AddRange(BitConverter.GetBytes((short)1024));
            bytes.AddRange(BitConverter.GetBytes((short)768));
            bytes.Add(65);
            bytes.InsertRange(0, BitConverter.GetBytes((short)(bytes.Count - 1)));

            return bytes.ToArray();
        }
    }
}
