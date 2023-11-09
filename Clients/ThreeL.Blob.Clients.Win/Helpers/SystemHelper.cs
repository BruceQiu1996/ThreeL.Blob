using Microsoft.Win32;
using System;

namespace ThreeL.Blob.Clients.Win.Helpers
{
    public class SystemHelper
    {
        public SystemHelper()
        {
            
        }

        public void SetAutoStart()
        {
            try
            {
                string strName = AppDomain.CurrentDomain.BaseDirectory + "HeadDisk.exe";//获取要自动运行的应用程序名，也就是本程序的名称
                if (!System.IO.File.Exists(strName))//判断要自动运行的应用程序文件是否存在
                    return;
                string strnewName = strName.Substring(strName.LastIndexOf("\\") + 1);//获取应用程序文件名，不包括路径
                RegistryKey registry = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);//检索指定的子项
                if (registry == null)//若指定的子项不存在
                    registry = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");//则创建指定的子项
                registry.SetValue(strnewName, strName);//设置该子项的新的“键值对”
                registry.Close();
            }
            catch (Exception ex)
            {
            }
        }

        public void CancelAutoStart()
        {
            try
            {
                string strName = AppDomain.CurrentDomain.BaseDirectory + "HeadDisk.exe";//获取要自动运行的应用程序名
                if (!System.IO.File.Exists(strName))//判断要取消的应用程序文件是否存在
                    return;
                string strnewName = strName.Substring(strName.LastIndexOf("\\") + 1);///获取应用程序文件名，不包括路径
                RegistryKey registry = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);//读取指定的子项
                if (registry == null)//若指定的子项不存在
                    registry = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");//则创建指定的子项
                registry.DeleteValue(strnewName, false);//删除指定“键名称”的键/值对
                registry.Close();
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
