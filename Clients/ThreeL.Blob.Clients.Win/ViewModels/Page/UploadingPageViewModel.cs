using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Win.Entities;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Item;

namespace ThreeL.Blob.Clients.Win.ViewModels.Page
{
    public class UploadingPageViewModel : ObservableObject
    {
        public AsyncRelayCommand LoadedRelayCommand { get; set; }
        public ObservableCollection<UploadItemViewModel> UploadItemViewModels { get; set; }

        private readonly IMapper _mapper;
        public UploadingPageViewModel(IMapper mapper)
        {
            _mapper = mapper;
            UploadItemViewModels = new ObservableCollection<UploadItemViewModel>();
            //new upload task
            WeakReferenceMessenger.Default.Register<UploadingPageViewModel, UploadFileRecord, string>(this, Const.AddUploadRecord, async (x, y) =>
             {
                 await AddNewUploadTaskAsync(y);
             });
        }

        private async Task AddNewUploadTaskAsync(UploadFileRecord uploadFileRecord)
        {
            var viewModel = App.ServiceProvider!.GetRequiredService<UploadItemViewModel>();
            _mapper.Map(uploadFileRecord, viewModel);
            UploadItemViewModels.Add(viewModel);

            await viewModel.StartAsync();
        }
    }
}
