using System;

namespace ThreeL.Blob.Clients.Win.Dtos.ChatServer
{
    public class QueryChatRecordsDto
    {
        public long Target { get; set; }
        public DateTime? LastTime { get; set; }
    }
}
