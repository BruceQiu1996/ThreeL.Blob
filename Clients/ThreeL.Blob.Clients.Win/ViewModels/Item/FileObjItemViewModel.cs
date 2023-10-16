using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Infra.Core.Extensions.System;
using static System.Net.Mime.MediaTypeNames;

namespace ThreeL.Blob.Clients.Win.ViewModels.Item
{
    public class FileObjItemViewModel : ObservableObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long? Size { get; set; }
        public long? ParentFolder { get; set; }
        public DateTime CreateTime { get; set; }
        public bool IsFolder { get; set; }
        public BitmapImage Icon => App.ServiceProvider!.GetRequiredService<FileHelper>().GetIconByFileExtension(Name);
        public string SizeText => Size?.ToSizeText() ?? string.Empty;
        public string NameDesc => GetShortDesc();
        public FileObjItemViewModel()
        {
            
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
