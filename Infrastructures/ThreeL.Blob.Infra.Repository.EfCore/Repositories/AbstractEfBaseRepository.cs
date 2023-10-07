using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ThreeL.Blob.Infra.Repository.Entities.EfEnities;
using ThreeL.Blob.Infra.Repository.IRepositories;

namespace ThreeL.Blob.Infra.Repository.EfCore.Repositories
{
    public abstract class AbstractEfBaseRepository<TDbContext, TEntity, TKey> : IEfBaseRepository<TEntity, TKey>
       where TDbContext : DbContext
       where TEntity : EfEntity<TKey>, IEfEntity<TKey>
    {
        protected virtual TDbContext DbContext { get; }

        protected AbstractEfBaseRepository(TDbContext dbContext) => DbContext = dbContext;

        protected virtual IQueryable<TEntity> GetDbSet(bool writeDb, bool noTracking)
        {
            if (noTracking && writeDb)
                return DbContext.Set<TEntity>().AsNoTracking().TagWith(RepositoryConsts.MAXSCALE_ROUTE_TO_MASTER);
            else if (noTracking)
                return DbContext.Set<TEntity>().AsNoTracking();
            else if (writeDb)
                return DbContext.Set<TEntity>().TagWith(RepositoryConsts.MAXSCALE_ROUTE_TO_MASTER);
            else
                return DbContext.Set<TEntity>();
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> whereExpression, bool writeDb = false, CancellationToken cancellationToken = default)
        {
            var dbSet = DbContext.Set<TEntity>().AsNoTracking();
            if (writeDb)
                dbSet = dbSet.TagWith(RepositoryConsts.MAXSCALE_ROUTE_TO_MASTER);
            return await dbSet.AnyAsync(whereExpression, cancellationToken);
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression, bool writeDb = false, CancellationToken cancellationToken = default)
        {
            var dbSet = DbContext.Set<TEntity>().AsNoTracking();
            if (writeDb)
                dbSet = dbSet.TagWith(RepositoryConsts.MAXSCALE_ROUTE_TO_MASTER);
            return await dbSet.CountAsync(whereExpression, cancellationToken);
        }

        public virtual async Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await DbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
            return await DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<int> InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            await DbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
            return await DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var entry = DbContext.Entry(entity);

            //如果实体没有被跟踪，必须指定需要更新的列
            if (entry.State == EntityState.Detached)
                throw new ArgumentException($"实体没有被跟踪，需要指定更新的列");

            if (entry.State == EntityState.Added || entry.State == EntityState.Deleted)
                throw new ArgumentException($"{nameof(entity)},实体状态为{nameof(entry.State)}");

            return await DbContext.SaveChangesAsync(cancellationToken);
        }

        public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression, bool writeDb = false, bool noTracking = true)
        {
            return GetDbSet(writeDb, noTracking).Where(expression);
        }
    }
}
