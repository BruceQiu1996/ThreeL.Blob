using System.Reflection;

namespace ThreeL.Blob.Shared.Application.Contract
{
    public interface IApplicationAssemblyInfo
    {
        Assembly DomainAssembly { get; }
        Assembly ImplementAssembly { get; }
        Assembly ContractAssembly { get; }
        Assembly MigrationsAssembly { get; }
    }
}
