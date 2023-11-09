using System.Windows;
using System.Windows.Input;
using ThreeL.Blob.Clients.Win.ViewModels.Window;

namespace ThreeL.Blob.Clients.Win.Windows
{
    /// <summary>
    /// Interaction logic for Move.xaml
    /// </summary>
    public partial class Move : Window
    {
        public Move(MoveViewModel moveViewModel)
        {
            InitializeComponent();
            DataContext = moveViewModel;
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
