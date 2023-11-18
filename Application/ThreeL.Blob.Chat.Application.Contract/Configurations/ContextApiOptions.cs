namespace ThreeL.Blob.Chat.Application.Contract.Configurations
{
    public class ContextApiOptions
    {
        public string Host { get; set; }
        public int Timeout { get; set; }
        public int RetryTimes { get; set; }
        public ushort Port { get; set; }
    }
}
