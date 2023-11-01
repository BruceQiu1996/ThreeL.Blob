using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Blob.Clients.Win.Entities;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Item;
using ThreeL.Blob.Shared.Domain.Metadata.FileObject;

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
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            DownloadItemViewModels = new ObservableCollection<DownloadItemViewModel>();
            //new download task
            WeakReferenceMessenger.Default.Register<DownloadingPageViewModel, DownloadFileRecord, string>(this, Const.AddDownloadRecord, async (x, y) =>
            {
                await AddNewDownloadTaskAsync(y, true, true);
            });

            //task complete
            WeakReferenceMessenger.Default.Register<DownloadingPageViewModel, DownloadItemViewModel, string>(this, Const.DownloadFinish, async (x, y) =>
            {
                RemoveTask(y);
                //增加到传输界面
                using (var context = dbContextFactory.CreateDbContext())
                {
                    var record = await context
                        .TransferCompleteRecords.FirstOrDefaultAsync(x => x.TaskId == y.Id);
                    var vm = _mapper.Map<TransferCompleteItemViewModel>(record);
                    WeakReferenceMessenger.Default.Send<TransferCompleteItemViewModel, string>(vm, Const.AddTransferRecord);
                }
            });

            PauseAllCommandAsync = new AsyncRelayCommand(PauseAllAsync);
            ResumeAllCommandAsync = new AsyncRelayCommand(ResumeAllAsync);
            CancelAllCommandAsync = new AsyncRelayCommand(CancelAllAsync);
        }

        /// <summary>
        /// 移除task
        /// </summary>
        /// <param name="uploadFileRecord"></param>
        private void RemoveTask(DownloadItemViewModel downloadItemViewModel)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DownloadItemViewModels.Remove(downloadItemViewModel);
                HadTask = DownloadItemViewModels.Count != 0;
            });

            WeakReferenceMessenger.Default.Send<ObservableCollection<DownloadItemViewModel>, string>(DownloadItemViewModels, Const.NotifyDownloadingCount);
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
            WeakReferenceMessenger.Default.Send<ObservableCollection<DownloadItemViewModel>, string>(DownloadItemViewModels, Const.NotifyDownloadingCount);
            if (start)
                await viewModel.StartAsync();
        }

        private Task PauseAllAsync()
        {
            foreach (var item in DownloadItemViewModels)
            {
                item.PauseCommand.Execute(null);
            }

            return Task.CompletedTask;
        }

        private async Task ResumeAllAsync()
        {
            foreach (var item in DownloadItemViewModels)
            {
                item.ResumeCommandAsync.ExecuteAsync(null);
            }
        }

        private async Task CancelAllAsync()
        {
            foreach (var item in DownloadItemViewModels)
            {
                item.CancelCommandAsync.ExecuteAsync(null);
            }
        }

        private async Task LoadAsync()
        {
            try
            {
                if (_isLoaded)
                    return;

                using (var context = _dbContextFactory.CreateDbContext())
                {
                    var downloadFileRecords = await context.DownloadFileRecords.Where(x => x.Status == FileDownloadingStatus.DownloadingSuspend
                    || x.Status == FileDownloadingStatus.DownloadingSuspend || x.Status == FileDownloadingStatus.Wait).OrderByDescending(x => x.CreateTime).ToListAsync();

                    foreach (var item in downloadFileRecords)
                    {
                        await AddNewDownloadTaskAsync(item, false);
                    }
                }

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                _isLoaded = false;
            }
        }
    }
}
