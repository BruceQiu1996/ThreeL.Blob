using Microsoft.Extensions.DependencyInjection;
using System.Windows.Media.Imaging;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Infra.Core.Extensions.System;
using ThreeL.Blob.Shared.Domain.Metadata.Message;

namespace ThreeL.Blob.Clients.Win.ViewModels.Message
{
    public class FileMessageViewModel : MessageViewModel
    {
        public FileMessageViewModel() : base(MessageType.File)
        {
        }

        public long FileId { get; set; }
        public string FileName { get; set; }
        public BitmapImage Icon  => App.ServiceProvider!.GetRequiredService<FileHelper>().GetIconByFileExtension(FileName).Item2;
        public long Size { get; set; }
        public string SizeText => Size.ToSizeText() ?? "未知";
    }
}
