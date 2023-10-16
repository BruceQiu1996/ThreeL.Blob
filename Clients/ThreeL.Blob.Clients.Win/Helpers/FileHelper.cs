using System.IO;

namespace ThreeL.Blob.Clients.Win.Helpers
{
    public class FileHelper
    {
        public string GetIconByFileExtension(string fileName)
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
                default:
                    icon = "unknown.png";
                    break;
            }

            return icon;
        }
    }
}
