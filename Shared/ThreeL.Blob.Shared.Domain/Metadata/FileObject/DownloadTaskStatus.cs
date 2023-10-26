namespace ThreeL.Blob.Shared.Domain.Metadata.FileObject
{
    public enum DownloadTaskStatus
    {
        Wait = 0,
        Downloading = 1,
        DownloadingSuspend = 2,
        Finished = 3,
        Failed = 4,
        Cancelled = 5
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
