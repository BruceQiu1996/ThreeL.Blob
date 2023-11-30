using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Dtos;
using ThreeL.Blob.Clients.Win.Entities;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Infra.Core.Serializers;

namespace ThreeL.Blob.Clients.Win.ViewModels
{
    public class LoginWindowViewModel : ObservableObject
    {
        public string _userName;
        public string UserName
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value); }
        }
        public AsyncRelayCommand<PasswordBox> LoginCommandAsync { get; set; }
        public AsyncRelayCommand<PasswordBox> LoadedCommandAsync { get; set; }

        private readonly ApiHttpRequest _httpRequest;
        private readonly ChatHttpRequest _chatHttpRequest;
        private readonly GrpcService _grpcService;
        private readonly GrowlHelper _growlHelper;
        private readonly IMapper _mapper;
        private readonly IniSettings _iniSettings;
        private readonly EncryptHelper _encryptHelper;
        public LoginWindowViewModel(ApiHttpRequest httpRequest, ChatHttpRequest chatHttpRequest, GrowlHelper growlHelper, IMapper mapper, GrpcService grpcService, IniSettings iniSettings, EncryptHelper encryptHelper)
        {
            _httpRequest = httpRequest;
            _chatHttpRequest = chatHttpRequest;
            _growlHelper = growlHelper;
            _mapper = mapper;
            httpRequest.ExcuteWhileBadRequest += _growlHelper.Warning;
            httpRequest.ExcuteWhileInternalServerError += _growlHelper.Warning;
            httpRequest.TryRefreshToken = RefreshTokenAsync;
            chatHttpRequest.TryRefreshToken = RefreshTokenAsync;
            LoginCommandAsync = new AsyncRelayCommand<PasswordBox>(LoginAsync);
            LoadedCommandAsync = new AsyncRelayCommand<PasswordBox>(LoadedAsync);
            _grpcService = grpcService;
            _iniSettings = iniSettings;
            _encryptHelper = encryptHelper;
        }

        private async Task LoginAsync(PasswordBox password)
        {
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(password.Password))
                return;

            var resp = await _httpRequest.PostAsync(Const.LOGIN, new UserLoginDto
            {
                UserName = UserName,
                Password = password.Password,
                Origin = "win"
            });

            if (resp != null)
            {
                var data = JsonSerializer.Deserialize<UserLoginResponseDto>(await resp.Content.ReadAsStringAsync(),
                    SystemTextJsonSerializer.GetDefaultOptions());
                _httpRequest.SetToken(data.AccessToken);
                _grpcService.SetToken(data.AccessToken);
                _chatHttpRequest.SetToken(data.AccessToken);
                App.UserProfile = _mapper.Map<UserProfile>(data);
                App.ServiceProvider.GetRequiredService<LoginWindow>().Hide();
                App.ServiceProvider.GetRequiredService<MainWindow>().Show();
                string mockTime = Guid.NewGuid().ToString("N");
                var enPwd = _encryptHelper.Encrypt(password.Password);
                await _iniSettings.WriteUserInfo(UserName, enPwd, mockTime);
            }
        }

        private async Task LoadedAsync(PasswordBox passwordBox)
        {
            if (string.IsNullOrEmpty(_iniSettings.UserName))
                return;

            try
            {
                var enPwd = _iniSettings.Password;
                var password = _encryptHelper.Decrypt(enPwd);
                UserName = _iniSettings.UserName;
                passwordBox.Password = password;
            }
            catch { }
        }

        //刷新token
        private async Task<bool> RefreshTokenAsync()
        {
            if (string.IsNullOrEmpty(App.UserProfile?.RefreshToken) || string.IsNullOrEmpty(App.UserProfile?.AccessToken))
            {
                return false;
            }

            var token = await _httpRequest.RefreshTokenAsync(new UserRefreshTokenDto
            {
                Origin = "win",
                AccessToken = App.UserProfile.AccessToken,
                RefreshToken = App.UserProfile.RefreshToken
            });

            if (token == null)
            {
                return false;
            }

            _httpRequest.SetToken(token.AccessToken);
            _grpcService.SetToken(token.AccessToken);
            _chatHttpRequest.SetToken(token.AccessToken);
            App.UserProfile.RefreshToken = token.RefreshToken;
            App.UserProfile.AccessToken = token.AccessToken;

            return true;
        }
    }
}
