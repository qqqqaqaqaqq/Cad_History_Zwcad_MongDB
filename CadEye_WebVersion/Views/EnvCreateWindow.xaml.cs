using CadEye_WebVersion.ViewModels;
using System.Windows;


namespace CadEye_WebVersion.Views
{
    public partial class EnvCreateWindow : Window
    {
        private EnvCreateWindowModel viewmodel = new EnvCreateWindowModel();

        public EnvCreateWindow()
        {
            InitializeComponent();
            this.DataContext = viewmodel;
        }
    }
}
