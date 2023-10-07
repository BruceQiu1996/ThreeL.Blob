using ThreeL.Blob.Infra.Repository.Entities.EfEnities;

namespace ThreeL.Blob.Shared.Domain.Entities
{
    public abstract class AggregateRoot<TKey> : DomainEntity<TKey>, IEfEntity<TKey>
    {

    }
}
