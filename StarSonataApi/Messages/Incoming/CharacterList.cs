namespace StarSonataApi.Messages.Incoming
{
    using System;
    using System.Collections.Generic;

    public class CharacterList : IIncomingMessage
    {
        public CharacterList(byte[] data)
        {
            // TODO[CJ] Clean up
            var characters = new List<Character>();
            var byteOffset = 0;
            var characterCount = BitConverter.ToInt16(data, byteOffset);
            byteOffset += sizeof(short);
            for (var i = 0; i < characterCount; ++i)
            {
                if (byteOffset < data.Length)
                {
                    var id = ByteUtility.GetInt(data, ref byteOffset);
                    var name = ByteUtility.GetString(data, ref byteOffset);
                    var unknown1 = ByteUtility.GetInt(data, ref byteOffset);
                    var unknown2 = ByteUtility.GetInt(data, ref byteOffset);
                    var unknown3 = ByteUtility.GetInt(data, ref byteOffset);
                    var className = ByteUtility.GetString(data, ref byteOffset);
                    var unknown4 = ByteUtility.GetInt(data, ref byteOffset);

                    if (byteOffset < data.Length)
                    {
                        var hasSkins = ByteUtility.GetBoolean(data, ref byteOffset);
                        if (hasSkins)
                        {
                            var skinCount = ByteUtility.GetInt(data, ref byteOffset);
                            for (var j = 0; j < skinCount; ++j)
                            {
                                var unknown5 = ByteUtility.GetByte(data, ref byteOffset);
                                var skinName = ByteUtility.GetString(data, ref byteOffset);
                                var skinName2 = ByteUtility.GetString(data, ref byteOffset);
                                var unknown6 = ByteUtility.GetLong(data, ref byteOffset);
                                var unknown7 = ByteUtility.GetByte(data, ref byteOffset);
                                for (var k = 0; k < 6; ++k)
                                {
                                    var skinFile = ByteUtility.GetString(data, ref byteOffset);
                                }
                            }
                        }
                    }

                    characters.Add(new Character { Id = id, Name = name });
                }
            }

            var test = ByteUtility.ByteArrayToHexString(data);
            this.Characters = characters.ToArray();
        }

        public Character[] Characters { get; set; }
    }
}
