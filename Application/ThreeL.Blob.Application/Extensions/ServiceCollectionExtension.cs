using Autofac;
using Microsoft.Extensions.DependencyInjection;
using ThreeL.Blob.Shared.Application.Contract;
using ThreeL.Blob.Shared.Application.Contract.Services;
using ThreeL.Blob.Shared.Application.Register;
using ThreeL.Blob.Shared.Domain;

namespace ThreeL.Blob.Application.Extensions
{
    public static class ServiceCollectionExtension
    {
        private readonly static IApplicationAssemblyInfo _applicationAssemblyInfo;
        static ServiceCollectionExtension() 
        {
            _applicationAssemblyInfo = new AppAssemblyInfo();
        }

        public static void AddApplicationService(this IServiceCollection services) 
        {
            ApplicationDependencyRegistrar applicationDependencyRegistrar = new ApplicationDependencyRegistrar(_applicationAssemblyInfo, services);
            applicationDependencyRegistrar.AddBlobInfraService();
        }

        public static void AddApplicationContainer(this ContainerBuilder container)
        {
            container.RegisterAssemblyTypes(_applicationAssemblyInfo.ImplementAssembly)
                .Where(t => typeof(IAppService).IsAssignableFrom(t)).AsImplementedInterfaces().InstancePerLifetimeScope();

            container.RegisterAssemblyTypes(_applicationAssemblyInfo.DomainAssembly)
                    .Where(t => typeof(IDomainService).IsAssignableFrom(t)).AsImplementedInterfaces().SingleInstance();
        }
    }
}
