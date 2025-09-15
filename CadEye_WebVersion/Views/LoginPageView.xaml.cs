using CadEye_WebVersion.ViewModels;

namespace CadEye_WebVersion.Views
{ 
    public partial class LoginPageView : System.Windows.Controls.UserControl
    {
        private readonly LoginPageViewModel _loginPageViewModel;
        public LoginPageView(
            LoginPageViewModel loginPageViewModel)
        {
            InitializeComponent();
            _loginPageViewModel = loginPageViewModel;
            this.DataContext = _loginPageViewModel;
        }
    }
}
