using Microsoft.Extensions.DependencyInjection;
using System.Windows.Media.Imaging;
using ThreeL.Blob.Clients.Win.Dtos.ChatServer;
using ThreeL.Blob.Clients.Win.Dtos.Message;
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
        public string Token { get; set; }
        public override void ToDto(MessageDto messageDto)
        {
            base.ToDto(messageDto);
            (messageDto as FolderMessageDto)!.FileObjectId = FileId;
        }

        public override void FromDto(MessageDto messageDto)
        {
            base.FromDto(messageDto);
            FileId = (messageDto as FileMessageResponseDto).FileObjectId;
            FileName = (messageDto as FileMessageResponseDto).FileName;
            Token = (messageDto as FileMessageResponseDto).Token;
        }

        public override void FromChatRecord(ChatRecordResponseDto chatRecordResponseDto)
        {
            base.FromChatRecord(chatRecordResponseDto);
            FileId = chatRecordResponseDto.FileObjectId!.Value;
            FileName = chatRecordResponseDto.Message;
            Token = chatRecordResponseDto.FileToken;
        }
    }
}
