using CadEye_WebVersion.ViewModels;

namespace CadEye_WebVersion.Views
{
    /// <summary>
    /// Main.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InformationView : System.Windows.Controls.UserControl
    {
        private InformationViewModel _viewModel;
        public InformationView(InformationViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;  
        }
    }
}
