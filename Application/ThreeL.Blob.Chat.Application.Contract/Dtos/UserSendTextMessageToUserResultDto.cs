﻿namespace ThreeL.Blob.Chat.Application.Contract.Dtos
{
    public class UserSendTextMessageToUserResultDto
    {
        public string Description { get; set; }
        public bool Success { get; set; }
        public MessageDto Message { get; set; }
    }
}
