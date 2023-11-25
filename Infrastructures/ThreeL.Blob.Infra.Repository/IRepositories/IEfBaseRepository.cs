using System.Linq.Expressions;
using ThreeL.Blob.Infra.Repository.Entities.EfEnities;

namespace ThreeL.Blob.Infra.Repository.IRepositories
{
    public interface IEfBaseRepository<TEntity, Tkey> : IRepository<TEntity> where TEntity : EfEntity<Tkey>, IEfEntity<Tkey>
    {
        /// <summary>
        /// 插入单个实体
        /// </summary>
        /// <param name="entity"><see cref="TEntity"/></param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns></returns>
        Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// 批量插入实体
        /// </summary>
        /// <param name="entities"><see cref="TEntity"/></param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns></returns>
        Task<int> InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// 更新单个实体
        /// </summary>
        /// <param name="entity"><see cref="T:TEntity"/></param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns></returns>
        Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据条件查询实体是否存在
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <param name="writeDb">是否读写库，默认false,可选参数</param>
        /// param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns></returns>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> whereExpression, bool writeDb = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// 统计符合条件的实体数量
        /// </summary>
        /// <param name="writeDb">是否读写库，默认false,可选参数</param>
        /// param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns></returns>
        Task<int> CountAsync(bool writeDb = false, bool ignoreFilters = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据条件查询，返回IQueryable<TEntity>
        /// </summary>
        /// <param name="expression">查询条件</param>
        /// <param name="writeDb">是否读写库，默认false,可选参数</param>
        /// <param name="noTracking">是否开启跟踪，默认false,可选参数</param>
        /// <param name="ignoreFilters">忽略筛选</param>
        /// <returns></returns>
        IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression, bool writeDb = false, bool noTracking = true, bool ignoreFilters = false);


        /// <summary>
        /// 全部
        /// </summary>
        /// <param name="writeDb">是否读写库，默认false,可选参数</param>
        /// <param name="noTracking">是否开启跟踪，默认false,可选参数</param>
        /// <param name="ignoreFilters">是否排除筛选</param>
        /// <returns></returns>
        IQueryable<TEntity> All(bool writeDb = false, bool noTracking = true, bool ignoreFilters = false);
    }
}
