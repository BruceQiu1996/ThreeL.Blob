using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
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
        private FriendApplyStatus _status;
        public FriendApplyStatus Status 
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
        public bool FromSelf => App.UserProfile.Id == Activer;
        public string Desc => App.UserProfile.Id == Activer ? $"添加【{PassiverName}】为好友" : $"【{ActiverName}】请求添加您为好友";

        public AsyncRelayCommand AcceptApplyCommandAsync { get; set; }
        public AddFriendApplyMessageViewModel()
        {
            AcceptApplyCommandAsync = new AsyncRelayCommand(AcceptApplyAsync);
        }

        //TODO 自己请求的不能处理
        private async Task AcceptApplyAsync()
        {
            if (Status != FriendApplyStatus.Unhandled)
                return;

            var resp = await App.ServiceProvider.GetRequiredService<HttpRequest>().PostAsync(string.Format(Const.HANDLEAPPLY, Id, "accept"), null);
            if (resp != null)
            {
                Status = FriendApplyStatus.Agreed;
            }
        }
    }
}
