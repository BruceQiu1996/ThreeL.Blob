using System;
using System.IO;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Helpers;

namespace ThreeL.Blob.Clients.Win.Resources
{
    public class IniSettings
    {
        #region 配置keys
        public const string ApplicationSectionKey = "ApplicationSection";
        public const string DownloadLocationKey = "DownloadLocation";
        #endregion

        #region 配置项
        public string? DownloadLocation { get; private set; }
        #endregion

        private readonly IniHelper _iniHelper;
        public IniSettings(IniHelper iniHelper)
        {
            _iniHelper = iniHelper;
            _iniHelper.Initialize(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.ini"));
        }

        public async Task InitializeAsync()
        {
            DownloadLocation = await _iniHelper.ReadAsync(ApplicationSectionKey, DownloadLocationKey);
            if (string.IsNullOrEmpty(DownloadLocation)) 
            {
                var appDir = AppDomain.CurrentDomain.BaseDirectory;
                var downloadDir = Path.Combine(appDir.Substring(0, appDir.IndexOf('\\')), "ThreeLDownloads");
                await WriteDownloadLocation(downloadDir);
                DownloadLocation = downloadDir;
            }
        }

        /// <summary>
        /// 修改下载目录地址
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteDownloadLocation(string value)
        {
            await _iniHelper.WriteAsync(ApplicationSectionKey, DownloadLocationKey, value);
            DownloadLocation = value;
        }
    }
}
