namespace ThreeL.Blob.Application.Contract.Dtos
{
    public class UserLoginResponseDto
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public long UploadMaxSizeLimit { get; set; }
        public long DaliyUploadMaxSizeLimit { get; set; }
        public long TodayUploadMaxSize { get; set; }
        public long? DownloadSpeedLimit { get; set; }
        public long? MaxSpaceSize { get; set; }
        public string? Avatar { get; set; }
    }
}
