using CadEye_WebVersion.ViewModels;
using CadEye_WebVersion.Views;
using Microsoft.VisualBasic;
using System.Windows;

namespace CadEye_WebVersion
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly InformationView _information;
        private readonly SettingView _settingView;
        private readonly HomeView _homeView;
        public MainWindow(
            MainViewModel viewModel,
            InformationView information,
            HomeView homeView,
            SettingView settingView
            )
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;

            _information = information;
            _homeView = homeView;
            _settingView = settingView;

            InformationHost.Content = _information;
            HomeHost.Content = _homeView;
            SettingHost.Content = _settingView;
        }
    }
}