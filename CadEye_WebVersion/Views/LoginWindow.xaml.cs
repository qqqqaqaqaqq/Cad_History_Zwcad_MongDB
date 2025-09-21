using CadEye_WebVersion.Services.Google;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.Services.Mongo.Services;
using CadEye_WebVersion.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace CadEye_WebVersion.Views
{
    /// <summary>
    /// LoginWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly IUserControlService _userControlService = new UserControlService();
        private readonly IGoogleService _googleService = new GoogleService();
        public LoginWindow()
        {
            InitializeComponent();
            LoginWindowModel _loginPageViewModel = new LoginWindowModel(_googleService, _userControlService);
            this.DataContext = _loginPageViewModel;
        }

        public void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        private void Border_Moved(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
