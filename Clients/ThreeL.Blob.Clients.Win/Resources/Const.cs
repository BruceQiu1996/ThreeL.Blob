namespace ThreeL.Blob.Clients.Win.Resources
{
    public class Const
    {
        public const string LOGIN = "user/login";
        public const string REFRESH_TOKEN = "user/refresh/token";
        public const string UPLOAD_FILE = "file";
        public const string DOWNLOAD_FILE = "file/download/{0}";
        public const string CREATE_FOLDER = "file/folder";
        public const string UPLOADING_STATUS = "file/upload-status/{0}";
        public const string DOWNLOADING_STATUS = "file/download-status/{0}";
        public const string UPLOADING_PAUSE = "file/upload-pause/{0}";
        public const string CANCEL_UPLOADING = "file/cancel/{0}";
        public const string CANCEL_DOWNLOADING = "file/cancel-download/{0}";

        //message key
        public const string AddUploadRecord = nameof(AddUploadRecord);
        public const string AddDownloadRecord = nameof(AddDownloadRecord);
        public const string SelectItem = nameof(SelectItem);
        public const string DoubleClickItem = nameof(DoubleClickItem);
        public const string Exit = nameof(Exit);
        public const string CanExit = nameof(CanExit);
        public const string UploadFinish = nameof(UploadFinish);
        public const string DownloadFinish = nameof(DownloadFinish);
        public const string NotifyUploadingCount = nameof(NotifyUploadingCount);
        public const string NotifyDownloadingCount = nameof(NotifyDownloadingCount);
        public const string AddTransferRecord = nameof(AddTransferRecord);
    }
}
