using Microsoft.Extensions.Configuration;

namespace ThreeL.Blob.Infra.Core.Extensions.Microsoft
{
    public static class ConfigurationExtension
    {
        public static IConfigurationSection GetMysqlSection(this IConfiguration configuration) 
        {
            return configuration.GetSection("Mysql");
        }
    }
}
