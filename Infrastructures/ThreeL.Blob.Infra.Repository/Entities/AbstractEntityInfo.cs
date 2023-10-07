using System.Reflection;
using ThreeL.Blob.Infra.Repository.Entities.EfEnities;

namespace ThreeL.Blob.Infra.Repository.Entities
{
    public abstract class AbstractEntityInfo : IEntityInfo
    {
        protected virtual IEnumerable<Type> GetEntityTypes(Assembly assembly)
        {
            var typeList = assembly.GetTypes().Where(m =>
                                                       m.FullName != null
                                                       && typeof(EfEntity<>).IsAssignableFrom(m)
                                                       && !m.IsAbstract);
            if (typeList is null)
                typeList = new List<Type>();

            return typeList;
        }

        public abstract void OnModelCreating(dynamic modelBuilder);
    }
}
