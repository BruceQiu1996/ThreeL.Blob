﻿using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Blob.Clients.Win.Configurations;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Pages;
using ThreeL.Blob.Clients.Win.Profiles;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels;
using ThreeL.Blob.Clients.Win.ViewModels.Item;
using ThreeL.Blob.Clients.Win.ViewModels.Page;
using ThreeL.Blob.Clients.Win.ViewModels.Window;
using ThreeL.Blob.Clients.Win.Windows;

namespace ThreeL.Blob.Clients.Win
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static IServiceProvider? ServiceProvider;
        internal static IHost host;
        internal static Entities.UserProfile UserProfile;
        internal static HubConnection HubConnection;
        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            var builder = Host.CreateDefaultBuilder(e.Args);
            builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.ConfigureServices((context, service) =>
            {
                Configure(service, context.Configuration);
                service.AddSingleton<MainWindow>();
                service.AddSingleton<MainWindowViewModel>();
                service.AddSingleton<LoginWindow>();
                service.AddSingleton<LoginWindowViewModel>();
                service.AddTransient<DownloadEnsure>();
                service.AddTransient<DownloadEnsureViewModel>();
                service.AddTransient<ZipFileObjectsEnsure>();
                service.AddTransient<ZipFileObjectsEnsureViewModel>();
                service.AddTransient<Move>();
                service.AddTransient<MoveViewModel>();
                service.AddSingleton<Chat>();
                service.AddSingleton<ChatViewModel>();

                service.AddSingleton<MainPage>();
                service.AddSingleton<MainPageViewModel>();
                service.AddSingleton<TransferPage>();
                service.AddSingleton<TransferPageViewModel>();
                service.AddSingleton<SettingsPage>();
                service.AddSingleton<SettingsPageViewModel>();
                service.AddSingleton<Share>();
                service.AddSingleton<ShareViewModel>();

                service.AddSingleton<UploadingPage>();
                service.AddSingleton<UploadingPageViewModel>();
                service.AddTransient<UploadItemViewModel>();
                service.AddSingleton<DownloadingPage>();
                service.AddSingleton<DownloadingPageViewModel>();
                service.AddTransient<DownloadItemViewModel>();
                service.AddSingleton<TransferComplete>();
                service.AddSingleton<TransferCompletePageViewModel>();

                service.AddSingleton<ApiHttpRequest>();
                service.AddSingleton<ChatHttpRequest>();
                service.AddSingleton<GrpcService>();
                service.AddSingleton<GrowlHelper>();
                service.AddSingleton<FileHelper>();
                service.AddSingleton<SystemHelper>();
                service.AddSingleton<DateTimeHelper>();
                service.AddSingleton<DatabaseHelper>();
                service.AddSingleton<IniHelper>();
                service.AddSingleton<EncryptHelper>();
                service.AddSingleton<IniSettings>();

                var connString = $"Data Source = {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db.db")}";
                service.AddDbContextFactory<MyDbContext>(option =>
                {
                    option.UseSqlite(connString, options => 
                    {
                        options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        options.MaxBatchSize(100);
                        options.CommandTimeout(10000);
                    });
                },ServiceLifetime.Singleton);
            }).UseSerilog();

            builder.ConfigureHostConfiguration(options =>
            {
                options.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            });

            host = builder.Build();
            ServiceProvider = host.Services;
            await host.StartAsync();
            await ServiceProvider.GetRequiredService<IniSettings>().InitializeAsync();
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            host.Services.GetRequiredService<LoginWindow>().Show();
        }

        public void Configure(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RemoteOptions>(configuration.GetSection("RemoteOptions"));

            AutoMapper.IConfigurationProvider config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<Profiles.UserProfile>();
                cfg.AddProfile<TransferProfile>();
                cfg.AddProfile<FileObjProfile>();
                cfg.AddProfile<RelationProfile>();
            });

            services.AddSingleton(config);
            services.AddScoped<IMapper, Mapper>();
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            ServiceProvider?.GetRequiredService<GrowlHelper>().Warning(e.ToString());
            //ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(Module.CLIENT_WIN_TASK_EXCEPTION))
            //     .LogError(e.ToString());
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ServiceProvider?.GetRequiredService<GrowlHelper>().Warning(e.ToString());
            //ServiceProvider.GetRequiredService<GrowlHelper>().Warning("未知错误");
            //ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(Module.CLIENT_WIN_THREAD_EXCEPTION))
            //    .LogError(e.ToString());
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ServiceProvider?.GetRequiredService<GrowlHelper>().Warning(e.ToString());
            //ServiceProvider.GetRequiredService<GrowlHelper>().Warning("未知错误");
            //ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(Module.CLIENT_WIN_UI_EXCEPTION))
            //    .LogError(e.ToString());
        }

        public async static Task CloseAsync()
        {
            await host.StopAsync();
            host.Dispose();
            Environment.Exit(0);
        }
    }
}
