using System.Windows;
using ThreeL.Blob.Clients.Win.ViewModels.Window;

namespace ThreeL.Blob.Clients.Win.Windows
{
    /// <summary>
    /// Interaction logic for ZipFileObjectsEnsure.xaml
    /// </summary>
    public partial class ZipFileObjectsEnsure : Window
    {
        public ZipFileObjectsEnsure(ZipFileObjectsEnsureViewModel zipFileObjectsEnsureViewModel)
        {
            InitializeComponent();
            DataContext = zipFileObjectsEnsureViewModel; 
        }
    }
}
