using AutoMapper;
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

            CreateMap<FileMessageDto, FileMessageResponseDto>();
            CreateMap<FileMessageResponseDto, ChatRecord>().ForMember(x => x.Id, opt =>
            {
                opt.Ignore();
            }).ForMember(x => x.Message, y =>
            {
                y.MapFrom(x => x.FileName);
            }).ForMember(x => x.FileToken, y =>
            {
                y.MapFrom(x => x.Token);
            }).AfterMap((x, y) =>
            {
                y.MessageType = Shared.Domain.Metadata.Message.MessageType.File;
            });

            CreateMap<FolderMessageDto, FolderMessageResponseDto>();
            CreateMap<ChatRecord, ChatRecordResponseDto>();
        }
    }
}
