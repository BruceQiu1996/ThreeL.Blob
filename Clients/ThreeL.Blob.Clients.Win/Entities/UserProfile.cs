namespace ThreeL.Blob.Clients.Win.Entities
{
    public class UserProfile
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public long UploadMaxSizeLimit { get; set; }
        public long DaliyUploadMaxSizeLimit { get; set; }
        public long TodayUploadMaxSize { get; set; }
        public long? DownloadSpeedLimit { get; set; }
        public long? MaxSpaceSize { get; set; }

        public void Clear()
        {
            UserId = 0;
            UserName = null;
            AccessToken = null;
            RefreshToken = null;
            Role = null;
        }
    }
}
