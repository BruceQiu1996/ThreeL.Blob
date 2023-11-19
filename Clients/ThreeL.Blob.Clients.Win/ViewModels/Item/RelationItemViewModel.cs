using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Message;
using ThreeL.Blob.Clients.Win.Windows;

namespace ThreeL.Blob.Clients.Win.ViewModels.Item
{
    public class RelationItemViewModel : ObservableObject
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

        private BitmapImage _icon;
        public BitmapImage Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        private ObservableCollection<MessageViewModel> messages;
        public ObservableCollection<MessageViewModel> Messages
        {
            get => messages;
            set => SetProperty(ref messages, value);
        }

        private MessageViewModel lastMessage;
        public MessageViewModel LastMessage
        {
            get => lastMessage;
            set => SetProperty(ref lastMessage, value);
        }

        public bool IsGroup { get; set; }
        public RelationItemViewModel()
        {
            Messages = new ObservableCollection<MessageViewModel>();
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

        public void AddMessage(MessageViewModel message)
        {
            Messages.Remove(message);
            var temp = Messages.FirstOrDefault(x => x.MessageId == message.MessageId);
            if (temp != null)
            {
                Messages.Remove(temp);
            }
            if (LastMessage == null)
                Messages.Add(new TimeMessageViewModel()
                {
                    Time = App.ServiceProvider.GetService<DateTimeHelper>().ConvertDateTimeToText(message.RemoteTime)
                });

            if (LastMessage != null && LastMessage.RemoteTime.AddMinutes(5) <= message.RemoteTime)
                Messages.Add(new TimeMessageViewModel()
                {
                    Time = App.ServiceProvider.GetService<DateTimeHelper>().ConvertDateTimeToText(message.RemoteTime)
                });

            Messages.Add(message);
            LastMessage = message;
            App.ServiceProvider.GetRequiredService<Chat>().chatScrollViewer.ScrollToEnd();
        }
    }
}
