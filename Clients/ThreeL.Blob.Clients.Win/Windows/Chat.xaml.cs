using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ThreeL.Blob.Clients.Win.Resources;
using ThreeL.Blob.Clients.Win.ViewModels.Window;

namespace ThreeL.Blob.Clients.Win.Windows
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Chat : Window
    {
        public Chat(ChatViewModel chatViewModel)
        {
            InitializeComponent();
            DataContext = chatViewModel;

            WeakReferenceMessenger.Default.Register<Chat, string, string>(this, Const.HiddenChatWindow, async (x, y) =>
            {
                if (y == "close")
                {
                    Hide();
                }
                else 
                {
                    Show();
                }
            });
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Label_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            Hide();
        }

        private void ScrollViewer_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        private void ListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }
    }
}
