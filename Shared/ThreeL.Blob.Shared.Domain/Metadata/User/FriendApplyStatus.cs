using System.ComponentModel;

namespace ThreeL.Blob.Shared.Domain.Metadata.User
{
    public enum FriendApplyStatus
    {
        [Description("未处理")]
        Unhandled,
        [Description("已同意")]
        Agreed,
        [Description("已拒绝")]
        Rejected
    }
}
