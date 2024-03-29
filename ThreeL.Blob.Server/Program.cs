﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Diagnostics;
using System.Net;
using ThreeL.Blob.Application.Extensions;
using ThreeL.Blob.Application.Middlewares;
using ThreeL.Blob.Infra.Redis;
using ThreeL.Blob.Server.Controllers;
using ThreeL.Blob.Shared.Application.Contract.Extensions;
using ThreeL.Blob.Shared.Application.Contract.Helpers;
using ThreeL.Blob.Shared.Application.Contract.Services;

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
                services.AddGrpc(options =>
                {
                    options.MaxReceiveMessageSize = 1024 * 1024 * 10;//最大10M
                });
                services.AddApplicationService();
                //配置跨域
                services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy", builder =>
                    {
                        builder.AllowAnyOrigin() //允许所有Origin策略

                               //允许所有请求方法：Get,Post,Put,Delete
                               .AllowAnyMethod()

                               //允许所有请求头:application/json
                               .AllowAnyHeader();
                    });
                });
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
                builder.Services.AddAuthentication(options =>
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
            }).UseSerilog((context, logger) =>
            {
                logger.WriteTo.Console();
            });

            builder.WebHost.UseKestrel((context, options) =>
            {
                options.Listen(IPAddress.Any, context.Configuration.GetSection("Ports:API")!.Get<int>(), listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                });

                options.Listen(IPAddress.Any, context.Configuration.GetSection("Ports:Grpc")!.Get<int>(), listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http2;
                });
            });

            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            //middleware
            host = builder.Build();
            await host.PreheatService();
            host.UseRouting();
            host.UseCors("CorsPolicy");
            host.UseAuthentication();
            host.UseAuthorization();
            host.UseMiddleware<AuthorizeThumbnailImagesMiddleware>("/api/thumbnailImages"); //授权静态文件访问,如果使用，则表情获取那需要自己控制下载
            host.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Add("Cache-Control", "public,max-age=600");
                }
            });

            if (!Directory.Exists(host.Configuration.GetSection("FileStorage:ThumbnailImagesLocation").Value!))
            {
                Directory.CreateDirectory(host.Configuration.GetSection("FileStorage:ThumbnailImagesLocation").Value!);
            }

            ///缩略图
            host.UseFileServer(new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(host.Configuration.GetSection("FileStorage:ThumbnailImagesLocation").Value!),
                RequestPath = new Microsoft.AspNetCore.Http.PathString("/api/thumbnailImages"),
                EnableDirectoryBrowsing = false
            });

            if (!Directory.Exists(host.Configuration.GetSection("FileStorage:AvatarImagesLocation").Value!))
            {
                Directory.CreateDirectory(host.Configuration.GetSection("FileStorage:AvatarImagesLocation").Value!);
            }

            //头像
            host.UseFileServer(new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(host.Configuration.GetSection("FileStorage:AvatarImagesLocation").Value!),
                RequestPath = new Microsoft.AspNetCore.Http.PathString("/api/avatars"),
                EnableDirectoryBrowsing = false
            });

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
            host.MapGrpcService<GrpcController>();
            host.MapGrpcService<ChatGrpcController>();

            await host.RunAsync();
        }
    }
}