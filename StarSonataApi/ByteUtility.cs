namespace StarSonataApi
{
    using System;
    using System.Linq;
    using System.Text;

    public static class ByteUtility
    {
        public static string ByteArrayToAsciiString(byte[] data)
        {
            return Encoding.ASCII.GetString(data.Where(b => b != 0).ToArray());
        }

        public static string ByteArrayToHexString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }

        public static bool GetBoolean(byte[] data, ref int offset)
        {
            var output = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            return output;
        }

        public static byte GetByte(byte[] data, ref int offset)
        {
            offset++;
            return data[offset - 1];
        }

        public static int GetInt(byte[] data, ref int offset)
        {
            var output = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            return output;
        }

        public static long GetLong(byte[] data, ref int offset)
        {
            var output = BitConverter.ToInt64(data, offset);
            offset += sizeof(long);
            return output;
        }

        public static string GetString(byte[] data, ref int offset)
        {
            var baseOffset = offset;
            while (offset < data.Length && data[offset] != 0)
            {
                offset++;
            }

            offset++;
            return Encoding.ASCII.GetString(data, baseOffset, offset - baseOffset - 1);
        }
    }
}
