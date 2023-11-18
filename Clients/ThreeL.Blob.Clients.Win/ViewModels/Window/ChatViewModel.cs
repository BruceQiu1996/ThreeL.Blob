using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Configurations;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Request;

namespace ThreeL.Blob.Clients.Win.ViewModels.Window
{
    public class ChatViewModel : ObservableObject
    {
        private bool _loaded;
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public ObservableCollection<string> Relations { get; set; }

        private readonly HttpRequest _httpRequest;
        private readonly RemoteOptions _remoteOptions;
        private readonly GrowlHelper _growlHelper;
        public ChatViewModel(HttpRequest httpRequest, IOptions<RemoteOptions> remoteOptions, GrowlHelper growlHelper)
        {
            _httpRequest = httpRequest;
            _remoteOptions = remoteOptions.Value;
            _growlHelper = growlHelper;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
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

                await App.HubConnection.StartAsync();

                _loaded = true;
            }
        }
    }
}
