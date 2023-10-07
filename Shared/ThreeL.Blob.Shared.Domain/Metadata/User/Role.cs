using System.ComponentModel;

namespace ThreeL.Blob.Shared.Domain.Metadata.User
{
    public enum Role
    {
        [Description("用户")]
        User,
        [Description("管理员")]
        Admin,
        [Description("超级管理员")]
        SuperAdmin
    }
}
