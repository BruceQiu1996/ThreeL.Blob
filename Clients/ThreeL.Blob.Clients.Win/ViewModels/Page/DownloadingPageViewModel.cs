using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Entities;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Item;

namespace ThreeL.Blob.Clients.Win.ViewModels.Page
{
    public class DownloadingPageViewModel : ObservableObject
    {
        private bool _isLoaded = false;
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand PauseAllCommandAsync { get; set; }
        public AsyncRelayCommand ResumeAllCommandAsync { get; set; }
        public AsyncRelayCommand CancelAllCommandAsync { get; set; }
        public ObservableCollection<DownloadItemViewModel> DownloadItemViewModels { get; set; }

        private bool _hadTask;
        public bool HadTask
        {
            get => _hadTask;
            set => SetProperty(ref _hadTask, value);
        }
        private readonly IMapper _mapper;
        private readonly ILogger<DownloadingPageViewModel> _logger;
        private readonly IDbContextFactory<MyDbContext> _dbContextFactory;
        public DownloadingPageViewModel(IMapper mapper,
                                        IDbContextFactory<MyDbContext> dbContextFactory,
                                        ILogger<DownloadingPageViewModel> logger)
        {
            _mapper = mapper;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            DownloadItemViewModels = new ObservableCollection<DownloadItemViewModel>();
            //new download task
            WeakReferenceMessenger.Default.Register<DownloadingPageViewModel, DownloadFileRecord, string>(this, Const.AddDownloadRecord, async (x, y) =>
            {
                await AddNewDownloadTaskAsync(y, true, true);
            });
        }

        /// <summary>
        /// 下载文件加到下载文件队列中
        /// </summary>
        /// <param name="uploadFileRecord"></param>
        /// <param name="start">是否添加后就启动上传</param>
        /// <param name="desc">是否加到队列最开始</param>
        /// <returns></returns>
        private async Task AddNewDownloadTaskAsync(DownloadFileRecord downloadFileRecord, bool start = true, bool desc = false)
        {
            var viewModel = App.ServiceProvider!.GetRequiredService<DownloadItemViewModel>();
            _mapper.Map(downloadFileRecord, viewModel);
            if (desc)
                DownloadItemViewModels.Insert(0, viewModel);
            else
                DownloadItemViewModels.Add(viewModel);
            HadTask = true;
            //WeakReferenceMessenger.Default.Send<ObservableCollection<UploadItemViewModel>, string>(UploadItemViewModels, Const.NotifyUploadingCount);
            if (start)
                await viewModel.StartAsync();
        }
    }
}
