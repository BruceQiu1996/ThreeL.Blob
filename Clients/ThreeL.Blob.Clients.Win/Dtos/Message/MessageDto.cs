using System;
using ThreeL.Blob.Shared.Domain.Metadata.Message;

namespace ThreeL.Blob.Clients.Win.Dtos.Message
{
    public class MessageDto
    {
        public string MessageId { get; set; }
        public DateTime LocalSendTime { get; set; }
        public DateTime RemoteTime { get; set; }
        public long From { get; set; }
        public long To { get; set; }
    }
}
