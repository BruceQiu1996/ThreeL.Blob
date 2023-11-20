namespace ThreeL.Blob.Application.Contract.Configurations
{
    public class ChatServerGrpcOptions
    {
        public string Host { get; set; }
        public int Timeout { get; set; }
        public int RetryTimes { get; set; }
        public ushort Port { get; set; }
    }
}
