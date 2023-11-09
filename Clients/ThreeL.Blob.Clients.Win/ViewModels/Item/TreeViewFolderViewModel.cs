using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ThreeL.Blob.Clients.Win.ViewModels.Item
{
    public class TreeViewFolderViewModel : ObservableObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        private ObservableCollection<TreeViewFolderViewModel> _childs;

        public ObservableCollection<TreeViewFolderViewModel> Childs 
        {
            get => _childs;
            set=> SetProperty(ref _childs, value);
        }

        public TreeViewFolderViewModel()
        {
            Childs = new ObservableCollection<TreeViewFolderViewModel>();
        }
    }
}
