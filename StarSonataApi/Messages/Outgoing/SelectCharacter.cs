namespace StarSonataApi.Messages.Outgoing
{
    using System;
    using System.Collections.Generic;

    internal class SelectCharacter : IOutgoingMessage
    {
        public SelectCharacter(Character character)
        {
            this.Character = character;
        }

        public Character Character { get; set; }

        public byte[] GetOutData()
        {
            var bytes = new List<byte> { MessageConstants.CS_selectcharacter };
            bytes.AddRange(BitConverter.GetBytes(this.Character.Id));
            bytes.InsertRange(0, BitConverter.GetBytes((short)(bytes.Count - 1)));
            return bytes.ToArray();
        }
    }
}
