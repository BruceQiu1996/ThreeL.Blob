﻿using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ThreeL.Blob.Clients.Win.Helpers
{
    public class IniHelper
    {
        private string _configFile;

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        [DllImport("kernel32")]
        private static extern int WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);

        public Task Initialize(string path)
        {
            _configFile = path;
            return Task.CompletedTask;
        }

        public Task<string> ReadAsync(string section, string key)
        {
            StringBuilder sb = new StringBuilder(1024);
            GetPrivateProfileString(section, key, null, sb, 1024, _configFile);
            return Task.FromResult(sb.ToString());
        }

        public Task WriteAsync(string section, string key, string value)
        {
            return Task.FromResult(WritePrivateProfileString(section, key, value, _configFile));
        }
    }
}
