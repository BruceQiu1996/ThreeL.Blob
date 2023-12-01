using System;
using ThreeL.Blob.Clients.Win.ViewModels.Message;
using ThreeL.Blob.Shared.Domain.Metadata.Message;

namespace ThreeL.Blob.Clients.Win.Dtos.ChatServer
{
    public class ChatRecordResponseDto
    {
        public string MessageId { get; set; }
        public long From { get; set; }
        public long To { get; set; }
        public string Message { get; set; }
        public string? FileToken { get; set; }
        public long? Size { get; set; }
        public long? FileObjectId { get; set; }
        public DateTime RemoteSendTime { get; set; }
        public MessageType MessageType { get; set; }
        public bool Withdraw { get; set; }

        public MessageViewModel ToViewModel() 
        {
            MessageViewModel result = null;
            if (MessageType == MessageType.Text)
            {
                result = new TextMessageViewModel();
                result.FromChatRecord(this);
            }
            else if (MessageType == MessageType.File) 
            {
                result = new FileMessageViewModel();
                result.FromChatRecord(this);
            }

            return result;
        }
    }
}
