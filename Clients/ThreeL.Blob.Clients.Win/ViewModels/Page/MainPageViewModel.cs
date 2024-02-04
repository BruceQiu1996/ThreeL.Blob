using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
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
using ThreeL.Blob.Clients.Win.ViewModels.Window;
using ThreeL.Blob.Clients.Win.Windows;
using ThreeL.Blob.Infra.Core.Extensions.System;
using ThreeL.Blob.Infra.Core.Serializers;
using ThreeL.Blob.Shared.Domain.Metadata.FileObject;
using ThreeL.Blob.Infra.Core.Extensions.System;

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
                OnSort(FileObjViewModels);
            }
        }
        public AsyncRelayCommand UploadCommandAsync { get; set; }
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public RelayCommand SearchFileByKeywordCommand { get; set; }
        public AsyncRelayCommand<KeyEventArgs> KeyDownCommandAsync { get; set; }
        public RelayCommand<DragEventArgs> DropCommand { get; set; }
        public RelayCommand GridGotFocusCommand { get; set; }
        public AsyncRelayCommand RefreshCommandAsync { get; set; }
        public AsyncRelayCommand NewFolderCommandAsync { get; set; }
        public RelayCommand SelectAllCommand { get; set; }
        public RelayCommand SelectNoCommand { get; set; }

        public RelayCommand<MouseButtonEventArgs> FileObjectsChooseDragCommand { get; set; }

        private readonly GrpcService _grpcService;
        private readonly ApiHttpRequest _httpRequest;
        private readonly DatabaseHelper _databaseHelper;
        private readonly GrowlHelper _growlHelper;
        private readonly FileHelper _fileHelper;
        private readonly IniSettings _iniSettings;
        private readonly IMapper _mapper;
        private long _currentParent = 0;

        private ObservableCollection<FileObjItemViewModel> _allFileObjViewModels;
        public ObservableCollection<FileObjItemViewModel> AllFileObjViewModels
        {
            get => _allFileObjViewModels;
            set
            {
                SetProperty(ref _allFileObjViewModels, value);
            }
        }

        private ObservableCollection<FileObjItemViewModel> _fileObjViewModels;
        public ObservableCollection<FileObjItemViewModel> FileObjViewModels
        {
            get => _fileObjViewModels;
            set
            {
                SetProperty(ref _fileObjViewModels, value);
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

        private bool _isListView;
        public bool IsListView
        {
            get => _isListView;
            set => SetProperty(ref _isListView, value);
        }

        public MainPageViewModel(GrpcService grpcService, ApiHttpRequest httpRequest,
                                 DatabaseHelper databaseHelper,
                                 GrowlHelper growlHelper, IMapper mapper, FileHelper fileHelper, IniSettings iniSettings)
        {
            _mapper = mapper;
            _grpcService = grpcService;
            _httpRequest = httpRequest;
            _growlHelper = growlHelper;
            _iniSettings = iniSettings;
            _fileHelper = fileHelper;
            _databaseHelper = databaseHelper;
            UploadCommandAsync = new AsyncRelayCommand(UploadAsync);
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            GridGotFocusCommand = new RelayCommand(GridGotFocus);
            SearchFileByKeywordCommand = new RelayCommand(SearchFileByKeyword);
            DropCommand = new RelayCommand<DragEventArgs>(Drop);
            FileObjectsChooseDragCommand = new RelayCommand<MouseButtonEventArgs>(FileObjectsChooseDrag);
            FileObjViewModels = new ObservableCollection<FileObjItemViewModel>();
            AllFileObjViewModels = new ObservableCollection<FileObjItemViewModel>();
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
                SelectedCount = FileObjViewModels == null ? 0 : FileObjViewModels.Count(x => x.IsSelected);
            });

            //双击事件
            WeakReferenceMessenger.Default.Register<MainPageViewModel, FileObjItemViewModel, string>(this, Const.DoubleClickItem, async (x, y) =>
            {
                await DoubleClickAsync(y);
            });

            //下载分享文件
            WeakReferenceMessenger.Default.Register<MainPageViewModel, string, string>(this, Const.DownloadSharedFile, async (x, y) =>
            {
                await DownloadSharedFileAsync(y, _iniSettings.DownloadLocation, false);
            });

            //下载分享文件并且打开
            WeakReferenceMessenger.Default.Register<MainPageViewModel, string, string>(this, Const.DownloadSharedFileAndOpen, async (x, y) =>
            {
                await DownloadSharedFileAsync(y, _iniSettings.DownloadLocation, true);
            });

            #region 菜单
            RefreshCommandAsync = new AsyncRelayCommand(RefreshAsync);
            NewFolderCommandAsync = new AsyncRelayCommand(NewFolderAsync);
            SelectAllCommand = new RelayCommand(SelectAll);
            SelectNoCommand = new RelayCommand(SelectNo);

            //下载
            WeakReferenceMessenger.Default.Register<MainPageViewModel, FileObjItemViewModel, string>(this, Const.MenuDownload, async (x, y) =>
            {
                await DownloadAsync();
            });
            //删除
            WeakReferenceMessenger.Default.Register<MainPageViewModel, FileObjItemViewModel, string>(this, Const.MenuDelete, async (x, y) =>
            {
                await DeleteAsync();
            });
            //全选
            WeakReferenceMessenger.Default.Register<MainPageViewModel, FileObjItemViewModel, string>(this, Const.MenuSelectAll, (x, y) =>
            {
                SelectAll();
            });
            //全不选
            WeakReferenceMessenger.Default.Register<MainPageViewModel, FileObjItemViewModel, string>(this, Const.MenuSelectNo, (x, y) =>
            {
                SelectNo();
            });
            //重命名
            WeakReferenceMessenger.Default.Register<MainPageViewModel, FileObjItemViewModel, string>(this, Const.MenuRename, async (x, y) =>
            {
                await RenameAsync(y);
            });
            //移动
            WeakReferenceMessenger.Default.Register<MainPageViewModel, FileObjItemViewModel, string>(this, Const.MenuMove, async (x, y) =>
            {
                MoveCommand();
            });
            //确认移动
            WeakReferenceMessenger.Default.Register<MainPageViewModel, TreeViewFolderViewModel, string>(this, Const.ConfirmMove, (MessageHandler<MainPageViewModel, TreeViewFolderViewModel>)(async (x, y) =>
            {
                await ConfirmCommandAsync(y);
            }));
            #endregion
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
            foreach (var item in AllFileObjViewModels)
            {
                item.IsSelected = false;
            }
        }

        private void MoveCommand()
        {
            if (FileObjViewModels.Count(x => x.IsSelected) == 0)
                return;

            var dialog = App.ServiceProvider.GetRequiredService<Move>();
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Owner = App.ServiceProvider!.GetRequiredService<MainWindow>();
            dialog.ShowDialog();
        }

        private async Task ConfirmCommandAsync(TreeViewFolderViewModel viewFolderViewModel)
        {
            if (viewFolderViewModel == null)
                return;

            var resp = await _httpRequest.PostAsync(Const.UPDATE_LOCATION, new UpdateFileObjectLocationDto()
            {
                FileIds = FileObjViewModels.Where(x => x.IsSelected).Select(x => x.Id).ToArray(),
                ParentFolder = viewFolderViewModel.Id
            });

            if (resp != null)
            {
                await RefreshAsync();
            }
        }

        private void FileObjectsChooseDrag(MouseButtonEventArgs eventArgs)
        {
            var listbox = FindVisualParent<ListBox>(eventArgs.OriginalSource as DependencyObject);
            object data = GetDataFromListBox(listbox, eventArgs.GetPosition(listbox));

            if (data != null)
            {
                DragDrop.DoDragDrop(listbox, listbox.Items, DragDropEffects.Move);
            }
        }

        private  object GetDataFromListBox(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);

                    if (data == DependencyProperty.UnsetValue)
                    {
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    }

                    if (element == source)
                    {
                        return null;
                    }
                }

                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }

            return null;
        }

        #region 操作文件集合
        public void AddFileObject(FileObjItemViewModel itemViewModel, bool first = false)
        {
            if (!first)
                FileObjViewModels?.Add(itemViewModel);
            else
                FileObjViewModels?.Insert(0, itemViewModel);
            AllCount = FileObjViewModels == null ? 0 : FileObjViewModels.Count;
            SelectedCount = FileObjViewModels == null ? 0 : FileObjViewModels.Count(x => x.IsSelected);
        }

        public void RemoveFileObject(FileObjItemViewModel itemViewModel)
        {
            FileObjViewModels?.Remove(itemViewModel);
            AllCount = FileObjViewModels == null ? 0 : FileObjViewModels.Count;
            SelectedCount = FileObjViewModels == null ? 0 : FileObjViewModels.Count(x => x.IsSelected);
        }
        #endregion

        #region 全选/全不选
        public void SelectAll()
        {
            foreach (var item in FileObjViewModels)
            {
                item.IsSelected = true;
            }
        }

        public void SelectNo()
        {
            foreach (var item in FileObjViewModels)
            {
                item.IsSelected = false;
            }
        }

        #endregion
        //拖拽上传文件
        private async void Drop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                long parent = 0;
                var item = FindVisualParent<ListBoxItem>(((DependencyObject)e.OriginalSource));
                if (item == null)
                {
                    parent = _currentParent;
                }
                else
                {
                    var vm = item.DataContext as FileObjItemViewModel;
                    if (vm == null || !vm.IsFolder)
                        return;

                    parent = vm.Id;
                }
                var files = (Array)e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files)
                {
                    if (File.Exists(file.ToString()))
                    {
                        await UploadFileAsync(file.ToString(), parent);
                    }
                    else if (Directory.Exists(file.ToString()))
                    {
                        await UploadFolderAsync(file.ToString(), parent);
                    }
                }
            }
        }

        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            return FindVisualParent<T>(parentObject);
        }

        private async Task NewFolderAsync()
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
            AllFileObjViewModels.Insert(0, model);
            AddFileObject(model, true);
            await Task.Delay(100);
            model.IsFocus = true;
        }

        public static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                var correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }

            return null;
        }

        private async Task RenameAsync(FileObjItemViewModel fileObjItemViewModel)
        {
            fileObjItemViewModel.IsRename = true;
            await Task.Delay(100);
            fileObjItemViewModel.IsFocus = true;
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
            else
            {
                if (!Directory.Exists(_iniSettings.TempLocation))
                {
                    _growlHelper.Warning("临时文件下载目录不存在");
                    return;
                }
                //下载后打开文件
                await DownloadFileAsync(fileObjItemViewModel.Id, _iniSettings.TempLocation, true);
            }
        }

        /// <summary>
        /// 获取当前目录下的元素
        /// </summary>
        /// <param name="parent">目录id</param>
        /// <returns></returns>
        private async Task RefreshByParentAsync(long parent)
        {
            var resp = await _httpRequest.GetAsync($"{Const.UPLOAD_FILE}/{parent}");
            if (resp != null)
            {
                var items = JsonSerializer
                    .Deserialize<IEnumerable<FileObjDto>>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());

                if (items != null && items.Count() > 0)
                {
                    FileObjViewModels = new ObservableCollection<FileObjItemViewModel>(items.Select(x =>
                    {
                        var vm = _mapper.Map<FileObjItemViewModel>(x);
                        vm.Icon = vm.IsFolder ? _fileHelper.GetBitmapImageByFileExtension("folder.png") : _fileHelper.GetIconByFileExtension(vm.Name).Item2;
                        vm.Type = vm.IsFolder ? 0 : _fileHelper.GetIconByFileExtension(vm.Name, true).Item1;

                        return vm;
                    }));

                    AllFileObjViewModels = new ObservableCollection<FileObjItemViewModel>(FileObjViewModels);
                }
                else
                {
                    AllFileObjViewModels = new ObservableCollection<FileObjItemViewModel>();
                    FileObjViewModels = new ObservableCollection<FileObjItemViewModel>();
                }
            }

            AllCount = FileObjViewModels == null ? 0 : FileObjViewModels.Count;
        }

        //排序
        private void OnSort(IEnumerable<FileObjItemViewModel> target)
        {
            switch (SortOption)
            {
                case "时间 ↑":
                    FileObjViewModels = new ObservableCollection<FileObjItemViewModel>(target.OrderBy(x => x.CreateTime));
                    break;
                case "时间 ↓":
                    FileObjViewModels = new ObservableCollection<FileObjItemViewModel>(target.OrderByDescending(x => x.CreateTime));
                    break;
                case "文件大小 ↑":
                    FileObjViewModels = new ObservableCollection<FileObjItemViewModel>(target.OrderBy(x => x.Size));
                    break;
                case "文件大小 ↓":
                    FileObjViewModels = new ObservableCollection<FileObjItemViewModel>(target.OrderByDescending(x => x.Size));
                    break;
                case "文件名":
                    FileObjViewModels = new ObservableCollection<FileObjItemViewModel>(target.OrderBy(x => x.Name));
                    break;
                case "文件类型":
                    FileObjViewModels = new ObservableCollection<FileObjItemViewModel>(target.OrderBy(x => x.Type));
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
                OnSort(AllFileObjViewModels);
            }
            else
            {
                OnSort(AllFileObjViewModels.Where(x => x.Name.Contains(Keyword)));
            }
        }

        private async Task DeleteAsync()
        {
            var files = FileObjViewModels.Where(x => x.IsSelected).ToList();
            if (files == null || files.Count() <= 0)
            {
                return;
            }

            var tempParent = _currentParent;
            var resp = await _httpRequest.PostAsync(Const.DELETE_FILE, new DeleteFileObjectsDto()
            {
                FileIds = files.Select(x => x.Id).ToArray()
            });

            if (resp != null)
            {
                foreach (var file in files)
                {
                    AllFileObjViewModels.Remove(file);
                    RemoveFileObject(file);
                }
            }
        }

        /// <summary>
        /// 下载所选
        /// </summary>
        /// <returns></returns>
        private async Task DownloadAsync()
        {
            var files = FileObjViewModels.Where(x => x.IsSelected);
            if (files == null)
            {
                return;
            }

            var dialog = App.ServiceProvider!.GetRequiredService<DownloadEnsure>();
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Owner = App.ServiceProvider!.GetRequiredService<MainWindow>();
            (dialog.DataContext as DownloadEnsureViewModel)!.DownloadEnsure = dialog;
            var result = dialog.ShowDialog();

            if (result != null && result.Value)
            {
                if (string.IsNullOrEmpty(_iniSettings.DownloadLocation))
                {
                    _growlHelper.Warning("下载目录异常");
                    return;
                }

                if (!Directory.Exists(_iniSettings.DownloadLocation))
                {
                    Directory.CreateDirectory(_iniSettings.DownloadLocation);
                }

                //存在文件夹则判断
                long size = 0;
                List<IEnumerable<PreDownloadFolderFileItemResponseDto>> items = new List<IEnumerable<PreDownloadFolderFileItemResponseDto>>();
                var folders = files.Where(x => x.IsFolder);
                foreach (var folder in folders)
                {
                    var resp = await _httpRequest.GetAsync(string.Format(Const.PRE_DOWNLOAD_FOLDER, folder.Id));
                    if (resp != null)
                    {
                        var dto = JsonSerializer.Deserialize<PreDownloadFolderResponseDto>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());
                        if (dto != null)
                        {
                            size += dto.Size;
                        }

                        items.Add(dto!.Items);
                    }
                }

                var appDir = _iniSettings.DownloadLocation;
                var disk = appDir!.Substring(0, appDir.IndexOf(':'));
                var freeSize = _fileHelper.GetHardDiskFreeSpace(disk);
                if (freeSize < size)
                {
                    _growlHelper.Warning("磁盘空间不够");
                    return;
                }

                //创建文件夹
                foreach (var item in items)
                {
                    var folderDtos = item.Where(x => x.IsFolder);
                    CreateFolders(folderDtos.FirstOrDefault(x => item.FirstOrDefault(y => y.Id == x.ParentFolder) == null), null, item);
                }

                List<(long id, string downloadLocation)> downloads = new List<(long, string)>();
                downloads.AddRange(files.Where(x => !x.IsFolder).Select(x => (x.Id, _iniSettings.DownloadLocation)));
                foreach (var item in items)
                {
                    downloads.AddRange(item.Where(x => !x.IsFolder).Select(x => (x.Id, x.LocalLocation)));
                }

                foreach (var download in downloads)
                {
                    await DownloadFileAsync(download.id, download.downloadLocation);
                }
            }
        }

        private void CreateFolders(PreDownloadFolderFileItemResponseDto? current, PreDownloadFolderFileItemResponseDto? parent, IEnumerable<PreDownloadFolderFileItemResponseDto> folders)
        {
            if (current == null)
                return;

            var dir = current.Name.GetAvailableDirLocation(parent == null ? _iniSettings.DownloadLocation : parent.LocalLocation);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            current.LocalLocation = dir;
            var children = folders.Where(x => x.ParentFolder == current.Id);
            foreach (var child in children)
            {
                if (child.IsFolder)
                    CreateFolders(child, current, folders);
                else
                    child.LocalLocation = dir;
            }
        }

        private async Task UploadAsync()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog()!.Value)
            {
                foreach (var item in openFileDialog.FileNames)
                {
                    await UploadFileAsync(item, _currentParent);
                }
            }
        }

        public async Task UploadFileAsync(string file, long parentFolder)
        {
            var fileInfo = new FileInfo(file);
            using (FileStream stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                var code = stream.ToSHA256();
                var resp = await _httpRequest.PostAsync(Const.UPLOAD_FILE, new UploadFileDto()
                {
                    Name = fileInfo.Name,
                    Size = fileInfo.Length,
                    ParentFolder = parentFolder,
                    Code = code
                });

                if (resp != null && resp.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.
                        Deserialize<UploadFileResponseDto>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());
                    var record = new UploadFileRecord()
                    {
                        UserId = App.UserProfile.Id,
                        FileId = result.FileId,
                        FileName = fileInfo.Name,
                        Size = fileInfo.Length,
                        FileLocation = fileInfo.FullName,
                        TransferBytes = 0,
                        Status = FileUploadingStatus.Wait,
                        Code = code
                    };

                    _databaseHelper.Excute("INSERT INTO UploadFileRecords (Id,FileId,FileName,FileLocation,Size,TransferBytes,CreateTime,UploadFinishTime,Code,Status,UserId)" +
                        "VALUES(@Id,@FileId,@FileName,@FileLocation,@Size,@TransferBytes,@CreateTime,@UploadFinishTime,@Code,@Status,@UserId)", record);

                    WeakReferenceMessenger.Default.Send(record, Const.AddUploadRecord);
                }
            }
        }

        private async Task UploadFolderAsync(string folder, long parentFolder)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folder);
            FolderTreeCreationDto folderTreeCreationDto = new FolderTreeCreationDto();
            folderTreeCreationDto.ParentId = parentFolder;
            var root = new FolderTreeCreationItemDto()
            {
                FolderName = directoryInfo.Name,
                Location = directoryInfo.FullName,
                ParentClientId = string.Empty
            };
            GetSubFolders(root, directoryInfo, folderTreeCreationDto.Items);
            if (folderTreeCreationDto.Items.Count > 512)
            {
                _growlHelper.Warning($"{folder}上传失败,目录或者目录层级过多，建议拆分上传");
            }
            var resp = await _httpRequest.PostAsync(Const.CREATE_MULTI_FOLDER, folderTreeCreationDto);
            if (resp != null)
            {
                var results =
                    JsonSerializer.Deserialize<IEnumerable<FolderTreeCreationResponseDto>>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());
                foreach (var item in results)
                {
                    folderTreeCreationDto.Items.FirstOrDefault(x => x.ClientId == item.ClientId)!.RemoteId = item.RemoteId;
                }

                foreach (var item in folderTreeCreationDto.Items)
                {
                    foreach (var item1 in item.Files)
                    {
                        await UploadFileAsync(item1, item.RemoteId);
                    }
                }
            }
        }

        private void GetSubFolders(FolderTreeCreationItemDto parent, DirectoryInfo directoryInfo, List<FolderTreeCreationItemDto> treeCreationDtoItems)
        {
            treeCreationDtoItems.Add(parent);
            foreach (var item in directoryInfo.GetFiles())
            {
                parent.Files.Add(item.FullName);
            }
            foreach (var folder in directoryInfo.GetDirectories())
            {
                var newItem = new FolderTreeCreationItemDto()
                {
                    Location = folder.FullName,
                    FolderName = folder.Name,
                };
                newItem.ParentClientId = parent.ClientId;

                GetSubFolders(newItem, folder, treeCreationDtoItems);
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="fileId">文件id</param>
        /// <param name="location">下载位置</param>
        /// <returns></returns>
        private async Task DownloadFileAsync(long fileId, string location, bool openWhenFinished = false)
        {
            var resp = await _httpRequest.PostAsync(string.Format(Const.DOWNLOAD_FILE, fileId), null);
            if (resp != null)
            {
                var result = JsonSerializer.
                           Deserialize<DownloadFileResponseDto>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());

                await GenerateDownloadTaskAsync(result, location, openWhenFinished);
            }
        }

        /// <summary>
        /// 下载分享的文件
        /// </summary>
        /// <param name="fileId">分享token</param>
        /// <param name="location">下载位置</param>
        /// <returns></returns>
        private async Task DownloadSharedFileAsync(string token, string location, bool openWhenFinished = false)
        {
            var resp = await _httpRequest.PostAsync(string.Format(Const.DOWNLOAD_SHARED_FILE, token), null);
            if (resp != null)
            {
                var result = JsonSerializer.
                           Deserialize<DownloadFileResponseDto>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());

                await GenerateDownloadTaskAsync(result, location, openWhenFinished);
            }
        }

        /// <summary>
        /// 根据请求远端生成的task创建本地的下载任务
        /// </summary>
        /// <param name="result">请求数据</param>
        /// <param name="location">文件下载位置</param>
        /// <param name="openWhenFinished">下载后是否打开</param>
        /// <returns></returns>
        private async Task GenerateDownloadTaskAsync(DownloadFileResponseDto result, string location, bool openWhenFinished = false)
        {
            var tempFileName = Path.Combine(location, $"{Path.GetRandomFileName()}.tmp");
            File.Create(tempFileName).Close();
            var record = new DownloadFileRecord()
            {
                UserId = App.UserProfile.Id,
                FileId = result.FileId,
                TaskId = result.TaskId,
                TempFileLocation = tempFileName,
                FileName = result.FileName,
                TransferBytes = 0,
                Status = FileDownloadingStatus.Wait,
                Size = result.Size,
                Code = result.Code
            };

            _databaseHelper.Excute("INSERT INTO DownloadFileRecords (Id,TaskId,FileId,FileName,TempFileLocation,Location,Size,TransferBytes,CreateTime,DownloadFinishTime,Code,Status,UserId)" +
                "VALUES(@Id,@TaskId,@FileId,@FileName,@TempFileLocation,@Location,@Size,@TransferBytes,@CreateTime,@DownloadFinishTime,@Code,@Status,@UserId)", record);

            //发送添加下载任务事件
            WeakReferenceMessenger.Default.Send(new Tuple<DownloadFileRecord, bool>(record, openWhenFinished), Const.AddDownloadRecord);
        }
    }
}
