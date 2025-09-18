using CadEye_WebVersion.Commands;
using CadEye_WebVersion.ViewModels.Messages;
using CadEye_WebVersion.Views;
using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CadEye_WebVersion.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        private object? _viewpage;
        public object? ViewPage
        {
            get => _viewpage;
            set
            {
                _viewpage = value;
                OnPropertyChanged(nameof(ViewPage));
            }
        }

        private Visibility _naviVisiblePage = Visibility.Visible;
        public Visibility NaviVisiblePage
        {
            get => _naviVisiblePage;
            set
            {
                _naviVisiblePage = value;
                OnPropertyChanged(nameof(NaviVisiblePage));
            }
        }

        public HomeViewModel(
            InformationViewModel informationViewModel,
            SettingViewModel settingViewModel,
            AdminViewModel adminViewModel)
        {
            Redirect = new RedirectPage();
            Redirect.OnRedirectRequested += OnRedirectPage;
            _informationViewModel = informationViewModel;
            _settingViewModel = settingViewModel;
            _adminViewModel = adminViewModel;
        }


        private readonly InformationViewModel _informationViewModel;
        private readonly SettingViewModel _settingViewModel;
        private readonly AdminViewModel _adminViewModel;
        // 메세지는 전역으로 모든 Host에 영향끼치니 사용 자제

        public void OnRedirectPage(string pagenmae)
        {
            switch (pagenmae)
            {
                case "Information":
                    ViewPage = new InformationView(_informationViewModel);
                    NaviVisiblePage = Visibility.Collapsed;
                    break;
                case "Admin":
                    ViewPage = new AdminView(_adminViewModel);
                    NaviVisiblePage = Visibility.Collapsed;
                    break;
                case "Setting":
                    ViewPage = new SettingView(_settingViewModel);
                    NaviVisiblePage = Visibility.Collapsed;
                    break;
            }
        }

        public RedirectPage Redirect { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
