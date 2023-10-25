using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace ThreeL.Blob.Clients.Win.Helpers
{
    public class FileHelper
    {
        public BitmapImage GetIconByFileExtension(string fileName)
        {
            var extensionName = Path.GetExtension(fileName)?.ToLower();
            var icon = string.Empty;
            switch (extensionName)
            {
                case ".zip":
                case ".7z":
                case ".rar":
                    icon = "zip.png";
                    break;
                case ".dll":
                    icon = "dll.png";
                    break;
                case ".doc":
                case ".docx":
                    icon = "word.png";
                    break;
                case ".mp4":
                case ".mov":
                case ".avi":
                case ".flv":
                case ".wmv":
                case ".mpeg":
                case ".mkv":
                case ".asf":
                case ".rmvb":
                    icon = "vedio.png";
                    break;
                case ".exe":
                case ".msi":
                    icon = "exe.png";
                    break;
                case ".sql":
                    icon = "sql.png";
                    break;
                case ".xbm":
                case ".bmp":
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".tif":
                case ".tiff":
                case ".ico":
                    icon = "image.png";
                    break;
                case ".pdf":
                    icon = "pdf.png";
                    break;
                default:
                    icon = "unknown.png";
                    break;
            }

            return GetBitmapImageByFileExtension(icon);
        }

        public BitmapImage GetBitmapImageByFileExtension(string imageName)
        {
            var source = new BitmapImage();
            try
            {
                string imgUrl = $"pack://application:,,,/ThreeL.Blob.Clients.Win;component/Images/{imageName}";
                source.BeginInit();
                source.UriSource = new Uri(imgUrl, UriKind.RelativeOrAbsolute);
                source.EndInit();

                return source;
            }
            finally
            {
                source.Freeze();
            }
        }
    }
}
