namespace ThreeL.Blob.Clients.Win.Configurations
{
    public class  RemoteOptions
    {
        public string APIHost { get; set; }
        public ushort APIPort { get; set; }
        public ushort APIGrpcPort { get; set; }
        public string ChatHost { get; set; }
        public ushort ChatPort { get; set; }
        public int MaxRetryAttempts { get; set; }
    }
}
