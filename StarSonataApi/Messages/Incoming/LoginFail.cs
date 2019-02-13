namespace StarSonataApi.Messages.Incoming
{
    public class LoginFail : IIncomingMessage
    {
        public LoginFail(byte[] data)
        {
            var offset = 0;
            this.FailureReason = ByteUtility.GetString(data, ref offset);
        }

        public string FailureReason { get; set; }
    }
}
