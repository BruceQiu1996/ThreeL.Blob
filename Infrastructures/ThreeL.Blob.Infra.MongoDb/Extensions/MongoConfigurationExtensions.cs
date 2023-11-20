using ThreeL.Blob.Infra.MongoDb.Configuration;

namespace ThreeL.Blob.Infra.MongoDb.Extensions
{
    public static class MongoConfigurationExtensions
    {
        public static string GetCollectionName<TEntity>(this MongoOptions options)
        {
            var collectionName = typeof(TEntity).Name;
            if (options.PluralizeCollectionNames)
            {
                collectionName = Pluralize(collectionName);
            }

            return collectionName;
        }

        private static string Pluralize(string name)
        {
            if (name.EndsWith("y"))
            {
                return name.Substring(0, name.Length - 1) + "ies";
            }
            else if (name.EndsWith("s"))
            {
                return name + "es";
            }
            else
            {
                return name + "s";
            }
        }
    }
}
