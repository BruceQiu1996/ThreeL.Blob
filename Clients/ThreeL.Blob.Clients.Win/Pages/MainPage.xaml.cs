using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ThreeL.Blob.Clients.Win.ViewModels.Page;

namespace ThreeL.Blob.Clients.Win.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
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

        private void ScrollViewer_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        private void window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            focusHold.Focus();
        }

        private void txtUserName_GotFocus(object sender, RoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            textbox.SelectAll();
        }

        private void ListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        //Path path = null;
        //System.Windows.Point startPoint;
        //bool isMouseDown = false;
        //private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    startPoint = e.GetPosition(canvas);
        //    isMouseDown = true;
        //}

        //private void Canvas_MouseButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    isMouseDown = false;

        //    foreach (var item in GetChildObjects<ListBoxItem>(gridListbox))
        //    {
            
        //    }
        //}

        //public static List<T> GetChildObjects<T>(DependencyObject obj) where T : FrameworkElement
        //{
        //    System.Windows.DependencyObject child = null;
        //    List<T> childList = new List<T>();
        //    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        //    {
        //        child = VisualTreeHelper.GetChild(obj, i);
        //        if (child is T)
        //        {
        //            childList.Add((T)child);
        //        }
        //        childList.AddRange(GetChildObjects<T>(child));
        //    }
        //    return childList;
        //}

        //private void Canvas_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (isMouseDown)
        //    {
        //        canvas.Children.Remove(this.path);
        //        this.path = new Path();

        //        this.path.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0,139,0));
        //        this.path.StrokeThickness = 1.5;

        //        RectangleGeometry rg = new RectangleGeometry();
        //        double startX = Math.Min(startPoint.X, e.GetPosition(canvas).X) + 1;
        //        double startY = Math.Min(startPoint.Y, e.GetPosition(canvas).Y) + 1;
        //        double height = Math.Abs(e.GetPosition(canvas).Y - startPoint.Y) + 1;
        //        double width = Math.Abs(e.GetPosition(canvas).X - startPoint.X) + 1;
        //        rg.Rect = new Rect(startX, startY, width, height);
        //        this.path.Data = rg;
        //        canvas.Children.Add(this.path);
        //    }
        //}
    }
}
