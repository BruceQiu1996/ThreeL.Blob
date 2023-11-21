using System.Windows;
using System.Windows.Controls;
using ThreeL.Blob.Clients.Win.ViewModels.Apply;

namespace ThreeL.Blob.Clients.Win.Helpers
{
    internal class ApplyMessageDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var fe = container as FrameworkElement;
            var obj = item as ApplyMessageViewModel;
            DataTemplate dt = null;
            if (item != null)
            {
                if (item is AddFriendApplyMessageViewModel)
                    dt = fe.FindResource("addfriend") as DataTemplate;
                if (item is ApplyDateMessageViewModel)
                    dt = fe.FindResource("adddate") as DataTemplate;
            }

            return dt;
        }
    }
}
