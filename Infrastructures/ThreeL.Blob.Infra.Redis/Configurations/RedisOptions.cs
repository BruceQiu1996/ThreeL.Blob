namespace ThreeL.Blob.Infra.Redis.Configurations
{
    public class RedisOptions
    {
        public bool EnableBloomFilter { get; set; }
        public DBOptions Dbconfig { get; set; } = default!;
    }
}
