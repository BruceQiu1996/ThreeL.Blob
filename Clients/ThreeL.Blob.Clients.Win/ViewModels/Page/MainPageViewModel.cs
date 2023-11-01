using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ThreeL.Blob.Clients.Win.Dtos;
using ThreeL.Blob.Clients.Win.Entities;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Item;
using ThreeL.Blob.Infra.Core.Extensions.System;
using ThreeL.Blob.Infra.Core.Serializers;
using ThreeL.Blob.Shared.Domain.Metadata.FileObject;

namespace ThreeL.Blob.Clients.Win.ViewModels.Page
{
    public class MainPageViewModel : ObservableObject
    {
        public List<string> SortOptions { get; set; }
        private string _sortOption;
        public string SortOption
        {
            get => _sortOption;
            set 
            { 
                SetProperty(ref _sortOption, value);
                OnSort(FileObjDtos);
            }
        }
        public AsyncRelayCommand UploadCommandAsync { get; set; }
        public AsyncRelayCommand NewFolderCommand { get; set; }
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand RefreshCommandAsync { get; set; }
        public AsyncRelayCommand DownloadCommandAsync { get; set; }
        public RelayCommand SearchFileByKeywordCommand { get; set; }
        public AsyncRelayCommand DeleteCommandAsync { get; set; }
        public RelayCommand<MouseEventArgs> PreviewMouseMoveCommand { get; set; }
        public RelayCommand<DragEventArgs> DropCommand { get; set; }
        public RelayCommand GridGotFocusCommand { get; set; }
        private readonly GrpcService _grpcService;
        private readonly HttpRequest _httpRequest;
        private readonly IDbContextFactory<MyDbContext> _dbContextFactory;
        private readonly GrowlHelper _growlHelper;
        private readonly FileHelper _fileHelper;
        private readonly IMapper _mapper;
        private long _currentParent = 0;

        private ObservableCollection<FileObjItemViewModel> _allFileObjDtos;
        public ObservableCollection<FileObjItemViewModel> AllFileObjDtos
        {
            get => _allFileObjDtos;
            set
            {
                SetProperty(ref _allFileObjDtos, value);
            }
        }

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

        private string _keyword;
        public string Keyword
        {
            get => _keyword;
            set
            {
                SetProperty(ref _keyword, value);
            }
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
                                 GrowlHelper growlHelper, IMapper mapper, FileHelper fileHelper)
        {
            _mapper = mapper;
            _grpcService = grpcService;
            _httpRequest = httpRequest;
            _growlHelper = growlHelper;
            _fileHelper = fileHelper;
            _dbContextFactory = dbContextFactory;
            UploadCommandAsync = new AsyncRelayCommand(UploadAsync);
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            RefreshCommandAsync = new AsyncRelayCommand(RefreshAsync);
            NewFolderCommand = new AsyncRelayCommand(NewFolder);
            GridGotFocusCommand = new RelayCommand(GridGotFocus);
            DownloadCommandAsync = new AsyncRelayCommand(DownloadAsync);
            SearchFileByKeywordCommand = new RelayCommand(SearchFileByKeyword);
            DeleteCommandAsync = new AsyncRelayCommand(DeleteAsync);
            PreviewMouseMoveCommand = new RelayCommand<MouseEventArgs>(PreviewMouseMove);
            DropCommand = new RelayCommand<DragEventArgs>(Drop);
            FileObjDtos = new ObservableCollection<FileObjItemViewModel>();
            AllFileObjDtos = new ObservableCollection<FileObjItemViewModel>();
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
            SortOptions = new List<string>()
            {
                "时间 ↑","时间 ↓","文件大小 ↑","文件大小 ↓","文件名","文件类型"
            };

            SortOption = SortOptions[1];
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
            foreach (var item in AllFileObjDtos)
            {
                item.IsSelected = false;
            }
        }

        private Point _dragStartPoint;
        private void PreviewMouseMove(MouseEventArgs e) 
        {
            Point point = e.GetPosition(null);
            Vector diff = _dragStartPoint - point;
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var lbi = FindVisualParent<ListBoxItem>(((DependencyObject)e.OriginalSource));
                if (lbi != null)
                {
                    DragDrop.DoDragDrop(lbi, lbi.DataContext, DragDropEffects.Move);
                }
            }
        }

        private void Drop(DragEventArgs e)
        {
            var item = FindVisualParent<ListBoxItem>(((DependencyObject)e.OriginalSource));
            if (item == null) 
            {
                //查看鼠标上是否有文件

            }
            var target = item.DataContext as FileObjItemViewModel;

            if (target != null && target.IsFolder) //可以接受移动或者外部文件
            { 
                
            }
        }

