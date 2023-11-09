using CommunityToolkit.Mvvm.Messaging;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels;

namespace ThreeL.Blob.Clients.Win
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IniSettings _settings;
        public MainWindow(MainWindowViewModel viewModel, IniSettings settings)
        {
            InitializeComponent();
            DataContext = viewModel;
            _settings = settings;
        }
        private void Border_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }

        private void Label_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Label_MouseLeftButtonDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_settings.ExitWithoutMin)
            {
                WeakReferenceMessenger.Default.Send<string, string>(string.Empty, Const.Exit);
            }
            else
            {
                Hide();
            }
        }
        private void NotifyIcon_Click(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Topmost = true;
            var _ = Task.Run(async () =>
            {
                await Task.Delay(1000);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Topmost = false;
                });
            });
        }
    }
}
