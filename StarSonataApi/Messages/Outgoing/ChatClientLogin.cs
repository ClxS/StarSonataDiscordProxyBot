namespace StarSonataApi.Messages.Outgoing
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class ChatClientLogin : IOutgoingMessage
    {
        private const short Subversion = 2;

        private const short Version = 100;

        private readonly int flags;

        public ChatClientLogin(User user)
        {
            this.User = user;
            this.flags = this.flags | 1;
        }

        public User User { get; set; }

        public byte[] GetOutData()
        {
            var usernameBytes = Encoding.ASCII.GetBytes(this.User.Username);
            var passwordBytes = Encoding.ASCII.GetBytes(this.User.Password);

            var bytes = new List<byte>
                        {
                            MessageConstants.CS_chatclientlogon,
                            1,
                            0,
                            0,
                            0
                        };
            bytes.AddRange(BitConverter.GetBytes(Version));
            bytes.AddRange(BitConverter.GetBytes(this.flags));
            bytes.AddRange(usernameBytes);
            bytes.Add(0);
            bytes.AddRange(passwordBytes);
            bytes.Add(0);
            bytes.Add(0);
            bytes.AddRange(BitConverter.GetBytes(Subversion));
            bytes.AddRange(this.Hash());
            bytes.InsertRange(0, BitConverter.GetBytes((short)(bytes.Count - 1)));

            return bytes.ToArray();
        }

        private byte[] Hash()
        {
            var r1 = 0;
            var r2 = 666;
            while (r1 < this.User.Username.Length)
            {
                r2 = r2 ^ ((short)this.User.Username[r1] << (r1 & 4095));
                r1++;
            }

            r1 = 0;
            while (r1 < this.User.Password.Length)
            {
                r2 = r2 ^ ((short)this.User.Password[r1] << ((this.User.Password.Length - r1) & 4095));
                r1++;
            }

            r2 = r2 ^ (2 + (2 << 4) + 2 + 15);

            return BitConverter.GetBytes(r2);
        }
    }
}
