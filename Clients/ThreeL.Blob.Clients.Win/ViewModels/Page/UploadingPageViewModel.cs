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
    public class UploadingPageViewModel : ObservableObject
    {
        private bool _isLoaded = false;
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand PauseAllCommandAsync { get; set; }
        public AsyncRelayCommand ResumeAllCommandAsync { get; set; }
        public AsyncRelayCommand CancelAllCommandAsync { get; set; }
        public ObservableCollection<UploadItemViewModel> UploadItemViewModels { get; set; }

        private bool _hadTask;
        public bool HadTask
        {
            get => _hadTask;
            set => SetProperty(ref _hadTask, value);
        }
        private readonly IMapper _mapper;
        private readonly ILogger<UploadingPageViewModel> _logger;
        private readonly DatabaseHelper _databaseHelper;
        private readonly IniSettings _iniSettings;
        public UploadingPageViewModel(IMapper mapper,
                                      DatabaseHelper databaseHelper,
                                      ILogger<UploadingPageViewModel> logger,
                                      IniSettings iniSettings)
        {
            _iniSettings = iniSettings;
            _mapper = mapper;
            _logger = logger;
            _databaseHelper = databaseHelper;
            UploadItemViewModels = new ObservableCollection<UploadItemViewModel>();
            //new upload task
            WeakReferenceMessenger.Default.Register<UploadingPageViewModel, UploadFileRecord, string>(this, Const.AddUploadRecord, async (x, y) =>
             {
                 await AddNewUploadTaskAsync(y, true);
             });

            //exit
            WeakReferenceMessenger.Default.Register<UploadingPageViewModel, string, string>(this, Const.Exit, async (x, y) =>
            {
                foreach (var item in UploadItemViewModels)
                {
                    item.PauseCommand.Execute(null);
                }

                WeakReferenceMessenger.Default.Send("uploading", Const.CanExit);
            });

            //finish
            WeakReferenceMessenger.Default.Register<UploadingPageViewModel, Tuple<UploadItemViewModel, TransferCompleteRecord>, string>(this, Const.UploadFinish, async (x, y) =>
            {
                RemoveTask(y.Item1);
                var vm = _mapper.Map<TransferCompleteItemViewModel>(y.Item2);
                WeakReferenceMessenger.Default.Send(vm, Const.AddTransferRecord);
            });

            WeakReferenceMessenger.Default.Register<UploadingPageViewModel, string, string>(this, Const.StartNewUploadTask,  (x, y) =>
            {
                lock (Const.UploadRunTaskLock)
                {
                    var count = UploadItemViewModels.Count(x => x.Status == FileUploadingStatus.Uploading);
                    if (count < _iniSettings.MaxUploadThreads)
                    {
                        UploadItemViewModels.Where(x => x.Status == FileUploadingStatus.Wait).Take(_iniSettings.MaxUploadThreads - count)
                        .ToList().ForEach(async x => await x.StartAsync());
                    }
                }
            });
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            PauseAllCommandAsync = new AsyncRelayCommand(PauseAllAsync);
            ResumeAllCommandAsync = new AsyncRelayCommand(ResumeAllAsync);
            CancelAllCommandAsync = new AsyncRelayCommand(CancelAllAsync);
        }

        /// <summary>
        /// 上传文件加到上传文件队列中
        /// </summary>
        /// <param name="uploadFileRecord"></param>
        /// <param name="start">是否添加后就启动上传</param>
        /// <param name="desc">是否加到队列最开始</param>
        /// <returns></returns>
        private async Task AddNewUploadTaskAsync(UploadFileRecord uploadFileRecord, bool desc = false)
        {
            var viewModel = App.ServiceProvider!.GetRequiredService<UploadItemViewModel>();
            _mapper.Map(uploadFileRecord, viewModel);
            if (desc)
                UploadItemViewModels.Insert(0, viewModel);
            else
                UploadItemViewModels.Add(viewModel);
            HadTask = true;
            WeakReferenceMessenger.Default.Send(UploadItemViewModels, Const.NotifyUploadingCount);
            if (uploadFileRecord.Status == FileUploadingStatus.Wait)
                await viewModel.StartAsync();
        }

        private Task PauseAllAsync()
        {
            foreach (var item in UploadItemViewModels)
            {
                item.Status = FileUploadingStatus.UploadingSuspend;
            }

            foreach (var item in UploadItemViewModels)
            {
                item.PauseCommand.Execute(null);
            }

            return Task.CompletedTask;
        }

        private async Task ResumeAllAsync()
        {
            foreach (var item in UploadItemViewModels)
            {
                item.ResumeCommandAsync.ExecuteAsync(null);
            }
        }

        private async Task CancelAllAsync()
        {
            foreach (var item in UploadItemViewModels)
            {
                item.Status = FileUploadingStatus.UploadingSuspend;
            }

            foreach (var item in UploadItemViewModels)
            {
                item.CancelCommandAsync.ExecuteAsync(null);
            }
        }

        /// <summary>
        /// 移除task
        /// </summary>
        /// <param name="uploadFileRecord"></param>
        private void RemoveTask(UploadItemViewModel uploadFileRecord)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UploadItemViewModels.Remove(uploadFileRecord);
                HadTask = UploadItemViewModels.Count != 0;
            });

            WeakReferenceMessenger.Default.Send(UploadItemViewModels, Const.NotifyUploadingCount);
        }

        private async Task LoadAsync()
        {
            try
            {
                if (_isLoaded)
                    return;

                var records = await _databaseHelper
                    .QueryListAsync<UploadFileRecord>("SELECT * FROM UploadFileRecords WHERE Status <=3 ORDER BY CreateTime DESC", null);

                _databaseHelper.Excute("UPDATE UploadFileRecords SET Status = 3 WHERE Id in @Ids", new { Ids = records.Select(x=>x.Id)});
                foreach (var item in records)
                {
                    item.Status = FileUploadingStatus.UploadingSuspend;
                    await AddNewUploadTaskAsync(item, false);
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
