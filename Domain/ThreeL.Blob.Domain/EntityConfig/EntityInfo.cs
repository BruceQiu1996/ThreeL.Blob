using System.Reflection;
using ThreeL.Blob.Shared.Domain.Entities;

namespace ThreeL.Blob.Domain.EntityConfig
{
    public class EntityInfo : AbstractDomainEntityInfo
    {
        public EntityInfo() : base()
        {
        }

        protected override Assembly GetCurrentAssembly() => GetType().Assembly;
    }
}
