using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Blob.Clients.Win.Dtos;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Item;
using ThreeL.Blob.Infra.Core.Serializers;

namespace ThreeL.Blob.Clients.Win.ViewModels.Window
{
    public class MoveViewModel : ObservableObject
    {
        public RelayCommand<RoutedPropertyChangedEventArgs<object>> SelectedItemChangedCommand { get; set; }
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public RelayCommand ConfirmMoveCommand { get; set; }
        private ObservableCollection<TreeViewFolderViewModel> _folderTreeItems;
        public ObservableCollection<TreeViewFolderViewModel> FolderTreeItems 
        {
            get { return _folderTreeItems; }
            set { SetProperty(ref _folderTreeItems, value); }
        }

        private TreeViewFolderViewModel _current;
        private readonly HttpRequest _httpRequest;
        private readonly GrowlHelper _growlHelper;
        public MoveViewModel(HttpRequest httpRequest, GrowlHelper growlHelper)
        {
            _httpRequest = httpRequest;
            _growlHelper = growlHelper;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            ConfirmMoveCommand = new RelayCommand(ConfirmMove);
            SelectedItemChangedCommand = new RelayCommand<RoutedPropertyChangedEventArgs<object>>(SelectedItemChanged);
        }

        private async Task LoadAsync()
        {
            try
            {
                var root = new TreeViewFolderViewModel()
                {
                    Name = "我的网盘",
                    Id = 0
                };
                FolderTreeItems = new ObservableCollection<TreeViewFolderViewModel>
                {
                    root
                };

                var resp = await _httpRequest.GetAsync(Const.Get_FOLDERS);
                if (resp != null)
                {
                    var items = JsonSerializer
                        .Deserialize<IEnumerable<FolderSimpleDto>>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());

                    if (items != null)
                    {
                        InsertItems(items, root);
                    }
                }
            }
            catch (Exception ex)
            {
                _growlHelper.Warning("加载列表失败");
            }
        }

        private void SelectedItemChanged(RoutedPropertyChangedEventArgs<object> eventArgs) 
        {
            _current = eventArgs.NewValue as TreeViewFolderViewModel;
        }

        private void ConfirmMove() 
        {
            WeakReferenceMessenger.Default.Send(_current, Const.ConfirmMove);
        }

        private void InsertItems(IEnumerable<FolderSimpleDto> folderSimpleDtos, TreeViewFolderViewModel parent) 
        {
            var childs = folderSimpleDtos.Where(x => x.ParentId == parent.Id).Select(x => new TreeViewFolderViewModel()
            {
                Id = x.Id,
                Name = x.Name,
            });

            foreach (var item in childs)
            {
                parent.Childs.Add(item);
                InsertItems(folderSimpleDtos, item);
            }
        }
    }
}
