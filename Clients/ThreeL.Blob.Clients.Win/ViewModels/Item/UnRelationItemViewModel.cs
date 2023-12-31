﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Shared.Domain;

namespace ThreeL.Blob.Clients.Win.ViewModels.Item
{
    public class UnRelationItemViewModel : ObservableObject
    {
        public long Id { get; set; }
        public string UserName { get; set; }

        private string _avatar;
        public string Avatar
        {
            get => _avatar;
            set
            {
                SetProperty(ref _avatar, value);
                if (value != null)
                {
                    GetAvatarImage(value);
                }
            }
        }

        private bool canAdd = true;
        public bool CanAdd
        {
            get => canAdd;
            set
            {
                SetProperty(ref canAdd, value);
            }
        }

        private BitmapImage _icon;
        public BitmapImage Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public bool MySelf => App.UserProfile.Id == Id;
        public bool IsGroup { get; set; }
        public AsyncRelayCommand AddRelationCommandAsync { get; set; }
        public AsyncRelayCommand RejectRelationCommandAsync { get; set; }

        public UnRelationItemViewModel()
        {
            AddRelationCommandAsync = new AsyncRelayCommand(AddRelationAsync);
        }

        public void GetAvatarImage(string avatar)
        {
            var _ = Task.Run(async () =>
            {
                var avatarResp = await App.ServiceProvider!.GetRequiredService<ApiHttpRequest>().GetAsync(string.Format(Const.GET_AVATAR_IMAGE, avatar.Replace("\\", "/")));
                if (avatarResp != null)
                {
                    Icon = App.ServiceProvider!.GetRequiredService<FileHelper>().BytesToImage(await avatarResp.Content.ReadAsByteArrayAsync());
                }
            });
        }

        private async Task AddRelationAsync() 
        {
            CanAdd = false;
            await App.HubConnection.SendAsync(HubConst.SendAddFriendApply,Id);
        }
    }
}
