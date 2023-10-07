using Microsoft.EntityFrameworkCore;
using System.Reflection;
using ThreeL.Blob.Infra.Repository.Entities;

namespace ThreeL.Blob.Infra.Repository.EfCore
{
    public abstract class EfEntityInfo : AbstractEntityInfo
    {
        public override void OnModelCreating(dynamic modelBuilder)
        {
            if (modelBuilder is not ModelBuilder builder)
                throw new ArgumentNullException(nameof(modelBuilder));

            var entityAssembly = GetCurrentAssembly();
            var entityTypes = GetEntityTypes(entityAssembly);
            foreach (var entityType in entityTypes)
            {
                builder.Entity(entityType);
            }

            builder.ApplyConfigurationsFromAssembly(entityAssembly);

            SetTableName(modelBuilder);
        }

        protected virtual void SetTableName(dynamic modelBuilder)
        {
        }

        protected abstract Assembly GetCurrentAssembly();
    }
}
