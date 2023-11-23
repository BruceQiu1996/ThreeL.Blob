using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Dtos.ChatServer;
using ThreeL.Blob.Shared.Domain;
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
        public AsyncRelayCommand RejectApplyCommandAsync { get; set; }
        public AddFriendApplyMessageViewModel()
        {
            AcceptApplyCommandAsync = new AsyncRelayCommand(AcceptApplyAsync);
            RejectApplyCommandAsync = new AsyncRelayCommand(RejectApplyAsync);
        }

        //TODO 自己请求的不能处理
        private async Task AcceptApplyAsync()
        {
            if (Status != FriendApplyStatus.Unhandled)
                return;

            await HandleApplyAsync(true);
        }

        private async Task RejectApplyAsync()
        {
            if (Status != FriendApplyStatus.Unhandled)
                return;

            await HandleApplyAsync(false);
        }

        private async Task HandleApplyAsync(bool access)
        {
            await App.HubConnection.SendAsync(HubConst.HandleAddFriendApply, new HandleAddFriendApplyDto()
            {
                Access = access,
                ApplyId = Id!.Value
            });
        }
    }
}
