namespace ThreeL.Blob.Chat.Application.Contract.Configurations
{
    public class ContextApiGrpcOptions
    {
        public string Host { get; set; }
        public int Timeout { get; set; }
        public int RetryTimes { get; set; }
        public ushort Port { get; set; }
    }
}
