using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;

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


        public bool IsGroup { get; set; }
        public AsyncRelayCommand AddRelationCommandAsync { get; set; }

        public UnRelationItemViewModel()
        {
            AddRelationCommandAsync = new AsyncRelayCommand(AddRelationAsync);
        }

        public void GetAvatarImage(string avatar)
        {
            var _ = Task.Run(async () =>
            {
                var avatarResp = await App.ServiceProvider!.GetRequiredService<HttpRequest>().GetAsync(string.Format(Const.GET_AVATAR_IMAGE, avatar.Replace("\\", "/")));
                if (avatarResp != null)
                {
                    Icon = App.ServiceProvider!.GetRequiredService<FileHelper>().BytesToImage(await avatarResp.Content.ReadAsByteArrayAsync());
                }
            });
        }

        private async Task AddRelationAsync() 
        {
            CanAdd = false;
            var resp = await App.ServiceProvider!.GetRequiredService<HttpRequest>().PostAsync(string.Format(Const.ADDFRIEND, Id),null);
            if (resp == null)
            {
                CanAdd = true;
            }
            else 
            {
                App.ServiceProvider!.GetRequiredService<GrowlHelper>().SuccessGlobal("好友添加申请已发送");
            }
        }
    }
}
