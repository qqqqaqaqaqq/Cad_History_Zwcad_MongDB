using CadEye_WebVersion.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace CadEye_WebVersion
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(
            MainViewModel viewModel
            )
        {
            InitializeComponent();
            _viewModel = viewModel;

            this.DataContext = _viewModel;
            this.Closing += OnClosedEvent;
            this.Closed += OnExactlyClosed;
        }

        public void OnClosedEvent(object? sender, CancelEventArgs e)
        {
            MessageBoxResult result1 = System.Windows.MessageBox.Show(
              "종료 하시겠습니까? 안전을 위해 모든 Zwcad가 종료됩니다.",
              "종료 확인",
              MessageBoxButton.YesNo,
              MessageBoxImage.Question
          );
            if (result1 == MessageBoxResult.Yes)
            {
                foreach (Process proc in Process.GetProcessesByName("ZWCAD"))
                {
                    proc.Kill();
                    proc.WaitForExit();
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        public void OnExactlyClosed(object? sender, EventArgs e)
        {
            foreach (Process proc in Process.GetProcessesByName("ZWCAD"))
            {
                proc.Kill();
                proc.WaitForExit();
            }
        }
    }
}