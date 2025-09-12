using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CadEye_WebVersion.Behavior
{
    public class BindableSelectedItemBehaviorDataGrid : Behavior<DataGrid>
    {
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(BindableSelectedItemBehaviorDataGrid),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as BindableSelectedItemBehaviorDataGrid;
            var dataGrid = behavior?.AssociatedObject;

            if (dataGrid != null && e.NewValue != null && dataGrid.SelectedItem != e.NewValue)
            {
                dataGrid.SelectedItem = e.NewValue;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AssociatedObject.SelectedItem != null)
            {
                SelectedItem = AssociatedObject.SelectedItem;
            }
        }
    }
}
