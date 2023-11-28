using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Dtos.Message;
using ThreeL.Blob.Shared.Domain;
using ThreeL.Blob.Shared.Domain.Metadata.Message;

namespace ThreeL.Blob.Clients.Win.ViewModels.Message
{
    public class MessageViewModel : ObservableObject
    {
        public string MessageId = Guid.NewGuid().ToString();
        public DateTime LocalSendTime { get; set; } = DateTime.Now;
        public DateTime? RemoteSendTime { get; set; }
        public long From { get; set; }
        public long To { get; set; }
        public bool FromSelf => App.UserProfile.Id == From;
        private bool sending;
        public bool Sending
        {
            get { return sending; }
            set => SetProperty(ref sending, value);
        }

        private bool sendFaild;
        public bool SendFaild
        {
            get { return sendFaild; }
            set => SetProperty(ref sendFaild, value);
        }

        private bool withdraw;
        public bool Withdraw
        {
            get { return withdraw; }
            set => SetProperty(ref withdraw, value);
        }

        public string WithdrawMessage => App.UserProfile.Id == From ? "您撤回了一条消息" : "对方撤回了一条消息";

        public MessageType MessageType { get; set; }

        public DateTime UsefulTime => RemoteSendTime == null ? LocalSendTime : RemoteSendTime.Value;

        public AsyncRelayCommand WithdrawCommandAsync { get; set; }
        public MessageViewModel(MessageType messageType)
        {
            MessageType = messageType;
            WithdrawCommandAsync = new AsyncRelayCommand(WithdrawAsync);
        }

        public virtual void ToDto(MessageDto messageDto)
        {
            messageDto.MessageId = MessageId;
            messageDto.LocalSendTime = LocalSendTime;
            messageDto.RemoteSendTime = RemoteSendTime;
            messageDto.From = From;
            messageDto.To = To;
        }

        public virtual void FromDto(MessageDto messageDto)
        {
            MessageId = messageDto.MessageId;
            LocalSendTime = messageDto.LocalSendTime;
            RemoteSendTime = messageDto.RemoteSendTime;
            From = messageDto.From;
            To = messageDto.To;
        }

        public async Task WithdrawAsync()
        {
            if (From != App.UserProfile.Id)
                return;

            await App.HubConnection.SendAsync(HubConst.SendWithdrawMessage, new WithdrawMessageDto()
            {
                MessageId = MessageId
            });
        }

        #region 菜单可见
        public virtual bool CanWithdraw => From == App.UserProfile.Id;
        public virtual bool CanDownload => MessageType == MessageType.File || MessageType == MessageType.Folder;
        public virtual bool CanOpenLocalLocation => MessageType == MessageType.File;
        public virtual bool CanSaveOnline => MessageType == MessageType.File || MessageType == MessageType.Folder;
        #endregion
    }
}
