using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Shared.Application.Contract.Extensions
{
    public static class HostExtensions
    {
        public static async Task PreheatService(this IHost host)
        {
            var services = host.Services.GetRequiredService<IEnumerable<IPreheatService>>();
            foreach (var service in services)
            {
                await service.PreheatAsync();
            }
        }
    }
}
