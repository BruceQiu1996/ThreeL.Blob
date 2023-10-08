using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ThreeL.Blob.Infra.Core.Extensions.Microsoft
{
    public static class ServiceCollectionExtension
    {
        public static IConfiguration GetConfiguration(this IServiceCollection services)
        {
            var hostBuilderContext = (HostBuilderContext)services.FirstOrDefault(d => d.ServiceType == typeof(HostBuilderContext))!.ImplementationInstance!;
            if (hostBuilderContext?.Configuration is not null)
            {
                var instance = hostBuilderContext.Configuration as IConfigurationRoot;
                if (instance is not null)
                    return instance;
            }

            throw new InvalidOperationException("can't find server ：HostBuilderContext.Configuration");
        }
    }
}
