using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using ThreeL.Blob.Application.Extensions;

namespace ThreeL.Blob.Server
{
    internal class Program
    {
        async static Task Main(string[] args)
        {
            WebApplication host = null;
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureContainer<ContainerBuilder>((hcontext, builder) =>
            {
                builder.AddApplicationContainer();
            });

            builder.Host.ConfigureServices((hostContext, services) =>
            {
                services.AddApplicationService();
                services.AddControllers();
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Version = "v1",
                        Title = "Three_Blob v1",
                        Description = "Three_Blob v1版本接口"
                    });

                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                    {
                        Description = "在下框中输入请求头中需要添加Jwt授权Token：Bearer Token",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        BearerFormat = "JWT",
                        Scheme = "Bearer"
                    });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme{Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme,Id = "Bearer"}},new string[] { }
                        }
                    });
                });
                //添加认证
                //builder.Services.AddAuthentication(options =>
                //{
                //    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                //}).AddJwtBearer(options =>
                //{
                //    options.TokenValidationParameters = new TokenValidationParameters()
                //    {
                //        ValidateIssuer = true,
                //        ValidIssuer = hostContext.Configuration["Jwt:Issuer"],
                //        ValidateAudience = true,
                //        ValidAudiences = hostContext.Configuration.GetSection("Jwt:Audiences").Get<string[]>(),
                //        ValidateIssuerSigningKey = true,
                //        ValidateLifetime = true,
                //        ClockSkew = TimeSpan.FromSeconds(int.Parse(hostContext.Configuration["Jwt:ClockSkew"]!)), //过期时间容错值，解决服务器端时间不同步问题（秒）
                //        RequireExpirationTime = true,
                //        IssuerSigningKeyResolver = host!.Services.GetRequiredService<IJwtService>().ValidateIssuerSigningKey
                //    };
                //});
            }).UseSerilog((context, logger) =>
            {
                logger.WriteTo.Console();
            });

            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            //middleware
            host = builder.Build();
            host.UseRouting();
            //host.UseAuthentication();
            //host.UseAuthorization();
            //host.UseMiddleware<AuthorizeStaticFilesMiddleware>("/files"); //授权静态文件访问,如果使用，则表情获取那需要自己控制下载
            host.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Add("Cache-Control", "public,max-age=600");
                }
            });

            host.UseSwagger();
            host.UseSwaggerUI(option =>
            {
                option.SwaggerEndpoint($"/swagger/v1/swagger.json", "v1");
            });
            host.MapControllers();

            await host.RunAsync();
        }
    }
}