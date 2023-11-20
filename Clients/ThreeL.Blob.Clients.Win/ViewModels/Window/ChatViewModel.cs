using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Configurations;
using ThreeL.Blob.Clients.Win.Dtos;
using ThreeL.Blob.Clients.Win.Dtos.Message;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Item;
using ThreeL.Blob.Clients.Win.ViewModels.Message;
using ThreeL.Blob.Infra.Core.Serializers;

namespace ThreeL.Blob.Clients.Win.ViewModels.Window
{
    public class ChatViewModel : ObservableObject
    {
        private bool _loaded;

        private string _textMessage;
        public string TextMessage
        {
            get => _textMessage;
            set => SetProperty(ref _textMessage, value);
        }

        private string _keyword;
        public string Keyword
        {
            get => _keyword;
            set => SetProperty(ref _keyword, value);
        }

        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand SendTextMessageCommandAsync { get; set; }
        public AsyncRelayCommand SearchUsersCommandAsync { get; set; }
        public ObservableCollection<RelationItemViewModel> Relations { get; set; }
        private RelationItemViewModel? _relation;
        public RelationItemViewModel? Relation
        {
            get => _relation;
            set => SetProperty(ref _relation, value);
        }
        private readonly HttpRequest _httpRequest;
        private readonly RemoteOptions _remoteOptions;
        private readonly GrowlHelper _growlHelper;
        private readonly IMapper _mapper;
        public ChatViewModel(HttpRequest httpRequest, IOptions<RemoteOptions> remoteOptions, GrowlHelper growlHelper, IMapper mapper)
        {
            _mapper = mapper;
            _httpRequest = httpRequest;
            _remoteOptions = remoteOptions.Value;
            _growlHelper = growlHelper;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            SearchUsersCommandAsync = new AsyncRelayCommand(SearchUsersAsync);
            SendTextMessageCommandAsync = new AsyncRelayCommand(SendTextMessageAsync);
            Relations = new ObservableCollection<RelationItemViewModel>();
        }

        private async Task LoadAsync()
        {
            if (!_loaded) 
            {
                App.HubConnection = new HubConnectionBuilder().WithUrl($"http://{_remoteOptions.Host}:{_remoteOptions.ChatPort}/Chat", option =>
                {
                    option.CloseTimeout = TimeSpan.FromSeconds(60);
                    option.AccessTokenProvider = () => Task.FromResult(_httpRequest._token)!;
                }).WithAutomaticReconnect().Build();

                App.HubConnection.On("LoginSuccess", () =>
                {
                    _growlHelper.Success("登录聊天系统成功");
                });

                App.HubConnection.On<UserSendTextMessageToUserResultDto>("ReceiveTextMessage", msg =>
                {
                    if (msg.Success) 
                    {
                        var relation = Relations.FirstOrDefault(x => x.Id == msg.Message.From || x.Id == msg.Message.To);
                        if (relation != null) 
                        {
                            var message = relation.Messages.FirstOrDefault(x => x.MessageId == msg.Message.MessageId);
                            var vm = new TextMessageViewModel();
                            vm.FromDto(msg.Message);
                            relation.AddMessage(vm);
                        }
                    }
                });

                await App.HubConnection.StartAsync();

                //拉取关系
                var resp = await _httpRequest.GetAsync(Const.RELATIONS);
                if (resp != null) 
                {
                    var result = JsonSerializer
                        .Deserialize<IEnumerable<RelationBriefDto>>(await resp.Content.ReadAsStringAsync(),SystemTextJsonSerializer.GetDefaultOptions());

                    if (result != null && result.Count() > 0) 
                    {
                        foreach (var relationBriefDto in result) 
                        {
                            Relations.Add(_mapper.Map<RelationItemViewModel>(relationBriefDto));
                        }

                        Relation = Relations.First();
                    }
                    _loaded = true;
                }
            }
        }

        private async Task SearchUsersAsync() 
        {
            var resp = await _httpRequest.GetAsync(string.Format(Const.QUERYRELATIONS,Keyword));
        }

        public async Task SendTextMessageAsync() 
        {
            var temp = Relation;
            if (temp == null || string.IsNullOrEmpty(TextMessage))
                return;

            var msg = new TextMessageViewModel()
            {
                Text = TextMessage,
                LocalSendTime = DateTime.Now,
                From = App.UserProfile.Id,
                To = temp.Id,
                Sending = true
            };
            temp.Messages.Add(msg);
            var dto = new TextMessageDto();
            msg.ToDto(dto);

            await App.HubConnection.SendAsync("SendTextMessage", dto);
        }
    }
}
