using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using SharpCompress.Common;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Blob.Clients.Win.Entities;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Item;

namespace ThreeL.Blob.Clients.Win.ViewModels.Page
{
    public class TransferCompletePageViewModel : ObservableObject
    {
        private bool _hadTask;
        public bool HadTask
        {
            get => _hadTask;
            set => SetProperty(ref _hadTask, value);
        }

        private bool _loaded = false;
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand ClearRecordCommandAsync { get; set; }
        public AsyncRelayCommand OpenFolderCommandAsync { get; set; }
        public AsyncRelayCommand OpenFileCommandAsync { get; set; }
        public RelayCommand DeleteRecordCommand { get; set; }
        public ObservableCollection<TransferCompleteItemViewModel> TransferCompleteItemViewModels { get; set; }

        public TransferCompleteItemViewModel TransferCompleteItemViewModel { get; set; }

        private readonly DatabaseHelper _databaseHelper;
        private readonly IMapper _mapper;
        private readonly GrowlHelper _gridHelper;
        private readonly ILogger<TransferCompletePageViewModel> _logger;
        public TransferCompletePageViewModel(DatabaseHelper databaseHelper, IMapper mapper, GrowlHelper gridHelper, ILogger<TransferCompletePageViewModel> logger)
        {
            _mapper = mapper;
            _logger = logger;
            _databaseHelper = databaseHelper;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            ClearRecordCommandAsync = new AsyncRelayCommand(ClearRecordAsync);
            OpenFolderCommandAsync = new AsyncRelayCommand(OpenFolderAsync);
            OpenFileCommandAsync = new AsyncRelayCommand(OpenFileAsync);
            DeleteRecordCommand = new RelayCommand(DeleteRecord);
            TransferCompleteItemViewModels = new ObservableCollection<TransferCompleteItemViewModel>();
            WeakReferenceMessenger.Default.Register<TransferCompletePageViewModel, TransferCompleteItemViewModel, string>(this, Const.AddTransferRecord, async (x, y) =>
            {
                AddRecord(y, true);
            });
            _gridHelper = gridHelper;
        }

        public void AddRecord(TransferCompleteItemViewModel transferCompleteItemViewModel, bool desc = false)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                //传输完成队列中没有任务绑定的列表则添加，防止异常情况下出现两条重复记录
                if (TransferCompleteItemViewModels.FirstOrDefault(x => x.TaskId == transferCompleteItemViewModel.TaskId) == null)
                {
                    if (desc)
                        TransferCompleteItemViewModels.Insert(0, transferCompleteItemViewModel);
                    else
                        TransferCompleteItemViewModels.Add(transferCompleteItemViewModel);
                }

                HadTask = TransferCompleteItemViewModels.Count != 0;
            });
        }

        private Task ClearRecordAsync() 
        {
            var result = HandyControl.Controls
                .MessageBox.Show("确定清空所有的传输完成记录?", "提示", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                _databaseHelper.Excute("DELETE FROM TransferCompleteRecords",null);
                TransferCompleteItemViewModels.Clear();
                HadTask = false;
            }

            return Task.CompletedTask;
        }

        private async Task LoadAsync()
        {
            try
            {
                if (_loaded) return;
                var records = await _databaseHelper
                    .QueryListAsync<TransferCompleteRecord>("SELECT * FROM TransferCompleteRecords WHERE UserId = @UserId ORDER BY BeginTime DESC", new 
                    {
                        UserId = App.UserProfile.Id
                    });
                foreach (var record in records)
                {
                    AddRecord(_mapper.Map<TransferCompleteItemViewModel>(record));
                }

                _loaded = true;
            }
            catch (Exception ex)
            {
                _loaded = false;
                _gridHelper.Warning("加载历史记录异常");
                _logger.LogError(ex.ToString());
            }
        }

        private async Task OpenFolderAsync() 
        {
            var record = await _databaseHelper
                .QueryFirstOrDefaultAsync<TransferCompleteRecord>("SELECT * FROM TransferCompleteRecords WHERE Id = @Id", TransferCompleteItemViewModel);

            if (File.Exists(record.FileLocation))
            {
                Process.Start("explorer", "/select,\"" + record.FileLocation + "\"");
            }
        }

        private async Task OpenFileAsync()
        {
            var record = await _databaseHelper
                .QueryFirstOrDefaultAsync<TransferCompleteRecord>("SELECT * FROM TransferCompleteRecords WHERE Id = @Id", TransferCompleteItemViewModel);

            if (File.Exists(record.FileLocation))
            {
                Process process = new Process();
                ProcessStartInfo processStartInfo = new ProcessStartInfo(record.FileLocation);
                processStartInfo.UseShellExecute = true;
                process.StartInfo = processStartInfo;
                process.Start();
            }
        }

        private void DeleteRecord()
        {
             _databaseHelper.Excute("DELETE FROM TransferCompleteRecords WHERE Id = @Id", TransferCompleteItemViewModel);
            TransferCompleteItemViewModels.Remove(TransferCompleteItemViewModel);
        }
    }
}
