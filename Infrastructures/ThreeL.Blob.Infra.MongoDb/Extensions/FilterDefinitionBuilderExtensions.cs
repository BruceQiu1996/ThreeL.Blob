using MongoDB.Driver;
using ThreeL.Blob.Infra.Repository.Entities.Mongo;

namespace ThreeL.Blob.Infra.MongoDb.Extensions
{
    public static class FilterDefinitionBuilderExtensions
    {
        public static FilterDefinition<TEntity> IdEq<TEntity>(this FilterDefinitionBuilder<TEntity> filter, string id)
            where TEntity : MongoEntity
            => filter.Eq(x => x.Id, id);
    }
}
