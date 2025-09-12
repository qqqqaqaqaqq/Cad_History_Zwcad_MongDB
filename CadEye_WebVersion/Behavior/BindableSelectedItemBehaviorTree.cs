using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace CadEye_WebVersion.Behavior
{
    public class BindableSelectedItemBehaviorTree : Behavior<System.Windows.Controls.TreeView>
    {
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(BindableSelectedItemBehaviorTree),
                new UIPropertyMetadata(null, OnSelectedItemChanged));

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is TreeViewItem item)
            {
                item.SetValue(TreeViewItem.IsSelectedProperty, true);
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = e.NewValue;
        }
    }
}
