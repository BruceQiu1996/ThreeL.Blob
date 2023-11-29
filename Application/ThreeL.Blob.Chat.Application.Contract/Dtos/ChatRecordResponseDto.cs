using ThreeL.Blob.Shared.Domain.Metadata.Message;

namespace ThreeL.Blob.Chat.Application.Contract.Dtos
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
    }
}
