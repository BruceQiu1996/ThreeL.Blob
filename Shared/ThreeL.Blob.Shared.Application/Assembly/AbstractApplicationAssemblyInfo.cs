using ThreeL.Blob.Shared.Application.Contract;

namespace ThreeL.Blob.Shared.Application.Assembly
{
    public abstract class AbstractApplicationAssemblyInfo : IApplicationAssemblyInfo
    {
        public virtual System.Reflection.Assembly ImplementAssembly { get => GetType().Assembly; }
        public virtual System.Reflection.Assembly ContractAssembly { get => System.Reflection.Assembly.Load(GetType().Assembly.FullName!.Replace("Application", "Application.Contract")); }
        public virtual System.Reflection.Assembly DomainAssembly { get => System.Reflection.Assembly.Load(GetType().Assembly.FullName!.Replace("Application", "Domain")); }
        public virtual System.Reflection.Assembly MigrationsAssembly { get => System.Reflection.Assembly.Load(GetType().Assembly.FullName!.Replace("Application", "Migrations")); }
    }
}
