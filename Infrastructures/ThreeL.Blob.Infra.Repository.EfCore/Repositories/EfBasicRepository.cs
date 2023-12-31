﻿using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ThreeL.Blob.Infra.Repository.Entities.EfEnities;
using ThreeL.Blob.Infra.Repository.IRepositories;

namespace ThreeL.Blob.Infra.Repository.EfCore.Repositories
{
    public class EfBasicRepository<TEntity, TKey> : AbstractEfBaseRepository<DbContext, TEntity, TKey>, IEfBasicRepository<TEntity, TKey>
            where TEntity : EfEntity<TKey>, IEfEntity<TKey>
    {
        public EfBasicRepository(DbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task<IEnumerable<TEntity>> AllAsync()
        {
            return await DbContext.Set<TEntity>().ToListAsync();
        }

        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, bool writeDb = false, bool ignoreFilters = false, CancellationToken cancellationToken = default)
        {
            if (ignoreFilters)
            {
                return await GetDbSet(writeDb, false).IgnoreQueryFilters().FirstOrDefaultAsync(filter);
            }
            else
            {
                return await GetDbSet(writeDb, false).FirstOrDefaultAsync(filter);
            }
        }

        public async Task<TEntity?> GetAsync(TKey keyValue, Expression<Func<TEntity, dynamic>>? navigationPropertyPath = null, bool writeDb = false, bool ignoreFilters = false, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query;
            if (ignoreFilters)
            {
                query = GetDbSet(writeDb, false).IgnoreQueryFilters().Where(t => t.Id.Equals(keyValue));
            }
            else
            {
                query = GetDbSet(writeDb, false).Where(t => t.Id.Equals(keyValue));
            }

            if (navigationPropertyPath is null)
                return await query.FirstOrDefaultAsync(cancellationToken);
            else
                return await query.Include(navigationPropertyPath).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> QuerySqlAsync(string sql, bool writeDb = false, CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<TEntity>().FromSqlRaw(sql).ToListAsync();
        }

        public async Task<int> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            this.DbContext.Remove(entity);
            return await this.DbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            this.DbContext.RemoveRange(entities);
            return await this.DbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            this.DbContext.UpdateRange(entities);
            return await this.DbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
