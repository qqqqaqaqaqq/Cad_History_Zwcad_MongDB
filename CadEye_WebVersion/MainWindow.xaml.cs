using CadEye_WebVersion.ViewModels;
using System.Windows;

namespace CadEye_WebVersion
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(
            MainViewModel viewModel
            )
        {
            InitializeComponent();
            _viewModel = viewModel;

            this.DataContext = _viewModel;
        }
    }
}