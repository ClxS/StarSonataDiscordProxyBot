namespace StarSonataApi.Messages.Outgoing
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using global::StarSonataApi.Objects;

    internal class TextMessageOut : IOutgoingMessage
    {
        public TextMessageOut(ChatMessage message)
        {
            this.Message = message;
        }

        public ChatMessage Message { get; }

        public byte[] GetOutData()
        {
            var bytes = new List<byte> { MessageConstants.CS_textmessage, this.GetMessageTypeByte() };
            bytes.AddRange(Encoding.ASCII.GetBytes(this.GetMessageText()));
            bytes.Add(0);

            if (this.Message.Channel == MessageChannel.Private)
            {
                bytes.AddRange(Encoding.ASCII.GetBytes(this.Message.Username));
                bytes.Add(0);
            }

            bytes.InsertRange(0, BitConverter.GetBytes((short)(bytes.Count - 1)));
            return bytes.ToArray();
        }

        private string GetMessageText()
        {
            // Do we need to do the >> thing?
            return this.Message.Message;
        }

        private byte GetMessageTypeByte()
        {
            switch (this.Message.Channel)
            {
                case MessageChannel.Team:
                    return MessageConstants.USER_TALK_TEAM;
                case MessageChannel.Galaxy:
                    return MessageConstants.USER_TALK_GALAXY;
                case MessageChannel.All:
                    return MessageConstants.USER_TALK_GLOBAL;
                case MessageChannel.Trade:
                    return MessageConstants.USER_TALK_TRADE;
                case MessageChannel.Squad:
                    return MessageConstants.USER_TALK_GROUP;
                case MessageChannel.Moderator:
                    return MessageConstants.USER_TALK_MODERATOR;
                case MessageChannel.Help:
                    return MessageConstants.USER_TALK_HELP;
                case MessageChannel.Chat:
                    return MessageConstants.USER_TALK_CHATCLIENTONLY;
                case MessageChannel.Event:
                    return MessageConstants.USER_TALK_CHATCLIENTONLY;
                case MessageChannel.Private:
                    return MessageConstants.USER_TALK_INDIVIDUAL;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
