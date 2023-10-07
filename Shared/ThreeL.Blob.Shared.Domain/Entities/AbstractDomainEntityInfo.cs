using System.Reflection;
using ThreeL.Blob.Infra.Repository.EfCore;

namespace ThreeL.Blob.Shared.Domain.Entities
{
    public abstract class AbstractDomainEntityInfo : EfEntityInfo
    {
        protected AbstractDomainEntityInfo() : base()
        {
        }

        protected override IEnumerable<Type> GetEntityTypes(Assembly assembly)
        {
            var typeList = assembly.GetTypes().Where(m =>
                                                       m.FullName != null
                                                       && (typeof(AggregateRoot<>).IsAssignableFrom(m) || typeof(DomainEntity<>).IsAssignableFrom(m))
                                                       && !m.IsAbstract);
            if (typeList is null)
                typeList = new List<Type>();

            return typeList;
        }
    }
}
