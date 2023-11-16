namespace ThreeL.Blob.Clients.Win.Configurations
{
    public class  RemoteOptions
    {
        public string Host { get; set; }
        public int APIPort { get; set; }
        public int GrpcPort { get; set; }
        public int ChatPort { get; set; }
        public int MaxRetryAttempts { get; set; }
    }
}
