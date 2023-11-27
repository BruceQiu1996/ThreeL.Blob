using System.Security.Cryptography;

namespace ThreeL.Blob.Infra.Core.Utils
{
    public class TokenGenerator
    {
        public static string GenerateToken(int length)
        {
            using (var crypto = new RNGCryptoServiceProvider())
            {
                var bits = (length * 6);
                var byte_size = ((bits + 7) / 8);
                var bytesarray = new byte[byte_size];
                crypto.GetBytes(bytesarray);
                return Convert.ToBase64String(bytesarray);
            }
        }
    }
}
