using System.Collections.Generic;

namespace ThreeL.Blob.Clients.Win.Dtos.ChatServer
{
    public class QueryChatRecordResponseDto
    {
        public int Count { get; set; }
        public IEnumerable<ChatRecordResponseDto> Records { get; set; }
    }
}
