using System;
using ThreeL.Blob.Shared.Domain.Metadata.User;

namespace ThreeL.Blob.Clients.Win.ViewModels.Apply
{
    public class AddFriendApplyMessageViewModel : ApplyMessageViewModel
    {
        public long Activer { get; set; }
        public long Passiver { get; set; }
        public DateTime CreateTime { get; set; }
        public string ActiverName { get; set; }
        public string PassiverName { get; set; }
        public FriendApplyStatus Status { get; set; }
        public string Desc => App.UserProfile.Id == Activer ? $"添加{PassiverName}为好友" : $"{ActiverName}请求添加您为好友";
    }
}
