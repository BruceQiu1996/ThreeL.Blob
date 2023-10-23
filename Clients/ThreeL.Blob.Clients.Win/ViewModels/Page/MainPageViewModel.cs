using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Dtos;
using ThreeL.Blob.Clients.Win.Entities;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Item;
using ThreeL.Blob.Infra.Core.Extensions.System;
using ThreeL.Blob.Infra.Core.Serializers;

namespace ThreeL.Blob.Clients.Win.ViewModels.Page
{
    public class MainPageViewModel : ObservableObject
    {
        public AsyncRelayCommand UploadCommandAsync { get; set; }
        public AsyncRelayCommand NewFolderCommand { get; set; }
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand RefreshCommandAsync { get; set; }
        public RelayCommand GridGotFocusCommand { get; set; }
        private readonly GrpcService _grpcService;
        private readonly HttpRequest _httpRequest;
        private readonly IDbContextFactory<MyDbContext> _dbContextFactory;
        private readonly GrowlHelper _growlHelper;
        private readonly IMapper _mapper;
        private long _currentParent = 0;

        private ObservableCollection<FileObjItemViewModel> _fileObjDtos;
        public ObservableCollection<FileObjItemViewModel> FileObjDtos
        {
            get => _fileObjDtos;
            set
            {
                SetProperty(ref _fileObjDtos, value);
                SelectedCount = value == null ? 0 : value.Count(x => x.IsSelected);
            }
        }

        private ObservableCollection<FileObjItemViewModel> _urls;
        public ObservableCollection<FileObjItemViewModel> Urls
        {
            get => _urls;
            set => SetProperty(ref _urls, value);
        }

        private int _allCount;
        public int AllCount
        {
            get => _allCount;
            set => SetProperty(ref _allCount, value);
        }

        private int _selectedCount;
        public int SelectedCount
        {
            get => _selectedCount;
            set => SetProperty(ref _selectedCount, value);
        }

        public MainPageViewModel(GrpcService grpcService, HttpRequest httpRequest,
                                 IDbContextFactory<MyDbContext> dbContextFactory,
                                 GrowlHelper growlHelper, IMapper mapper)
        {
            _mapper = mapper;
            _grpcService = grpcService;
            _httpRequest = httpRequest;
            _growlHelper = growlHelper;
            _dbContextFactory = dbContextFactory;
            UploadCommandAsync = new AsyncRelayCommand(UploadAsync);
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            RefreshCommandAsync = new AsyncRelayCommand(RefreshAsync);
            NewFolderCommand = new AsyncRelayCommand(NewFolder);
            GridGotFocusCommand = new RelayCommand(GridGotFocus);
            FileObjDtos = new ObservableCollection<FileObjItemViewModel>();
            Urls = new ObservableCollection<FileObjItemViewModel>()
            {
                new FileObjItemViewModel
                {
                    Id = 0,
                    Name = "我的网盘",
                    IsUrlSelected = true,
                    IsFolder = true
                }
            };

            //更新选中文件数量
            WeakReferenceMessenger.Default.Register<MainPageViewModel, FileObjItemViewModel, string>(this, Const.SelectItem, (x, y) =>
            {
                SelectedCount = FileObjDtos == null ? 0 : FileObjDtos.Count(x => x.IsSelected);
            });

            //双击事件
            WeakReferenceMessenger.Default.Register<MainPageViewModel, FileObjItemViewModel, string>(this, Const.DoubleClickItem, async (x, y) =>
            {
                await DoubleClickAsync(y);
            });
        }

        private async Task LoadAsync()
        {
            await RefreshByParentAsync(_currentParent);
        }

        private async Task RefreshAsync()
        {
            await RefreshByParentAsync(_currentParent);
        }

        private void GridGotFocus() 
        {
            foreach (var item in FileObjDtos)
            {
                item.IsSelected = false;
            }
        }

        private async Task NewFolder()
        {
            var model = new FileObjItemViewModel()
            {
                Name = "新建文件夹",
                ParentFolder = _currentParent,
                IsFolder = true,
                IsRename = true,
                IsSelected = true,
            };

            FileObjDtos.Add(model);
            await Task.Delay(100);
            model.IsFocus = true;
        }

        private async Task DoubleClickAsync(FileObjItemViewModel fileObjItemViewModel)
        {
            if (fileObjItemViewModel.IsFolder)
            {
                await RefreshByParentAsync(fileObjItemViewModel.Id);
                var index = Urls.IndexOf(fileObjItemViewModel);
                if (index == -1)
                {
                    Urls.Add(fileObjItemViewModel);
                }
                else
                {
                    for (var i = Urls.Count - 1; i > index; i--)
                    {
                        Urls.RemoveAt(i);
                    }
                }

                fileObjItemViewModel.IsUrlSelected = true;
                _currentParent = fileObjItemViewModel.Id;
            }
        }

        private async Task RefreshByParentAsync(long parent)
        {
            var resp = await _httpRequest.GetAsync($"{Const.UPLOAD_FILE}/{parent}");
            if (resp != null)
            {
                var items = JsonSerializer
                    .Deserialize<IEnumerable<FileObjDto>>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());

                if (items != null && items.Count() > 0)
                {
                    FileObjDtos = new ObservableCollection<FileObjItemViewModel>(items.Select(_mapper.Map<FileObjItemViewModel>));
                }
                else
                {
                    FileObjDtos = new ObservableCollection<FileObjItemViewModel>();
                }
            }

            AllCount = FileObjDtos == null ? 0 : FileObjDtos.Count;
        }

        private async Task UploadAsync()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog()!.Value)
            {
                var fileInfo = new FileInfo(openFileDialog.FileName);
                using (FileStream stream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                {
                    var code = stream.ToSHA256();
                    var resp = await _httpRequest.PostAsync(Const.UPLOAD_FILE, new UploadFileDto()
                    {
                        Name = fileInfo.Name,
                        Size = fileInfo.Length,
                        ParentFolder = _currentParent,
                        Code = code
                    });

                    if (resp != null && resp.IsSuccessStatusCode)
                    {
                        var result = JsonSerializer.
                            Deserialize<UploadFileResponseDto>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());
                        using (var context = await _dbContextFactory.CreateDbContextAsync())
                        {
                            var record = new UploadFileRecord()
                            {
                                FileId = result.FileId,
                                FileName = fileInfo.Name,
                                Size = fileInfo.Length,
                                FileLocation = fileInfo.FullName,
                                TransferBytes = 0,
                                Status = Status.Doing,
                                Code = code
                            };
                            await context.UploadFileRecords.AddAsync(record);
                            await context.SaveChangesAsync();
                            WeakReferenceMessenger.Default.Send<UploadFileRecord, string>(record, Const.AddUploadRecord);
                        };
                    }
                }
            }
        }
    }
}
