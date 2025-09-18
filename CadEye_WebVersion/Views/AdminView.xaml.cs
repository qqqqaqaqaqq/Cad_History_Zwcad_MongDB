using CadEye_WebVersion.ViewModels;

namespace CadEye_WebVersion.Views
{
    /// <summary>
    /// AdminView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AdminView : System.Windows.Controls.UserControl
    {
        private readonly AdminViewModel _viewModel;
        public AdminView(
            AdminViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;
        }
    }
}
