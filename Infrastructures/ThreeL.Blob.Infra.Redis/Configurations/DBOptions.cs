namespace ThreeL.Blob.Infra.Redis.Configurations
{
    public class DBOptions
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsSsl { get; set; } = false;
        public string SslHost { get; set; } = string.Empty;
        public int ConnectionTimeout { get; set; } = 5000;
        public IList<ServerEndPoint> Endpoints { get; } = new List<ServerEndPoint>();
        public bool AllowAdmin { get; set; } = false;
        public string ConnectionString { get; set; } = string.Empty;
        public bool AbortOnConnectFail { get; set; } = false;
        public int Database { get; set; } = 0;
        public int SyncTimeout { get; set; }
        public int ConnectTimeout { get; set; }
        public int ConnectRetry { get; set; }
        public int ConnectRetryTimeout { get; set; }
        public int ResponseTimeout { get; set; }
        public int KeepAlive { get; set; }
        public int RetryTimeout { get; set; }
    }
}
