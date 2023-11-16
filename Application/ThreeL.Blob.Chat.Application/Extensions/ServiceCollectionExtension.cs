using Autofac;
using Microsoft.Extensions.DependencyInjection;
using ThreeL.Blob.Infra.Core.Extensions.Microsoft;
using ThreeL.Blob.Shared.Application.Contract;
using ThreeL.Blob.Shared.Application.Contract.Configurations;
using ThreeL.Blob.Shared.Application.Register;

namespace ThreeL.Blob.Chat.Application.Extensions
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
            services.Configure<JwtOptions>(services.GetConfiguration().GetSection("Jwt"));
            services.Configure<SystemOptions>(services.GetConfiguration().GetSection("System"));
            ApplicationDependencyRegistrar applicationDependencyRegistrar = new ApplicationDependencyRegistrar(_applicationAssemblyInfo, services);
            applicationDependencyRegistrar.AddChatInfraService();
        }

        public static void AddApplicationContainer(this ContainerBuilder container)
        {

        }
    }
}
