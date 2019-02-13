namespace StarSonataApi.Messages.Incoming
{
    using global::StarSonataApi.Objects;

    public class TextMessage : IIncomingMessage
    {
        public TextMessage(byte[] data)
        {
            var offset = 0;
            var type = ByteUtility.GetByte(data, ref offset);
            var message = ByteUtility.GetString(data, ref offset);
            var username = string.Empty;
            if (offset < data.Length)
            {
                ByteUtility.GetByte(data, ref offset);
                username = ByteUtility.GetString(data, ref offset);
            }

            this.Message = new ChatMessage(type, message, username);
        }

        public ChatMessage Message { get; set; }
    }
}
