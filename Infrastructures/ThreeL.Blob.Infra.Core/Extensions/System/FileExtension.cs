﻿using System.Security.Cryptography;

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

        public static string GetAvailableDirLocation(this string dirName, string parentFolderName)
        {
            var newName = Path.Combine(parentFolderName, dirName);
            int count = 1;
            while (Directory.Exists(newName))
            {
                newName = Path.Combine(parentFolderName, $"{dirName}({count})");
                count++;
            }

            return newName;
        }

        public static long GetDirectoryLength(this string dirPath)
        {
            long len = 0;
            DirectoryInfo di = new DirectoryInfo(dirPath);
            foreach (FileInfo fi in di.GetFiles())
            {
                len += fi.Length;
            }

            DirectoryInfo[] dis = di.GetDirectories();
            if (dis.Length > 0)
            {
                for (int i = 0; i < dis.Length; i++)
                {
                    len += GetDirectoryLength(dis[i].FullName);
                }
            }

            return len;
        }
    }
}
