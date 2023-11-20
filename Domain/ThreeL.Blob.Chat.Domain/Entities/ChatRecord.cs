using ThreeL.Blob.Infra.Repository.Entities.Mongo;
using ThreeL.Blob.Shared.Domain.Metadata.Message;

namespace ThreeL.Blob.Chat.Domain.Entities
{
    public class ChatRecord : MongoEntity
    {
        public string MessageId { get; set; }
        public long From { get; set; }
        public long To { get; set; }
        public string Message { get; set; }
        public long? FileObjectId { get; set; }
        public DateTime LocalSendTime { get; set; }
        public DateTime RemoteSendTime { get; set; } = DateTime.Now;
        public MessageType MessageType { get; set; }
        public bool Withdraw { get; set; }
        public DateTime? WithdrawTime { get; set; }
    }
}
