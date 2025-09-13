using CadEye_WebVersion.ViewModels;

namespace CadEye_WebVersion.Views
{
    /// <summary>
    /// SettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingView : System.Windows.Controls.UserControl
    {
        private SettingViewModel _viewModel;
        public SettingView(
            SettingViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;
        }
    }
}
