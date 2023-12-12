using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using ThreeL.Blob.Clients.Win.Dtos.ChatServer;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Message;
using ThreeL.Blob.Clients.Win.ViewModels.Window;
using ThreeL.Blob.Clients.Win.Windows;
using ThreeL.Blob.Infra.Core.Extensions.System;
using ThreeL.Blob.Infra.Core.Serializers;

namespace ThreeL.Blob.Clients.Win.ViewModels.Item
{
    public class RelationItemViewModel : ObservableObject
    {
        public bool Loaded = false;
        public DateTime? LastFetchChatRecordsTime;
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

        private MessageViewModel previewMessage;
        public MessageViewModel PreviewMessage
        {
            get => previewMessage;
            set => SetProperty(ref previewMessage, value);
        }

        public bool IsGroup { get; set; }

        private int _unReadCount;
        public int UnReadCount 
        {
            get { return _unReadCount; }
            set => SetProperty(ref _unReadCount, value);
        }

        private string _unReadCountText;
        public string UnReadCountText
        {
            get { return _unReadCountText; }
            set => SetProperty(ref _unReadCountText, value);
        }

        public RelationItemViewModel()
        {
            Messages = new ObservableCollection<MessageViewModel>
            {
                new LoadRecordsViewModel()
            };
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

        public void AddMessage(MessageViewModel message)
        {
            Application.Current.Dispatcher.Invoke(() =>
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
                        Time = App.ServiceProvider.GetService<DateTimeHelper>().ConvertDateTimeToText(message.UsefulTime)
                    });

                if (LastMessage != null && LastMessage.UsefulTime.AddMinutes(5) <= message.UsefulTime)
                    Messages.Add(new TimeMessageViewModel()
                    {
                        Time = App.ServiceProvider.GetService<DateTimeHelper>().ConvertDateTimeToText(message.UsefulTime)
                    });

                Messages.Add(message);
                LastMessage = message;
                if (LastMessage.From == App.UserProfile.Id) //自己发送的则滚动条到最下面
                {
                    App.ServiceProvider.GetRequiredService<Chat>().chatScrollViewer.ScrollToEnd();
                }
                //收到消息，并且当前界面不是该人
                if (LastMessage.From != App.UserProfile.Id && App.ServiceProvider.GetRequiredService<ChatViewModel>().Relation != this)
                {
                    UnReadCount++;
                    if (UnReadCount <= 0)
                    {
                        UnReadCountText = null;
                    }
                    else
                    {
                        UnReadCountText = UnReadCount >= 100 ? "99+" : UnReadCount.ToString();
                    }
                }
            });
        }

        /// <summary>
        /// 插入聊天记录，到index = 1的位置
        /// </summary>
        /// <param name="message"></param>
        public void InsertMessageAtBegin(MessageViewModel message ,bool last = false)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Remove(message);
                var temp = Messages.FirstOrDefault(x => x.MessageId == message.MessageId);
                if (temp != null)
                {
                    Messages.Remove(temp);
                }

                if (PreviewMessage != null && PreviewMessage.UsefulTime.AddMinutes(-5) >= message.UsefulTime)
                    Messages.Insert(1, new TimeMessageViewModel()
                    {
                        Time = App.ServiceProvider.GetService<DateTimeHelper>().ConvertDateTimeToText(PreviewMessage.UsefulTime)
                    });

                Messages.Insert(1, message);
                PreviewMessage = message;

                if (last) 
                {
                    Messages.Insert(1, new TimeMessageViewModel()
                    {
                        Time = App.ServiceProvider.GetService<DateTimeHelper>().ConvertDateTimeToText(message.UsefulTime)
                    });
                }
            });
        }

        public async Task FetchHistoryChatRecordsAsync()
        {
            var loadMsg = Messages.FirstOrDefault(x => x is LoadRecordsViewModel) as LoadRecordsViewModel;
            try
            {
                loadMsg.IsDisabled = true;//防止重复拉取
                var time = LastFetchChatRecordsTime == null ? DateTime.Now : LastFetchChatRecordsTime.Value;
                var resp = await App.ServiceProvider.GetRequiredService<ChatHttpRequest>().GetAsync(string.Format(Const.CHAT_RECORDS, Id,
                    time.ToLong()));
                if (resp != null)
                {
                    var result = JsonSerializer
                        .Deserialize<QueryChatRecordResponseDto>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());

                    if (result.Records.Count() > 0)
                    {
                        foreach (var record in result.Records)
                        {
                            InsertMessageAtBegin(record.ToViewModel(), record == result.Records.Last());
                        }

                        loadMsg.IsDisabled = false;
                        LastFetchChatRecordsTime = result.Records.Min(x => x.RemoteSendTime);
                    }
                    else 
                    {
                        App.ServiceProvider.GetRequiredService<GrowlHelper>().InfoGlobal($"没有更多聊天记录");
                    }

                    Loaded = true;
                }
            }
            catch (Exception ex) 
            {
                loadMsg.IsDisabled = false;
                App.ServiceProvider.GetRequiredService<GrowlHelper>().WarningGlobal($"拉取历史聊天记录出错:{ex.Message}");
            }
        }

        public void UpdateMessage(MessageViewModel message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var index = Messages.IndexOf(message);
                Messages.Remove(message);
                Messages.Insert(index, message);
            });
        }


        public void Withdraw(string messageId)
        {
            var message = Messages.FirstOrDefault(x => x.MessageId == messageId);

            if (message != null)
            {
                message.Withdraw = true;
                UpdateMessage(message);
            }
        }
    }
}
