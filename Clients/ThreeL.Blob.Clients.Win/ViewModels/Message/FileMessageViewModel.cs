using Microsoft.Extensions.DependencyInjection;
using System.Windows.Media.Imaging;
using ThreeL.Blob.Clients.Win.Dtos.ChatServer;
using ThreeL.Blob.Clients.Win.Dtos.Message;
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
        public BitmapImage Icon => App.ServiceProvider!.GetRequiredService<FileHelper>().GetIconByFileExtension(FileName).Item2;
        public long Size { get; set; }
        public string SizeText => Size.ToSizeText() ?? "未知";
        public string Token { get; set; }

        public override void ToDto(MessageDto messageDto)
        {
            base.ToDto(messageDto);
            (messageDto as FileMessageDto)!.FileObjectId = FileId;
        }

        public override void FromDto(MessageDto messageDto)
        {
            base.FromDto(messageDto);
            FileId = (messageDto as FileMessageResponseDto).FileObjectId;
            FileName = (messageDto as FileMessageResponseDto).FileName;
            Size = (messageDto as FileMessageResponseDto).Size;
            Token = (messageDto as FileMessageResponseDto).Token;
        }

        public override void FromChatRecord(ChatRecordResponseDto chatRecordResponseDto)
        {
            base.FromChatRecord(chatRecordResponseDto);
            FileId = chatRecordResponseDto.FileObjectId!.Value;
            FileName = chatRecordResponseDto.Message;
            Size = chatRecordResponseDto.Size!.Value;
            Token = chatRecordResponseDto.FileToken;
        }
    }
}
