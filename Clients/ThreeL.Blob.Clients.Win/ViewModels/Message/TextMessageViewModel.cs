﻿using ThreeL.Blob.Clients.Win.Dtos.ChatServer;
using ThreeL.Blob.Clients.Win.Dtos.Message;
using ThreeL.Blob.Shared.Domain.Metadata.Message;

namespace ThreeL.Blob.Clients.Win.ViewModels.Message
{
    public class TextMessageViewModel : MessageViewModel
    {
        public TextMessageViewModel() : base(MessageType.Text)
        {
        }

        public string Text { get; set; }

        public override void ToDto(MessageDto messageDto)
        {
            base.ToDto(messageDto);
            (messageDto as TextMessageDto)!.Text = Text;
        }

        public override void FromDto(MessageDto messageDto)
        {
            base.FromDto(messageDto);
            Text = (messageDto as TextMessageDto)!.Text;
        }

        public override void FromChatRecord(ChatRecordResponseDto chatRecordResponse)
        {
            base.FromChatRecord(chatRecordResponse);
            Text = chatRecordResponse.Message;
        }
    }
}
