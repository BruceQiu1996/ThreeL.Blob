using System.Security.Cryptography;

namespace ThreeL.Blob.Infra.Core.Extensions.System
{
    public static class FileExtension
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

        public static string GetAvailableFileLocation(this string fileName, string folder)
        {
            var newName = Path.Combine(folder, fileName);
            var onlyName = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            int count = 1;
            while (File.Exists(newName)) 
            {
                newName = Path.Combine(folder, $"{onlyName}({count}){extension}");
                count++;
            }

            return newName;
        }
    }
}
