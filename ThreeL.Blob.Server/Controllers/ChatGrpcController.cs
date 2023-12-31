﻿using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using ThreeL.Blob.Application.Contract.Protos;
using ThreeL.Blob.Application.Contract.Services;

namespace ThreeL.Blob.Server.Controllers
{
    public class ChatGrpcController : RpcContextAPIService.RpcContextAPIServiceBase
    {
        private readonly IRelationService _relationService;
        private readonly ILogger<ChatGrpcController> _logger;
        public ChatGrpcController(IRelationService relationService, ILogger<ChatGrpcController> logger)
        {
            _logger = logger;
            _relationService = relationService;
        }

        [Authorize]
        public async override Task<CommonResponse> AddFriendApply(AddFriendApplyRequest request, ServerCallContext context)
        {
            try
            {
                return await _relationService.AddFriendApplyAsync(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                return new CommonResponse()
                {
                    Success = false,
                    Message = "服务器错误"
                };
            }
        }

        [Authorize]
        public async override Task<HandleAddFriendApplyResponse> HandleAddFriendApply(HandleAddFriendApplyRequest request, ServerCallContext context)
        {
            try
            {
                return await _relationService.HandleAddFriendApplyAsync(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                return new HandleAddFriendApplyResponse()
                {
                    Success = false,
                    Message = "服务器错误"
                };
            }
        }

        [Authorize]
        public async override Task<SendFileResponse> SendFile(SendFileRequest request, ServerCallContext context)
        {
            try
            {
                return await _relationService.SendFileAsync(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                return new SendFileResponse()
                {
                    Success = false,
                    Message = "服务器错误"
                };
            }
        }

        [Authorize]
        public async override Task<SendFolderResponse> SendFolder(SendFolderRequest request, ServerCallContext context)
        {
            try
            {
                return await _relationService.SendFolderAsync(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                return new SendFolderResponse()
                {
                    Success = false,
                    Message = "服务器错误"
                };
            }
        }

        [Authorize]
        public async override Task<CancelSendFileResponse> CancelSendFile(CancelSendFileRequest request, ServerCallContext context)
        {
            try
            {
                return await _relationService.CancelSendFileAsync(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                return new CancelSendFileResponse()

                {
                    Success = false,
                    Message = "服务器错误"
                };
            }
        }
    }
}
