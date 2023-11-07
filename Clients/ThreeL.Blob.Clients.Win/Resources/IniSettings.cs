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
        public const string TempLocationKey = "TempLocation";
        public const string MaxUploadThreadsKey = "MaxUploadThreads";
        public const string MaxDownloadThreadsKey = "MaxDownloadThreads";
        #endregion

        #region 配置项
        public string? DownloadLocation { get; private set; }
        public string? TempLocation { get; private set; }
        public int MaxUploadThreads { get; private set; }
        public int MaxDownloadThreads { get; private set; }
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
            }

            TempLocation = await _iniHelper.ReadAsync(ApplicationSectionKey, TempLocationKey);
            if (string.IsNullOrEmpty(TempLocation))
            {
                var dir = Path.Combine(Path.GetTempPath(), "ThreeLDownloads");
                if (!Directory.Exists(dir)) 
                {
                    Directory.CreateDirectory(dir);
                }

                await WriteTempLocation(dir);
            }

            MaxUploadThreads = int.TryParse(await _iniHelper.ReadAsync(ApplicationSectionKey, MaxUploadThreadsKey),out var tempMaxUploadThreads)? tempMaxUploadThreads : 5;
            MaxDownloadThreads = int.TryParse(await _iniHelper.ReadAsync(ApplicationSectionKey, MaxDownloadThreadsKey), out var tempMaxDownloadThreads) ? tempMaxDownloadThreads : 5;
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

        /// <summary>
        /// 修改临时文件目录地址
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteTempLocation(string value)
        {
            await _iniHelper.WriteAsync(ApplicationSectionKey, TempLocationKey, value);
            TempLocation = value;
        }

        public async Task WriteMaxUploadThreads(int value)
        {
            await _iniHelper.WriteAsync(ApplicationSectionKey, MaxUploadThreadsKey, value.ToString());
            MaxUploadThreads = value;
        }

        public async Task WriteMaxDownloadThreads(int value)
        {
            await _iniHelper.WriteAsync(ApplicationSectionKey, MaxDownloadThreadsKey, value.ToString());
            MaxDownloadThreads = value;
        }
    }
}
