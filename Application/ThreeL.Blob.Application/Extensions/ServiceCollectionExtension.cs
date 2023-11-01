using Autofac;
using Autofac.Extras.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using ThreeL.Blob.Application.Channels;
using ThreeL.Blob.Application.Contract.Configurations;
using ThreeL.Blob.Infra.Core.Extensions.Microsoft;
using ThreeL.Blob.Shared.Application.Contract;
using ThreeL.Blob.Shared.Application.Contract.Interceptors;
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
            services.AddSingleton<GenerateThumbnailChannel>();
            services.Configure<JwtOptions>(services.GetConfiguration().GetSection("Jwt"));
            services.Configure<SystemOptions>(services.GetConfiguration().GetSection("System"));
            ApplicationDependencyRegistrar applicationDependencyRegistrar = new ApplicationDependencyRegistrar(_applicationAssemblyInfo, services);
            applicationDependencyRegistrar.AddBlobInfraService();
        }

        public static void AddApplicationContainer(this ContainerBuilder container)
        {
            container.RegisterType<UowAsyncInterceptor>();
            container.RegisterGeneric(typeof(AsyncInterceptorAdaper<>));
            container.RegisterAssemblyTypes(_applicationAssemblyInfo.ImplementAssembly)
                .Where(t => typeof(IAppService).IsAssignableFrom(t)).AsImplementedInterfaces().InstancePerLifetimeScope()
                .EnableInterfaceInterceptors().InterceptedBy(typeof(AsyncInterceptorAdaper<UowAsyncInterceptor>));

            container.RegisterAssemblyTypes(_applicationAssemblyInfo.DomainAssembly)
                    .Where(t => typeof(IDomainService).IsAssignableFrom(t)).AsImplementedInterfaces().InstancePerLifetimeScope()
                    .EnableInterfaceInterceptors().InterceptedBy(typeof(AsyncInterceptorAdaper<UowAsyncInterceptor>)); ;
        }
    }
}
