using Google.Protobuf.WellKnownTypes;
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
        public const string AutoStartKey = "AutoStartKey";
        public const string ExitWithoutMinKey = "ExitWithoutMinKey";
        public const string UserNameKey = "UserNameKey";
        public const string PasswordKey = "PasswordKey";
        public const string LoginTimeKey = "LoginTimeKey";
        #endregion

        #region 配置项
        public string? DownloadLocation { get; private set; }
        public string? TempLocation { get; private set; }
        public int MaxUploadThreads { get; private set; }
        public int MaxDownloadThreads { get; private set; }
        public bool AutoStart { get; private set; }
        public bool ExitWithoutMin { get; private set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string LoginTime { get; set; }
        #endregion

        private readonly IniHelper _iniHelper;
        private readonly SystemHelper _systemHelper;
        public IniSettings(IniHelper iniHelper, SystemHelper systemHelper)
        {
            _iniHelper = iniHelper;
            _iniHelper.Initialize(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.ini"));
            _systemHelper = systemHelper;
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
            AutoStart = bool.TryParse(await _iniHelper.ReadAsync(ApplicationSectionKey, AutoStartKey), out var tempAutoStart) ? tempAutoStart : false;
            ExitWithoutMin = bool.TryParse(await _iniHelper.ReadAsync(ApplicationSectionKey, ExitWithoutMinKey), out var tempExitWithoutMin) ? tempExitWithoutMin : false;
            Password = await _iniHelper.ReadAsync(ApplicationSectionKey, PasswordKey);
            UserName = await _iniHelper.ReadAsync(ApplicationSectionKey, UserNameKey);
            LoginTime = await _iniHelper.ReadAsync(ApplicationSectionKey, LoginTimeKey);
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

        public async Task WriteAutoStart(bool value)
        {
            await _iniHelper.WriteAsync(ApplicationSectionKey, AutoStartKey, value.ToString());
            AutoStart = value;
            if (value)
            {
                _systemHelper.SetAutoStart();
            }
            else 
            {
                _systemHelper.CancelAutoStart();
            }
        }

        public async Task WriteExitWithoutMin(bool value)
        {
            await _iniHelper.WriteAsync(ApplicationSectionKey, ExitWithoutMinKey, value.ToString());
            ExitWithoutMin = value;
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

        public async Task WriteUserInfo(string userName,string password,string loginTime)
        {
            await _iniHelper.WriteAsync(ApplicationSectionKey, UserNameKey, userName);
            await _iniHelper.WriteAsync(ApplicationSectionKey, PasswordKey, password);
            await _iniHelper.WriteAsync(ApplicationSectionKey, LoginTimeKey, loginTime);

            UserName = userName;
            Password = password;
            LoginTime = loginTime;
        }
    }
}
