using AutoMapper;
using ThreeL.Blob.Clients.Win.Entities;
using ThreeL.Blob.Clients.Win.ViewModels.Item;
using ThreeL.Blob.Shared.Domain.Metadata.FileObject;

namespace ThreeL.Blob.Clients.Win.Profiles
{
    public class TransferProfile : Profile
    {
        public TransferProfile()
        {
            CreateMap<UploadFileRecord, UploadItemViewModel>();
            CreateMap<UploadFileRecord, TransferCompleteRecord>().ForMember(x => x.BeginTime, y =>
            {
                y.MapFrom(x => x.CreateTime);
            }).ForMember(x => x.FinishTime, y =>
            {
                y.MapFrom(x => x.UploadFinishTime);
            }).ForMember(x => x.Id, y => y.Ignore());
            CreateMap<TransferCompleteRecord, TransferCompleteItemViewModel>();

            CreateMap<DownloadFileRecord, DownloadItemViewModel>();
        }

        public static string GetDescriptionByUploadStatus(FileUploadingStatus uploadingStatus)
        {
            if (uploadingStatus == FileUploadingStatus.UploadingComplete)
            {
                return "上传完成";
            }
            else if (uploadingStatus == FileUploadingStatus.UploadingFaild)
            {
                return "上传异常";
            }
            else
            {
                return "未知";
            }
        }

        public static string GetDescriptionByDownloadStatus(FileDownloadingStatus fileDownloadingStatus)
        {
            if (fileDownloadingStatus == FileDownloadingStatus.DownloadingComplete)
            {
                return "上传完成";
            }
            else if (fileDownloadingStatus == FileDownloadingStatus.DownloadingComplete)
            {
                return "上传异常";
            }
            else
            {
                return "未知";
            }
        }
    }
}
