using System.Security.Cryptography;

namespace ThreeL.Blob.Infra.Core.Extensions.System
{
    public static class FileStreamExtension
    {
        public static string ToSHA256(this Stream stream)
        {
            string code = string.Empty;
            using (SHA256 mySHA256 = SHA256.Create())
            {
                var sha256Bytes = mySHA256.ComputeHash(stream);
                code = BitConverter.ToString(sha256Bytes).Replace("-", "").ToUpper();
            }

            return code;
        }
    }
}