        private T FindVisualParent<T>(DependencyObject child)
            where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            return FindVisualParent<T>(parentObject);
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
                Type = 0
            };

            model.Icon = _fileHelper.GetBitmapImageByFileExtension("folder.png");
            FileObjDtos.Add(model);
            AllFileObjDtos.Add(model);
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
                    FileObjDtos = new ObservableCollection<FileObjItemViewModel>(items.Select(x=>
                    {
                        var vm =  _mapper.Map<FileObjItemViewModel>(x);
                        vm.Icon = vm.IsFolder ? _fileHelper.GetBitmapImageByFileExtension("folder.png") : _fileHelper.GetIconByFileExtension(vm.Name).Item2;
                        vm.Type = vm.IsFolder ? 0 : _fileHelper.GetIconByFileExtension(vm.Name,true).Item1;

                        return vm;
                    }));

                    AllFileObjDtos = new ObservableCollection<FileObjItemViewModel>(FileObjDtos);
                }
                else
                {
                    AllFileObjDtos = new ObservableCollection<FileObjItemViewModel>();
                    FileObjDtos = new ObservableCollection<FileObjItemViewModel>();
                }
            }

            AllCount = FileObjDtos == null ? 0 : FileObjDtos.Count;
        }

        //排序
        private void OnSort(IEnumerable<FileObjItemViewModel> target)
        {
            switch (SortOption)
            {
                case "时间 ↑":
                    FileObjDtos = new ObservableCollection<FileObjItemViewModel>(target.OrderBy(x => x.CreateTime));
                    break;
                case "时间 ↓":
                    FileObjDtos = new ObservableCollection<FileObjItemViewModel>(target.OrderByDescending(x => x.CreateTime));
                    break;
                case "文件大小 ↑":
                    FileObjDtos = new ObservableCollection<FileObjItemViewModel>(target.OrderBy(x => x.Size));
                    break;
                case "文件大小 ↓":
                    FileObjDtos = new ObservableCollection<FileObjItemViewModel>(target.OrderByDescending(x => x.Size));
                    break;
                case "文件名":
                    FileObjDtos = new ObservableCollection<FileObjItemViewModel>(target.OrderBy(x => x.Name));
                    break;
                case "文件类型":
                    FileObjDtos = new ObservableCollection<FileObjItemViewModel>(target.OrderBy(x => x.Type));
                    break;
            };
        }

        /// <summary>
        /// 根据关键字查找文件
        /// </summary>
        private void SearchFileByKeyword() 
        {
            if (string.IsNullOrEmpty(Keyword))
            {
                OnSort(AllFileObjDtos);
            }
            else 
            {
                OnSort(AllFileObjDtos.Where(x => x.Name.Contains(Keyword)));
            }
        }

        private async Task DeleteAsync() 
        {
            var files = FileObjDtos.Where(x => x.IsSelected);
            if (files == null)
            {
                return;
            }

            var result = HandyControl.Controls.MessageBox.Show("确认删除?删除后不能够被找回", "提示", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var resp = await _httpRequest.PostAsync(Const.DELETE_FILE,new DeleteFileObjectsDto() 
                {
                    FileIds = files.Select(x => x.Id).ToArray()
                });

                if (resp != null) 
                {
                    //TODO okk
                }
            }
        }

        /// <summary>
        /// 下载所选
        /// </summary>
        /// <returns></returns>
        private async Task DownloadAsync()
        {
            var files = FileObjDtos.Where(x => x.IsSelected);
            if (files == null)
            {
                return;
            }

            foreach (var file in files)
            {
                await DownloadAsync(file, "d:\\ThreeL_blob_download");
            }
        }

        private async Task UploadAsync()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog()!.Value)
            {
                foreach (var file in openFileDialog.FileNames)
                {
                    var fileInfo = new FileInfo(file);
                    using (FileStream stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
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
                                    Status = FileUploadingStatus.Wait,
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

        private async Task DownloadAsync(FileObjItemViewModel itemViewModel,string location) 
        {
            if (itemViewModel.IsFolder)
            {
                //TODO 文件夹下载待定
            }
            else 
            {
                var resp = await _httpRequest.PostAsync(string.Format(Const.DOWNLOAD_FILE, itemViewModel.Id), null);
                if (resp != null) 
                {
                    var result = JsonSerializer.
                               Deserialize<DownloadFileResponseDto>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());

                    var tempFileName = Path.Combine(location, $"{Path.GetRandomFileName()}.tmp");
                    File.Create(tempFileName).Close();
                    using (var context = await _dbContextFactory.CreateDbContextAsync())
                    {
                        var record = new DownloadFileRecord()
                        {
                            FileId = result.FileId,
                            TaskId = result.TaskId,
                            TempFileLocation = tempFileName,
                            FileName = result.FileName,
                            TransferBytes = 0,
                            Status = FileDownloadingStatus.Wait,
                            Size = result.Size,
                            Code = result.Code
                        };
                        await context.DownloadFileRecords.AddAsync(record);
                        await context.SaveChangesAsync();
                        //发送添加下载任务事件
                        WeakReferenceMessenger.Default.Send<DownloadFileRecord, string>(record, Const.AddDownloadRecord);
                    };
                }
            }
        }
    }
}
