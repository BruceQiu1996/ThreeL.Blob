using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Blob.Clients.Win.Entities;
using ThreeL.Blob.Clients.Win.Helpers;
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
        private readonly DatabaseHelper _databaseHelper;
        private readonly IniSettings _iniSettings;
        public DownloadingPageViewModel(IMapper mapper,
                                        DatabaseHelper databaseHelper,
                                        ILogger<DownloadingPageViewModel> logger,
                                        IniSettings iniSettings)
        {
            _mapper = mapper;
            _logger = logger;
            _databaseHelper = databaseHelper;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            DownloadItemViewModels = new ObservableCollection<DownloadItemViewModel>();
            //new download task
            WeakReferenceMessenger.Default.Register<DownloadingPageViewModel, Tuple<DownloadFileRecord, bool>, string>(this, Const.AddDownloadRecord, async (x, y) =>
            {
                await AddNewDownloadTaskAsync(y.Item1, true, true, y.Item2);
            });

            //task complete
            WeakReferenceMessenger.Default.Register<DownloadingPageViewModel, Tuple<DownloadItemViewModel, TransferCompleteRecord>, string>(this, Const.DownloadFinish, async (x, y) =>
            {
                RemoveTask(y.Item1);
                var vm = _mapper.Map<TransferCompleteItemViewModel>(y.Item2);
                WeakReferenceMessenger.Default.Send(vm, Const.AddTransferRecord);
            });

            WeakReferenceMessenger.Default.Register<DownloadingPageViewModel, string, string>(this, Const.StartNewDownloadTask, (x, y) =>
            {
                lock (Const.DownloadRunTaskLock)
                {
                    var count = DownloadItemViewModels.Count(x => x.Status == FileDownloadingStatus.Downloading);
                    if (count < _iniSettings.MaxDownloadThreads)
                    {
                        DownloadItemViewModels
                        .Where(x => x.Status == FileDownloadingStatus.Wait).Take(_iniSettings.MaxDownloadThreads - count)
                        .ToList().ForEach(async x => await x.StartAsync());
                    }
                }
            });

            PauseAllCommandAsync = new AsyncRelayCommand(PauseAllAsync);
            ResumeAllCommandAsync = new AsyncRelayCommand(ResumeAllAsync);
            CancelAllCommandAsync = new AsyncRelayCommand(CancelAllAsync);
            _iniSettings = iniSettings;
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

            WeakReferenceMessenger.Default.Send(DownloadItemViewModels, Const.NotifyDownloadingCount);
        }

        /// <summary>
        /// 下载文件加到下载文件队列中
        /// </summary>
        /// <param name="uploadFileRecord"></param>
        /// <param name="start">是否添加后就启动上传</param>
        /// <param name="desc">是否加到队列最开始</param>
        /// <param name="openWhenComplete">下载后是否打开</param>
        /// <returns></returns>
        private async Task AddNewDownloadTaskAsync(DownloadFileRecord downloadFileRecord, bool start = true, bool desc = false, bool openWhenComplete = false)
        {
            var viewModel = App.ServiceProvider!.GetRequiredService<DownloadItemViewModel>();
            _mapper.Map(downloadFileRecord, viewModel);
            viewModel.OpenWhenComplete = openWhenComplete; //是否下载后就打开文件
            if (desc)
                DownloadItemViewModels.Insert(0, viewModel);
            else
                DownloadItemViewModels.Add(viewModel);
            HadTask = true;
            WeakReferenceMessenger.Default.Send(DownloadItemViewModels, Const.NotifyDownloadingCount);
            if (start)
            {
                await viewModel.StartAsync();
            }
        }

        private Task PauseAllAsync()
        {
            foreach (var item in DownloadItemViewModels)
            {
                item.Status = FileDownloadingStatus.DownloadingSuspend;
            }
            
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
                item.Status = FileDownloadingStatus.DownloadingSuspend;
            }

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

                //var downloadFileRecords = await context.DownloadFileRecords.Where(x => x.Status == FileDownloadingStatus.DownloadingSuspend
                    //|| x.Status == FileDownloadingStatus.DownloadingSuspend || x.Status == FileDownloadingStatus.Wait).OrderByDescending(x => x.CreateTime).ToListAsync();

                var downloadFileRecords = await _databaseHelper
                    .QueryListAsync<DownloadFileRecord>("SELECT * FROM DownloadFileRecords WHERE Status <=3 ORDER BY CreateTime DESC",null);

                _databaseHelper.Excute("UPDATE DownloadFileRecords SET Status = 3 WHERE Id in @Ids", new { Ids = downloadFileRecords.Select(x => x.Id) });
                foreach (var item in downloadFileRecords)
                {
                    item.Status = FileDownloadingStatus.DownloadingSuspend;
                    await AddNewDownloadTaskAsync(item, false);
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
