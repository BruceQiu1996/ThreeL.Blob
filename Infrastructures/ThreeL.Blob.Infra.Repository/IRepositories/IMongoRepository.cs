using MongoDB.Driver;
using System.Linq.Expressions;
using ThreeL.Blob.Infra.Repository.Entities.Mongo;
using ThreeL.Blob.Infra.Repository.IRepositories.Models;

namespace ThreeL.Blob.Infra.Repository.IRepositories
{
    public interface IMongoRepository<TEntity> : IRepository<TEntity> where TEntity : MongoEntity
    {
        Task<TEntity> GetAsync(string id, CancellationToken cancellationToken = default);
        Task<TEntity> GetAsync(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default);
        Task<ICollection<TEntity>> GetAllAsync(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default);
        Task<ICollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task AddManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<long> DeleteManyAsync(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default);
        Task<TEntity> ReplaceAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<PagedModel<TEntity>> PagedAsync(int pageNumber, int pageSize, FilterDefinition<TEntity> filter, Expression<Func<TEntity, object>> orderByExpression, bool ascending = false, CancellationToken cancellationToken = default);
    }
}
