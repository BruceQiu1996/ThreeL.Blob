using System.Linq.Expressions;
using ThreeL.Blob.Infra.Repository.Entities.EfEnities;

namespace ThreeL.Blob.Infra.Repository.IRepositories
{
    public interface IEfBasicRepository<TEntity, TKey> : IEfBaseRepository<TEntity, TKey> where TEntity : EfEntity<TKey>, IEfEntity<TKey>
    {
        Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        Task<int> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<int> RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        Task<TEntity?> GetAsync(TKey keyValue, Expression<Func<TEntity, dynamic>>? navigationPropertyPath = null, bool writeDb = false, CancellationToken cancellationToken = default);

        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, bool writeDb = false, CancellationToken cancellationToken = default);
    }
}
