using HandyControl.Controls;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace ThreeL.Blob.Clients.Win.Helpers
{
    public class FileHelper
    {
        public (int, BitmapImage) GetIconByFileExtension(string fileName, bool onlyType = false)
        {
            var extensionName = Path.GetExtension(fileName)?.ToLower();
            var icon = string.Empty;
            int key = int.MaxValue;
            switch (extensionName)
            {
                case ".zip":
                case ".7z":
                case ".rar":
                    icon = "zip.png";
                    key = 1;
                    break;
                case ".dll":
                    key = 2;
                    icon = "dll.png";
                    break;
                case ".doc":
                case ".docx":
                    key = 3;
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
                    key = 4;
                    icon = "vedio.png";
                    break;
                case ".exe":
                case ".msi":
                    key = 5;
                    icon = "exe.png";
                    break;
                case ".sql":
                    key = 6;
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
                    key = 7;
                    icon = "image.png";
                    break;
                case ".pdf":
                    key = 8;
                    icon = "pdf.png";
                    break;
                case ".html":
                    key = 9;
                    icon = "html.png";
                    break;
                case ".txt":
                    key = 10;
                    icon = "txt.png";
                    break;
                case ".ppt":
                case ".pptx":
                case ".potx":
                case ".pot":
                    key = 11;
                    icon = "ppt.png";
                    break;
                default:
                    icon = "unknown.png";
                    break;
            }

            if (onlyType)
            {
                return (key, null);
            }
            return (key, GetBitmapImageByFileExtension(icon));
        }

        public BitmapImage GetBitmapImageByFileExtension(string imageName)
        {
            var source = new BitmapImage();
            try
            {
                string imgUrl = $"pack://application:,,,/HeadDisk;component/Images/{imageName}";
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
        public long GetHardDiskSpace(string str_HardDiskName)
        {
            long totalSize = new long();
            str_HardDiskName = str_HardDiskName + ":\\";
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                if (drive.Name == str_HardDiskName)
                {
                    totalSize = drive.TotalSize;
                }
            }

            return totalSize;
        }

        public long GetHardDiskFreeSpace(string str_HardDiskName)
        {
            long freeSpace = new long();
            str_HardDiskName = str_HardDiskName + ":\\";
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                if (drive.Name == str_HardDiskName)
                {
                    freeSpace = drive.AvailableFreeSpace;
                }
            }

            return freeSpace;
        }

        public BitmapImage BytesToImage(byte[] bytes)
        {
            var source = new BitmapImage();
            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    source.BeginInit();
                    source.StreamSource = ms;
                    source.CacheOption = BitmapCacheOption.OnLoad;
                    source.EndInit();

                    return source;
                }
            }
            catch(Exception ex)
            {
                return null;
            }
            finally
            {
                source.Freeze();
            }
        }
    }
}
