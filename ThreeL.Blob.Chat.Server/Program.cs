using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Net;
using ThreeL.Blob.Chat.Server.Controllers;
using ThreeL.Blob.Shared.Application.Contract.Services;
using ThreeL.Blob.Chat.Application.Extensions;

namespace ThreeL.Blob.Chat.Server
{
    internal class Program
    {
        static WebApplication host = null;
        async static Task Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            host = CreateHostBuilder(args).Build();
            host.UseRouting();
            host.MapControllers();
            host.UseAuthentication();
            host.UseAuthorization();
            host.MapHub<ChatHub>("/Chat");
            await host.RunAsync();
        }

        public static WebApplicationBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = WebApplication.CreateBuilder(args);
            hostBuilder.Host.UseSerilog((context, logger) =>//Serilog
            {
                logger.WriteTo.Console();
            });

            hostBuilder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureContainer<ContainerBuilder>((hcontext, builder) =>
            {
                builder.AddApplicationContainer();
            });

            hostBuilder.Host.ConfigureServices((hostContext, services) =>
            {
                services.AddApplicationService();
                services.AddControllers();
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen();
                services.AddSignalR(option =>
                {
                    option.MaximumReceiveMessageSize = 1024 * 1024 * 20;
                    option.StreamBufferCapacity = 10;
                });

                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = hostContext.Configuration["System:Name"],
                        ValidateAudience = true,
                        ValidAudiences = hostContext.Configuration.GetSection("Jwt:Audiences").Get<string[]>(),
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(int.Parse(hostContext.Configuration["Jwt:ClockSkew"]!)), //过期时间容错值，解决服务器端时间不同步问题（秒）
                        RequireExpirationTime = true,
                        IssuerSigningKeyResolver = host!.Services.GetRequiredService<IJwtService>().ValidateIssuerSigningKey
                    };
                });
            });

            hostBuilder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            hostBuilder.WebHost.UseKestrel((context, options) =>
            {
                options.Listen(IPAddress.Any, 5826, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                });
            });

            return hostBuilder;

        }
    }
}