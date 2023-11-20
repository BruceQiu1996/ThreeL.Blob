﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using ThreeL.Blob.Clients.Win.Dtos.Message;
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

        public MessageType MessageType { get; set; }

        public DateTime UsefulTime => RemoteSendTime == null ? LocalSendTime : RemoteSendTime.Value;

        public MessageViewModel(MessageType messageType)
        {
            MessageType = messageType;
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
    }
}