using System;
namespace ThreeL.Blob.Clients.Win.ViewModels.Message
{
    public class TimeMessageViewModel : MessageViewModel
    {
        public TimeMessageViewModel() : base(Shared.Domain.Metadata.Message.MessageType.Time)
        {
        }
        public string Time { get; set; }
    }
}
