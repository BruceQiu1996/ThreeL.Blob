using Microsoft.Extensions.DependencyInjection;
using System.Windows.Media.Imaging;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Shared.Domain.Metadata.Message;

namespace ThreeL.Blob.Clients.Win.ViewModels.Message
{
    public class FolderMessageViewModel : MessageViewModel
    {
        public FolderMessageViewModel() : base(MessageType.Folder)
        {
        }

        public long FileId { get; set; }
        public string FileName { get; set; }
        public BitmapImage Icon => App.ServiceProvider!.GetRequiredService<FileHelper>().GetBitmapImageByFileExtension("folder.png");
    }
}
