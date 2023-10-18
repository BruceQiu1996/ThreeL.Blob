using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ThreeL.Blob.Clients.Win.Dtos;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Infra.Core.Extensions.System;
using ThreeL.Blob.Infra.Core.Serializers;

namespace ThreeL.Blob.Clients.Win.ViewModels.Item
{
    public class FileObjItemViewModel : ObservableObject
    {
        public long Id { get; set; }
        public string Name
        {
            get;
            set;
        }
        public long? Size { get; set; }
        public long ParentFolder { get; set; }
        public DateTime CreateTime { get; set; }
        public bool IsFolder { get; set; }
        public BitmapImage Icon => IsFolder ? App.ServiceProvider!.GetRequiredService<FileHelper>().GetBitmapImageByFileExtension("folder.png")
            : App.ServiceProvider!.GetRequiredService<FileHelper>().GetIconByFileExtension(Name);
        public string SizeText => Size?.ToSizeText() ?? string.Empty;
        public string NameDesc => GetShortDesc();

        private bool _isRename;
        public bool IsRename
        {
            get => _isRename;
            set => SetProperty(ref _isRename, value);
        }

        private bool _isFocus;
        public bool IsFocus
        {
            get => _isFocus;
            set => SetProperty(ref _isFocus, value);
        }

        public AsyncRelayCommand RenameTextSubmitCommandAsync { get; set; }
        public RelayCommand<HandyControl.Controls.TextBox> TextboxGetFocusCommand { get; set; }
        public FileObjItemViewModel()
        {
            RenameTextSubmitCommandAsync = new AsyncRelayCommand(RenameTextSubmitAsync);
        }

        private void TextboxGetFocus(HandyControl.Controls.TextBox textBox) 
        {
            textBox.SelectAll();
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
                    CreateTime = data.CreateTime;
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
