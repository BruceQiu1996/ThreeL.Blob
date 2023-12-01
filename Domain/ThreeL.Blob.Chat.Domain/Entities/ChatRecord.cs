using MongoDB.Bson.Serialization.Attributes;
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
        public string? FileToken { get; set; }
        public long? Size { get; set; }
        public long? FileObjectId { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LocalSendTime { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime RemoteSendTime { get; set; }
        public MessageType MessageType { get; set; }
        public bool Withdraw { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? WithdrawTime { get; set; }
    }
}
