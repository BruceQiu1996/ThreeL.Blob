﻿using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Blob.Clients.Win.Configurations;
using ThreeL.Blob.Clients.Win.Dtos;
using ThreeL.Blob.Clients.Win.Dtos.ChatServer;
using ThreeL.Blob.Clients.Win.Dtos.Message;
using ThreeL.Blob.Clients.Win.Helpers;
using ThreeL.Blob.Clients.Win.Request;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Apply;
using ThreeL.Blob.Clients.Win.ViewModels.Item;
using ThreeL.Blob.Clients.Win.ViewModels.Message;
using ThreeL.Blob.Infra.Core.Serializers;
using ThreeL.Blob.Shared.Domain;

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

        private bool _searching;
        public bool Searching
        {
            get => _searching;
            set => SetProperty(ref _searching, value);
        }

        private bool _hadFriends;
        public bool HadFriends
        {
            get => _hadFriends;
            set => SetProperty(ref _hadFriends, value);
        }

        private bool _hadSearchingResult;
        public bool HadSearchingResult
        {
            get => _hadSearchingResult;
            set => SetProperty(ref _hadSearchingResult, value);
        }

        private bool openApply;
        public bool OpenApply
        {
            get => openApply;
            set => SetProperty(ref openApply, value);
        }
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand SendTextMessageCommandAsync { get; set; }
        public AsyncRelayCommand SearchUsersCommandAsync { get; set; }
        public AsyncRelayCommand RefreshApplysCommandAsync { get; set; }
        public AsyncRelayCommand<DragEventArgs> DropItemsOnChatCommandAsync { get; set; }
        public AsyncRelayCommand ChangeRelationCommandAsync { get; set; }
        public ObservableCollection<RelationItemViewModel> Relations { get; set; }
        public ObservableCollection<UnRelationItemViewModel> UnRelations { get; set; }
        private ObservableCollection<ApplyMessageViewModel> _applys;
        public ObservableCollection<ApplyMessageViewModel> Applys
        {
            get => _applys;
            set => SetProperty(ref _applys, value);
        }
        public RelayCommand OpenApplyCommand { get; set; }

        private RelationItemViewModel? _relation;
        public RelationItemViewModel? Relation
        {
            get => _relation;
            set => SetProperty(ref _relation, value);
        }
        private readonly ApiHttpRequest _httpRequest;
        private readonly ChatHttpRequest _chatHttpRequest;
        private readonly RemoteOptions _remoteOptions;
        private readonly GrowlHelper _growlHelper;
        private readonly IMapper _mapper;
        public ChatViewModel(ApiHttpRequest httpRequest,
                             ChatHttpRequest chatHttpRequest,
                             IOptions<RemoteOptions> remoteOptions,
                             GrowlHelper growlHelper, IMapper mapper)
        {
            _mapper = mapper;
            _httpRequest = httpRequest;
            _remoteOptions = remoteOptions.Value;
            _chatHttpRequest = chatHttpRequest;
            _growlHelper = growlHelper;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            SearchUsersCommandAsync = new AsyncRelayCommand(SearchUsersAsync);
            SendTextMessageCommandAsync = new AsyncRelayCommand(SendTextMessageAsync);
            OpenApplyCommand = new RelayCommand(OpenApplyM);
            RefreshApplysCommandAsync = new AsyncRelayCommand(RefreshApplysAsync);
            ChangeRelationCommandAsync = new AsyncRelayCommand(ChangeRelationAsync);
            Relations = new ObservableCollection<RelationItemViewModel>();
            UnRelations = new ObservableCollection<UnRelationItemViewModel>();
            Applys = new ObservableCollection<ApplyMessageViewModel>();

            //被通知发送好友申请成功
            WeakReferenceMessenger.Default.Register<ChatViewModel, string, string>(this, Const.AddFriendApplySuccess, async (x, y) =>
            {
                await RefreshApplysAsync();
            });

            WeakReferenceMessenger.Default.Register<ChatViewModel, FileObjItemViewModel, string>(this, Const.SendFileObjectToChat, async (x, y) =>
            {
                if (Relations.Count <= 0 || Relation == null)
                {
                    return;
                }

                var type = y.IsFolder ? "文件夹" : "文件";
                var result = HandyControl.Controls.MessageBox.Ask($"确认发送{type}【{y.Name}】给好友【{Relation.UserName}】吗？(分享有效期为3天)", "询问");
                if (result == MessageBoxResult.OK)
                {
                    if (y.IsFolder)
                        await SendFolderMessageAsync(y);
                    else
                        await SendFileMessageAsync(y);
                }
            });

            //加载历史记录
            WeakReferenceMessenger.Default.Register<ChatViewModel, string, string>(this, Const.FetchHistoryChatRecords, async (x, y) =>
            {
                await FetcRelationhHistoryChatRecordsAsync(Relation);
            });
        }

        private async Task LoadAsync()
        {
            if (!_loaded)
            {
                App.HubConnection = new HubConnectionBuilder().WithUrl($"http://{_remoteOptions.ChatHost}:{_remoteOptions.ChatPort}/Chat", option =>
                {
                    option.CloseTimeout = TimeSpan.FromSeconds(60);
                    option.AccessTokenProvider = () => Task.FromResult(_httpRequest._token)!;
                }).WithAutomaticReconnect().Build();

                //登录成功回调
                App.HubConnection.On(HubConst.LoginSuccess, () =>
                {
                    _growlHelper.SuccessGlobal("登录聊天系统成功");
                });

                //发送文本消息回调
                App.HubConnection.On<HubMessageResponseDto<TextMessageDto>>(HubConst.ReceiveTextMessage, msg =>
                {
                    var relation = Relations.FirstOrDefault(x => x.Id == msg.Data.From || x.Id == msg.Data.To);
                    if (relation != null)
                    {
                        var vm = new TextMessageViewModel();
                        vm.FromDto(msg.Data);
                        vm.Sending = false;
                        vm.SendFaild = !msg.Success;
                        relation.AddMessage(vm);
                    }

                    if (!msg.Success) _growlHelper.WarningGlobal(msg.Message);
                });

                //发送文件消息回调
                App.HubConnection.On<HubMessageResponseDto<FileMessageResponseDto>>(HubConst.ReceiveFileMessage, msg =>
                {
                    var relation = Relations.FirstOrDefault(x => x.Id == msg.Data.From || x.Id == msg.Data.To);
                    if (relation != null)
                    {
                        var vm = new FileMessageViewModel();
                        vm.FromDto(msg.Data);
                        vm.Sending = false;
                        vm.SendFaild = !msg.Success;
                        relation.AddMessage(vm);
                    }

                    if (!msg.Success) _growlHelper.WarningGlobal(msg.Message);
                });

                //发送文件夹消息回调
                App.HubConnection.On<HubMessageResponseDto<FolderMessageResponseDto>>(HubConst.ReceiveFolderMessage, msg =>
                {
                    var relation = Relations.FirstOrDefault(x => x.Id == msg.Data.From || x.Id == msg.Data.To);
                    if (relation != null)
                    {
                        var vm = new FolderMessageViewModel();
                        vm.FromDto(msg.Data);
                        vm.Sending = false;
                        vm.SendFaild = !msg.Success;
                        relation.AddMessage(vm);
                    }

                    if (!msg.Success) _growlHelper.WarningGlobal(msg.Message);
                });

                //撤回消息回调
                App.HubConnection.On<HubMessageResponseDto<WithdrawMessageResponseDto>>(HubConst.ReceiveWithdrawMessage, msg =>
                {
                    var relation = Relations.FirstOrDefault(x => x.Id == msg.Data.From || x.Id == msg.Data.To);
                    if (relation != null)
                    {
                        relation.Withdraw(msg.Data.MessageId);
                    }

                    if (!msg.Success) _growlHelper.WarningGlobal(msg.Message);
                });

                //收到好友请求回调
                App.HubConnection.On<HubMessageResponseDto<object>>(HubConst.NewAddFriendApply, async msg =>
                {
                    if (msg.Success)
                    {
                        //刷新好友请求
                        await RefreshApplysAsync();
                    }
                    else
                    {
                        _growlHelper.WarningGlobal(msg.Message);
                    }
                });

                //处理好友请求回调
                App.HubConnection.On<HubMessageResponseDto<HandleAddFriendApplyResponseDto>>(HubConst.AddFriendApplyResult, async msg =>
                {
                    if (msg.Success)
                    {
                        await RefreshApplysAsync();
                        if (msg.Data.IsAgree && Relations.FirstOrDefault(x => x.Id == msg.Data.FriendId) == null)
                        {
                            //加载好友
                            var resp = await _httpRequest.GetAsync(string.Format(Const.RELATION_SOMEONE, msg.Data.FriendId));
                            if (resp != null)
                            {
                                var result = JsonSerializer
                                    .Deserialize<RelationBriefDto>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    AddRealtion(_mapper.Map<RelationItemViewModel>(result), true);
                                });

                                if (Relation == null) Relation = Relations.First();
                            }
                        }
                    }
                    else
                    {
                        _growlHelper.WarningGlobal(msg.Message);
                    }
                });

                await App.HubConnection.StartAsync();

                //拉取关系
                var resp = await _httpRequest.GetAsync(Const.RELATIONS);
                if (resp != null)
                {
                    var result = JsonSerializer
                        .Deserialize<IEnumerable<RelationBriefDto>>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());

                    if (result != null && result.Count() > 0)
                    {
                        foreach (var relationBriefDto in result)
                        {
                            AddRealtion(_mapper.Map<RelationItemViewModel>(relationBriefDto));
                        }

                        Relation = Relations.First();
                    }

                    _loaded = true;
                }

                //拉取申请
                await RefreshApplysAsync();
            }
        }

        public void AddRealtion(RelationItemViewModel relation, bool first = false)
        {
            if (Relations == null)
                return;

            if (first)
            {
                Relations.Insert(0, relation);
            }
            else
            {
                Relations.Add(relation);
            }

            HadFriends = Relations.Count > 0;
        }

        //拉取聊天记录
        private async Task ChangeRelationAsync()
        {
            if (Relation == null)
                return;

            if (!Relation.Loaded)
            {
                try
                {
                    await FetcRelationhHistoryChatRecordsAsync(Relation);
                }
                catch (Exception ex)
                {
                    _growlHelper.WarningGlobal(ex.Message);
                }
            }

            Relation.UnReadCount = 0;
            Relation.UnReadCountText = null;
        }

        private async Task FetcRelationhHistoryChatRecordsAsync(RelationItemViewModel relation)
        {
            await relation.FetchHistoryChatRecordsAsync();
        }

        private async Task RefreshApplysAsync()
        {
            var applyResp = await _httpRequest.GetAsync(Const.QUERYAPPLYS);
            if (applyResp != null)
            {
                var result = JsonSerializer
                    .Deserialize<IEnumerable<ApplyDto>>(await applyResp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());

                if (result != null && result.Count() > 0)
                {
                    var dates = result
                        .GroupBy(x => x.CreateTime.ToString("yyyy-MM-dd"));

                    List<ApplyMessageViewModel> applys = new List<ApplyMessageViewModel>();
                    foreach (var date in dates.OrderByDescending(x => x.Key))
                    {
                        applys.Add(new ApplyDateMessageViewModel()
                        {
                            CreateDate = date.Key,
                        });

                        foreach (var item in date.OrderBy(x => x.CreateTime))
                        {
                            applys.Add(_mapper.Map<AddFriendApplyMessageViewModel>(item));
                        }
                    }

                    Applys = new ObservableCollection<ApplyMessageViewModel>(applys);
                }
            }
        }

        private async Task SearchUsersAsync()
        {
            UnRelations.Clear();
            HadSearchingResult = false;
            if (string.IsNullOrEmpty(Keyword))
            {
                Searching = false;
                return;
            }
            Searching = true;
            try
            {
                var resp = await _httpRequest.GetAsync(string.Format(Const.QUERYRELATIONS, Keyword));
                if (resp != null)
                {
                    var result = JsonSerializer
                            .Deserialize<IEnumerable<RelationBriefDto>>(await resp.Content.ReadAsStringAsync(), SystemTextJsonSerializer.GetDefaultOptions());

                    if (result != null && result.Count() > 0)
                    {
                        foreach (var relationBriefDto in result)
                        {
                            UnRelations.Add(_mapper.Map<UnRelationItemViewModel>(relationBriefDto));
                        }

                        HadSearchingResult = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Searching = false;
            }
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
            temp.AddMessage(msg);
            var dto = new TextMessageDto();
            msg.ToDto(dto);

            await App.HubConnection.SendAsync(HubConst.SendTextMessage, dto);
            TextMessage = null;
        }

        public async Task SendFileMessageAsync(FileObjItemViewModel fileObjItemViewModel)
        {
            var temp = Relation;
            if (temp == null)
                return;

            var viewModel = fileObjItemViewModel.ToFileMessageVM(App.UserProfile.Id, temp.Id);
            temp.AddMessage(viewModel);
            var dto = new FileMessageDto();
            viewModel.ToDto(dto);

            await App.HubConnection.SendAsync(HubConst.SendFileMessage, dto);
        }

        public async Task SendFolderMessageAsync(FileObjItemViewModel fileObjItemViewModel)
        {
            _growlHelper.Warning("暂时不支持直接发送文件夹，请压缩成压缩文件后发送");
        }

        private void OpenApplyM()
        {
            OpenApply = true;
        }
    }
}
