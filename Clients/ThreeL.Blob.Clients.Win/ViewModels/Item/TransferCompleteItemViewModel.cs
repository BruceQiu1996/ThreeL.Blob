using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Media.Imaging;
using ThreeL.Blob.Clients.Win.Helpers;

namespace ThreeL.Blob.Clients.Win.ViewModels.Item
{
    public class TransferCompleteItemViewModel : ObservableObject
    {
        public string Id { get; set; }
        public bool IsUpload { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime FinishTime { get; set; }
        public bool Success { get; set; }
        public string Reason { get; set; }
        public string TaskId { get; set; }
        public BitmapImage Icon => App.ServiceProvider!.GetRequiredService<FileHelper>().GetIconByFileExtension(FileName);

        public TransferCompleteItemViewModel() 
        {
        
        }
    }
}
