namespace ThreeL.Blob.Infra.Redis.Configurations
{
    public class ServerEndPoint
    {
        public ServerEndPoint(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public ServerEndPoint()
        {
        }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        /// <value>The host.</value>
        public string Host { get; set; } = string.Empty;
    }
}
