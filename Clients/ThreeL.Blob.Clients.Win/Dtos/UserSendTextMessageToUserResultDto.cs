using ThreeL.Blob.Clients.Win.Dtos.Message;

namespace ThreeL.Blob.Clients.Win.Dtos
{
    public class UserSendTextMessageToUserResultDto
    {
        public string Description { get; set; }
        public bool Success { get; set; }
        public MessageDto Message { get; set; }
    }
}
