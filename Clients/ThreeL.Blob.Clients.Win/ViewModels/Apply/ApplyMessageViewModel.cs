using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ThreeL.Blob.Clients.Win.ViewModels.Apply
{
    public class ApplyMessageViewModel : ObservableObject
    {
        public long? Id { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateDate { get; set; }
    }
}
