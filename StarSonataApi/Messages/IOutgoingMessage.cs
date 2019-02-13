namespace StarSonataApi.Messages
{
    internal interface IOutgoingMessage
    {
        byte[] GetOutData();
    }
}
