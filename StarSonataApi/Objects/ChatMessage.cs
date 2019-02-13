namespace StarSonataApi.Objects
{
    public class ChatMessage
    {
        public ChatMessage()
        {
        }

        public ChatMessage(int msgType, string message, string username)
        {
            if (message[0] == 65467 || message[0] == '»')
            {
                this.Message = message.Substring(1);
                this.IsExternalChatMessage = true;
            }
            else
            {
                this.Message = message;
                this.IsExternalChatMessage = false;
            }

            switch (msgType)
            {
                case MessageConstants.MSG_USER_TEAM:
                    this.Channel = MessageChannel.Team;
                    break;
                case MessageConstants.MSG_USER_GALAXY:
                    this.Channel = MessageChannel.Galaxy;
                    break;
                case MessageConstants.MSG_USER_GLOBAL:
                    this.Channel = MessageChannel.All;
                    break;
                case MessageConstants.MSG_USER_TRADE:
                    this.Channel = MessageChannel.Trade;
                    break;
                case MessageConstants.MSG_USER_GROUP:
                    this.Channel = MessageChannel.Squad;
                    break;
                case MessageConstants.MSG_USER_MODERATOR:
                    this.Channel = MessageChannel.Moderator;
                    break;
                case MessageConstants.MSG_USER_HELP:
                    this.Channel = MessageChannel.Help;
                    break;
                case MessageConstants.MSG_USER_CHATCLIENTONLY:
                    this.Channel = MessageChannel.Chat;
                    break;
                case MessageConstants.MSG_GLOBAL_LOGIN_MSG:
                    this.Channel = MessageChannel.Event;
                    break;
                case MessageConstants.MSG_ERROR:
                    this.Channel = MessageChannel.Event;
                    break;
                case MessageConstants.MSG_USER_USER:
                    this.Channel = MessageChannel.Private;
                    break;
                default:
                    this.Channel = MessageChannel.Event;
                    break;
            }

            this.Username = username;

            if (this.Message.StartsWith("/me "))
            {
                this.Message = this.Message.Substring(3);
                this.IsMeMessage = true;
            }
        }

        public MessageChannel Channel { get; set; }

        public bool IsExternalChatMessage { get; set; }

        public bool IsMeMessage { get; set; }

        public string Message { get; set; }

        public string Username { get; set; }
    }
}
