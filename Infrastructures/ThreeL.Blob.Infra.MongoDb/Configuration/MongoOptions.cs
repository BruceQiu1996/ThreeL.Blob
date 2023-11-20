namespace ThreeL.Blob.Infra.MongoDb.Configuration
{
    public class MongoOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public bool PluralizeCollectionNames { get; set; } = true;
    }
}
