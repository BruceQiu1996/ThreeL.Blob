namespace ThreeL.Blob.Application.Contract.Dtos
{
    public class UserRefreshTokenDto
    {
        public string Origin { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
