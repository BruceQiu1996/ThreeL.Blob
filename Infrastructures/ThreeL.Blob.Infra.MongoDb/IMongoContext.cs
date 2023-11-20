using MongoDB.Driver;
using ThreeL.Blob.Infra.Repository.Entities.Mongo;

namespace ThreeL.Blob.Infra.MongoDb
{
    public interface IMongoContext
    {
        Task<IMongoCollection<TEntity>> GetCollectionAsync<TEntity>(CancellationToken cancellationToken = default)
            where TEntity : MongoEntity;
    }
}
