namespace ThreeL.Blob.Clients.Win.Resources
{
    public class Const
    {
        public const string LOGIN = "user/login";
        public const string MODIFY_PASSWORD = "user/password";
        public const string REFRESH_TOKEN = "user/refresh/token";
        public const string UPLOAD_FILE = "file";
        public const string Get_FOLDERS = "file/folders";
        public const string DELETE_FILE = "file/delete";
        public const string DOWNLOAD_FILE = "file/download/{0}";
        public const string CREATE_FOLDER = "file/folder";
        public const string CREATE_MULTI_FOLDER = "file/folders";
        public const string UPLOADING_STATUS = "file/upload-status/{0}";
        public const string DOWNLOADING_STATUS = "file/download-status/{0}";
        public const string UPLOADING_PAUSE = "file/upload-pause/{0}";
        public const string CANCEL_UPLOADING = "file/cancel/{0}";
        public const string CANCEL_DOWNLOADING = "file/cancel-download/{0}";
        public const string GET_THUMBNAIL_IMAGE = "thumbnailImages/{0}/{1}";
        public const string GET_AVATAR_IMAGE = "avatars{0}";
        public const string PRE_DOWNLOAD_FOLDER = "file/preDownloadFolder/{0}";
        public const string UPDATE_NAME = "file/update-name";
        public const string UPDATE_LOCATION = "file/update-location";
        public const string UPLOAD_AVATAR = "user/upload-avatar";

        //message key
        public const string AddUploadRecord = nameof(AddUploadRecord);
        public const string AddDownloadRecord = nameof(AddDownloadRecord);
        public const string SelectItem = nameof(SelectItem);
        public const string DoubleClickItem = nameof(DoubleClickItem);
        public const string Exit = nameof(Exit);
        public const string CanExit = nameof(CanExit);
        public const string ExitToLogin = nameof(ExitToLogin);
        public const string UploadFinish = nameof(UploadFinish);
        public const string DownloadFinish = nameof(DownloadFinish);
        public const string NotifyUploadingCount = nameof(NotifyUploadingCount);
        public const string NotifyDownloadingCount = nameof(NotifyDownloadingCount);
        public const string AddTransferRecord = nameof(AddTransferRecord);
        public const string ConfirmDownload = nameof(ConfirmDownload);
        public const string CancelDownload = nameof(CancelDownload);
        public const string StartNewUploadTask = nameof(StartNewUploadTask);
        public const string StartNewDownloadTask = nameof(StartNewDownloadTask);
        public const string AvatarUploaded = nameof(AvatarUploaded);
        public static object UploadRunTaskLock = new object();
        public static object DownloadRunTaskLock = new object();


        public const string MenuRefresh = nameof(MenuRefresh);
        public const string MenuNewFolder = nameof(MenuNewFolder);
        public const string MenuDownload = nameof(MenuDownload);
        public const string MenuDelete = nameof(MenuDelete);
        public const string MenuSelectAll = nameof(MenuSelectAll);
        public const string MenuSelectNo = nameof(MenuSelectNo);
        public const string MenuRename = nameof(MenuRename);
        public const string MenuMove = nameof(MenuMove);
        public const string ConfirmMove = nameof(ConfirmMove);
    }
}
