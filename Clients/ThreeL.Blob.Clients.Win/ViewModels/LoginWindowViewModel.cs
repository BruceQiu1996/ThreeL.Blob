﻿using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
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

        private readonly HttpRequest _httpRequest;
        private readonly GrowlHelper _growlHelper;
        private readonly IMapper _mapper;
        public LoginWindowViewModel(HttpRequest httpRequest, GrowlHelper growlHelper, IMapper mapper)
        {
            _httpRequest = httpRequest;
            _growlHelper = growlHelper;
            _mapper = mapper;
            httpRequest.ExcuteWhileBadRequest += _growlHelper.Warning;
            LoginCommandAsync = new AsyncRelayCommand<PasswordBox>(LoginAsync);
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
                _httpRequest.SetToken($"{data.AccessToken}");
                _mapper.Map(data, App.UserProfile);
                App.ServiceProvider.GetRequiredService<LoginWindow>().Hide();
                App.ServiceProvider.GetRequiredService<MainWindow>().Show();
            }
        }
    }
}
