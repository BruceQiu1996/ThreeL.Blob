using ThreeL.Blob.Domain.Entities;

namespace ThreeL.Blob.Application.Contract.Configurations
{
    public class JwtOptions
    {
        public string[] Audiences { get; set; } //订阅者们
        public int SecretExpireSeconds { get; set; } //jwt secret过期时间
        public int TokenExpireSeconds { get; set; } //jwt过期时间
        public int RefreshTokenExpireSeconds { get; set; } //refresh token过期时间
        public int ClockSkew { get; set; }

        public JwtSetting ToEntity() 
        {
            return new JwtSetting()
            {
                SecretKey = Guid.NewGuid().ToString(),
                Audiences = Audiences.ToArray(),
                TokenExpireSeconds = TokenExpireSeconds,
                ClockSkew = ClockSkew
            };
        }
    }
}
