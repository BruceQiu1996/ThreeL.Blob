namespace ThreeL.Blob.Infra.Repository.Entities.EfEnities
{
    public abstract class EfEntity<TEntity> : IEfEntity<TEntity>
    {
        public TEntity Id { get; set; }
    }
}
