using CadEye_WebVersion.ViewModels;
using System.Windows;

namespace CadEye_WebVersion.Views
{
    /// <summary>
    /// SplashWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SplashWindow : Window
    {
        public readonly SplashWindowModel _viewmodel;
        public SplashWindow()
        {
            _viewmodel = new SplashWindowModel();
            InitializeComponent();
            this.DataContext = _viewmodel;
        }
    }
}
