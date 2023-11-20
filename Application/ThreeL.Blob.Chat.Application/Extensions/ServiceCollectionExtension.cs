using Autofac;
using Microsoft.Extensions.DependencyInjection;
using ThreeL.Blob.Chat.Application.Contract.Configurations;
using ThreeL.Blob.Infra.Core.Extensions.Microsoft;
using ThreeL.Blob.Shared.Application.Contract;
using ThreeL.Blob.Shared.Application.Contract.Configurations;
using ThreeL.Blob.Shared.Application.Contract.Services;
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
            services.Configure<ContextApiOptions>(services.GetConfiguration().GetSection("ContextApiOptions"));
            ApplicationDependencyRegistrar applicationDependencyRegistrar = new ApplicationDependencyRegistrar(_applicationAssemblyInfo, services);
            applicationDependencyRegistrar.AddChatInfraService();
        }

        public static void AddApplicationContainer(this ContainerBuilder container)
        {
            container.RegisterAssemblyTypes(_applicationAssemblyInfo.ImplementAssembly)
                .Where(t => typeof(IAppService).IsAssignableFrom(t)).AsImplementedInterfaces().InstancePerLifetimeScope();
        }
    }
}
