using HandyControl.Controls;
using HandyControl.Data;

namespace ThreeL.Blob.Clients.Win.Helpers
{
    public class GrowlHelper
    {
        public void Info(string message)
        {
            Growl.Info(new GrowlInfo()
            {
                Message = message,
                ShowDateTime = false,
                ShowCloseButton = true,
                StaysOpen = false,
                WaitTime = 2
            });
        }

        public void Success(string message)
        {
            Growl.Success(new GrowlInfo()
            {
                Message = message,
                ShowDateTime = false,
                ShowCloseButton = true,
                StaysOpen = false,
                WaitTime = 2
            });
        }

        public void Warning(string message)
        {
            Growl.Warning(new GrowlInfo()
            {
                Message = message,
                ShowDateTime = false,
                ShowCloseButton = true,
                StaysOpen = false,
                WaitTime = 2
            });
        }
    }
}
