using Autofac.Extensions.DependencyInjection;
using AutoMapper;
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
using ThreeL.Blob.Clients.Win.Entities;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Pages;
using ThreeL.Blob.Clients.Win.Profiles;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.ViewModels;
using ThreeL.Blob.Clients.Win.ViewModels.Item;
using ThreeL.Blob.Clients.Win.ViewModels.Page;

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

                service.AddSingleton<MainPage>();
                service.AddSingleton<MainPageViewModel>();
                service.AddSingleton<TransferPage>();
                service.AddSingleton<TransferPageViewModel>();
                service.AddSingleton<SettingsPage>();
                service.AddSingleton<SettingsPageViewModel>();

                service.AddSingleton<UploadingPage>();
                service.AddSingleton<UploadingPageViewModel>();
                service.AddSingleton<UploadItemViewModel>();
                service.AddSingleton<DownloadingPage>();
                service.AddSingleton<DownloadingPageViewModel>();
                service.AddSingleton<TransferComplete>();
                service.AddSingleton<TransferCompletePageViewModel>();

                service.AddSingleton<HttpRequest>();
                service.AddSingleton<GrpcService>();
                service.AddSingleton<GrowlHelper>();

                var connString = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"db.db");
                service.AddDbContextFactory<MyDbContext>(option => 
                {
                    option.UseSqlite(connString);
                });
            }).UseSerilog();

            builder.ConfigureHostConfiguration(options =>
            {
                options.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            });

            host = builder.Build();
            ServiceProvider = host.Services;
            await host.StartAsync();
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
            });

            services.AddSingleton(config);
            services.AddScoped<IMapper, Mapper>();
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            //ServiceProvider.GetRequiredService<GrowlHelper>().Warning("未知错误");
            //ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(Module.CLIENT_WIN_TASK_EXCEPTION))
            //     .LogError(e.ToString());
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //ServiceProvider.GetRequiredService<GrowlHelper>().Warning("未知错误");
            //ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(Module.CLIENT_WIN_THREAD_EXCEPTION))
            //    .LogError(e.ToString());
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
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
