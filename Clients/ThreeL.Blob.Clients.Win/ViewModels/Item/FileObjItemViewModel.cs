using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ThreeL.Blob.Clients.Win.Dtos;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Infra.Core.Extensions.System;
using ThreeL.Blob.Infra.Core.Serializers;

namespace ThreeL.Blob.Clients.Win.ViewModels.Item
{
    public class FileObjItemViewModel : ObservableObject
    {
        public long Id { get; set; }
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value);
                NameDesc = GetShortDesc();
            }
        }
        public long? Size { get; set; }
        public long ParentFolder { get; set; }
        public DateTime _createTime;
        public DateTime CreateTime
        {
            get => _createTime;
            set => SetProperty(ref _createTime, value);
        }
        private DateTime _lastUpdateTime;
        public DateTime LastUpdateTime
        {
            get => _lastUpdateTime;
            set => SetProperty(ref _lastUpdateTime, value);
        }
        public bool IsFolder { get; set; }
        public int Type { get; set; }
        private BitmapImage _icon;
        public BitmapImage Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }
        public string SizeText => Size?.ToSizeText() ?? "未知";
        private string _nameDesc;
        public string NameDesc
        {
            get => _nameDesc;
            set => SetProperty(ref _nameDesc, value);
        }

        private bool _isRename;
        public bool IsRename
        {
            get => _isRename;
            set => SetProperty(ref _isRename, value);
        }

        /// <summary>
        /// 是否聚焦到该item的重命名box
        /// </summary>
        private bool _isFocus;
        public bool IsFocus
        {
            get => _isFocus;
            set => SetProperty(ref _isFocus, value);
        }

        private bool _isHighlight;
        public bool IsHighlight
        {
            get => _isHighlight;
            set => SetProperty(ref _isHighlight, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                SetProperty(ref _isSelected, value);
                IsHighlight = value;
                WeakReferenceMessenger.Default.Send(this, Const.SelectItem);
            }
        }

        private bool _isUrlSelected;
        public bool IsUrlSelected
        {
            get => _isUrlSelected;
            set
            {
                SetProperty(ref _isUrlSelected, value);
            }
        }

        private bool _isThumbnailDisplay;
        public bool IsThumbnailDisplay
        {
            get => _isThumbnailDisplay;
            set
            {
                SetProperty(ref _isThumbnailDisplay, value);
            }
        }

        private string _thumbnailImageLocation;
        public string? ThumbnailImageLocation
        {
            get => _thumbnailImageLocation;
            set
            {
                _thumbnailImageLocation = value;
                if (!string.IsNullOrEmpty(value))
                {
                    GetThumbnailImage(value);
                }
            }
        }

        public AsyncRelayCommand RenameTextSubmitCommandAsync { get; set; }
        public RelayCommand<MouseButtonEventArgs> ClickFileObjectCommand { get; set; }
        public RelayCommand RightClickFileObjectCommand { get; set; }
        public RelayCommand ClickUrlObjectCommand { get; set; }
        public RelayCommand DragEnterFileObjectCommand { get; set; }
        public RelayCommand DragLeaveFileObjectCommand { get; set; }
        public RelayCommand DownloadCommand { get; set; }
        public RelayCommand DeleteCommand { get; set; }
        public RelayCommand SelectAllCommand { get; set; }
        public RelayCommand SelectNoCommand { get; set; }
        public RelayCommand RenameCommand { get; set; }
        public RelayCommand MoveCommand { get; set; }

        public FileObjItemViewModel()
        {
            RenameTextSubmitCommandAsync = new AsyncRelayCommand(RenameTextSubmitAsync);
            ClickFileObjectCommand = new RelayCommand<MouseButtonEventArgs>(ClickFileObject);
            RightClickFileObjectCommand = new RelayCommand(RightClickFileObject);
            ClickUrlObjectCommand = new RelayCommand(ClickUrlObject);
            DragEnterFileObjectCommand = new RelayCommand(DragEnterFileObject);
            DragLeaveFileObjectCommand = new RelayCommand(DragLeaveFileObject);
            DownloadCommand = new RelayCommand(() => { WeakReferenceMessenger.Default.Send(this, Const.MenuDownload); });
            DeleteCommand = new RelayCommand(() => { WeakReferenceMessenger.Default.Send(this, Const.MenuDelete); });
            SelectAllCommand = new RelayCommand(() => { WeakReferenceMessenger.Default.Send(this, Const.MenuSelectAll); });
            SelectNoCommand = new RelayCommand(() => { WeakReferenceMessenger.Default.Send(this, Const.MenuSelectNo); });
            RenameCommand = new RelayCommand(() => { WeakReferenceMessenger.Default.Send(this, Const.MenuRename); });
            MoveCommand = new RelayCommand(() => { WeakReferenceMessenger.Default.Send(this, Const.MenuMove); });
        }

        public void GetThumbnailImage(string thumbnailImage)
        {
            var _ = Task.Run(async () =>
            {
                var resp = await App.ServiceProvider.GetRequiredService<HttpRequest>()
                    .GetAsync(string.Format(Const.GET_THUMBNAIL_IMAGE, App.UserProfile.Id, thumbnailImage));

                if (resp != null && resp.IsSuccessStatusCode)
                {
                    var bytes = await resp.Content.ReadAsByteArrayAsync();
                    var source = new BitmapImage();
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(bytes))
                        {
                            source.BeginInit();
                            source.StreamSource = ms;
                            source.CacheOption = BitmapCacheOption.OnLoad;
                            source.EndInit();

                            Icon = source;
                        }
                    }
                    finally
                    {
                        source.Freeze();
                    }
                }
            });
        }

        private void ClickFileObject(MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                IsSelected = true;
            }
            else if (e.ClickCount > 1)
            {
                WeakReferenceMessenger.Default.Send(this, Const.DoubleClickItem);
            }
        }

        private void RightClickFileObject()
        {
            IsSelected = true;
        }

        private void ClickUrlObject()
        {
            WeakReferenceMessenger.Default.Send(this, Const.DoubleClickItem);
        }

        private void DragEnterFileObject()
        {
            IsHighlight = true;
        }


        private void DragLeaveFileObject()
        {
            IsHighlight = IsSelected;
        }

        public string GetShortDesc()
        {
            if (MeasureTextWidth(Name, 13, "微软雅黑") <= 95)
                return Name;

            string temp = Name.Substring(0, 6);
            foreach (var index in Enumerable.Range(7, Name.Length - 7))
            {
                string str = Name.Substring(0, index);
                var len = MeasureTextWidth(str, 13, "微软雅黑");
                if (len > 95)
                    break;

                temp = str;
            }

            return $"{temp}..";
        }

        private async Task RenameTextSubmitAsync()
        {
            if (IsFolder && Id == 0)
            {
                var resp = await App.ServiceProvider.GetRequiredService<HttpRequest>().PostAsync(Const.CREATE_FOLDER, new FolderCreationDto()
                {
                    FolderName = Name,
                    ParentId = ParentFolder
                });

                if (resp != null)
                {
                    var data = JsonSerializer.Deserialize<FileObjDto>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());
                    Id = data.Id;
                    Name = data.Name;
                    CreateTime = data.CreateTime;
                    IsRename = false;
                    LastUpdateTime = data.LastUpdateTime;
                }
            }
            else 
            {
                string name = Name;
                var resp = await App.ServiceProvider.GetRequiredService<HttpRequest>().PostAsync(Const.UPDATE_NAME, new UpdateFileObjectNameDto()
                {
                    Name = name,
                    FileId = Id
                });

                if (resp != null)
                {
                    Name = name;
                    IsRename = false;
                }
            }
        }

        private double MeasureTextWidth(string text, double fontSize, string fontFamily)
        {
            FormattedText formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(fontFamily.ToString()),
            fontSize,
            Brushes.Black
            );
            return formattedText.WidthIncludingTrailingWhitespace;
        }
    }
}
