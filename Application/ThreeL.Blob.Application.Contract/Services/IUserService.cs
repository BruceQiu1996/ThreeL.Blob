﻿using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Shared.Application.Contract.Interceptors.Attributes;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Application.Contract.Services
{
    public interface IUserService
    {
        [Uow]
        Task<ServiceResult> CreateUserAsync(UserCreationDto creationDto, long creator);
        Task<ServiceResult<UserLoginResponseDto>> AccountLoginAsync(UserLoginDto userLoginDto);
    }
}