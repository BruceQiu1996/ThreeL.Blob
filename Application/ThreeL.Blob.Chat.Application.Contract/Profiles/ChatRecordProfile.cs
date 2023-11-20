﻿using AutoMapper;
using ThreeL.Blob.Chat.Application.Contract.Dtos;
using ThreeL.Blob.Chat.Domain.Entities;

namespace ThreeL.Blob.Chat.Application.Contract.Profiles
{
    public class ChatRecordProfile : Profile
    {
        public ChatRecordProfile()
        {
            CreateMap<TextMessageDto, ChatRecord>().ForMember(x => x.Id, opt => 
            {
                opt.Ignore();
            }).ForMember(x => x.Message, y => 
            {
                y.MapFrom(x => x.Text);
            }).AfterMap((x, y) => 
            {
                y.MessageType = Shared.Domain.Metadata.Message.MessageType.Text;
            });
        }
    }
}