using CadEye_WebVersion.ViewModels;

namespace CadEye_WebVersion.Views
{
    /// <summary>
    /// Home.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HomeView : System.Windows.Controls.UserControl
    {
        private readonly HomeViewModel _viewModel;


        public HomeView(
            HomeViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;

            this.DataContext = _viewModel;
        }
    }
}
