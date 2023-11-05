using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Item;
using Z.EntityFramework.Plus;

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
        public ObservableCollection<TransferCompleteItemViewModel> TransferCompleteItemViewModels { get; set; }

        private readonly IDbContextFactory<MyDbContext> _dbContextFactory;
        private readonly IMapper _mapper;
        public TransferCompletePageViewModel(IDbContextFactory<MyDbContext> dbContextFactory, IMapper mapper)
        {
            _mapper = mapper;
            _dbContextFactory = dbContextFactory;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            ClearRecordCommandAsync = new AsyncRelayCommand(ClearRecordAsync);
            TransferCompleteItemViewModels = new ObservableCollection<TransferCompleteItemViewModel>();
            WeakReferenceMessenger.Default.Register<TransferCompletePageViewModel, TransferCompleteItemViewModel, string>(this, Const.AddTransferRecord, async (x, y) =>
            {
                AddRecord(y, true);
            });
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
                lock (Const.WriteDbLock)
                {
                    using (var context = _dbContextFactory.CreateDbContext())
                    {
                        context.TransferCompleteRecords.Delete();
                        context.SaveChanges();

                        TransferCompleteItemViewModels.Clear();
                        HadTask = false;
                    }
                }
            }

            return Task.CompletedTask;
        }

        private async Task LoadAsync()
        {
            try
            {
                if (_loaded) return;
                using (var context = _dbContextFactory.CreateDbContext())
                {
                    var records = await context.TransferCompleteRecords.OrderByDescending(x => x.BeginTime).ToListAsync();
                    foreach (var record in records)
                    {
                        AddRecord(_mapper.Map<TransferCompleteItemViewModel>(record));
                    }
                }

                _loaded = true;
            }
            catch (Exception ex)
            {
                _loaded = false;
            }
        }
    }
}
