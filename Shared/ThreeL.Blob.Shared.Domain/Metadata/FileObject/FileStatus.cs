namespace ThreeL.Blob.Shared.Domain.Metadata.FileObject
{
    /// <summary>
    /// for server
    /// </summary>
    public enum FileStatus
    {
        Wait = 1,
        Uploading = 2,
        UploadingSuspend = 3,
        Normal = 4,
        Faild = 5,
        Cancel = 6
    }

    /// <summary>
    /// client
    /// </summary>
    public enum FileUploadingStatus
    {
        Wait = 1,
        Uploading = 2,
        UploadingSuspend = 3,
        UploadingComplete = 4,
        UploadingFaild = 5,
    }

    public enum FileDownloadingStatus
    {
        Wait = 1,
        Downloading = 2,
        DownloadingSuspend = 3,
        DownloadingComplete = 4,
        DownloadingFaild = 5,
    }
}
