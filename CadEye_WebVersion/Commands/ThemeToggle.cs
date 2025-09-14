using CadEye_WebVersion.ViewModels.Messages.ThemeBrush;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows.Media;

namespace CadEye_WebVersion.Commands
{

    public class ThemeToggle
    {
        public RelayCommand<object> ThemeToggleCommand { get; }

        public ThemeToggle()
        {
            ThemeToggleCommand = new RelayCommand<object>(OnThemeToggle);
        }

        public void OnThemeToggle(object? parameter)
        {
            System.Windows.Media.Brush globalColor, theme, viewContainBackground, globalBorderBrush;
            if (parameter == null) { return; }
            string? mode = parameter as string;
            if (mode == "DarkMode")
            {
                AppSettings.ThemeToggle = true;
            }
            else
            {
                AppSettings.ThemeToggle = false;
            }

            if (AppSettings.ThemeToggle)
            {
                globalColor = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2E2E2E"));
                theme = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                viewContainBackground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF1E1E1E"));
                globalBorderBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF444444"));

                AppSettings.ThemeToggle = !AppSettings.ThemeToggle;
            }
            else
            {
                globalColor = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF0F0F0"));
                theme = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                viewContainBackground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                globalBorderBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFCCCCCC"));

                AppSettings.ThemeToggle = !AppSettings.ThemeToggle;
            }

            // 브러시 전송
            WeakReferenceMessenger.Default.Send(new SendGlobalColor(globalColor));
            WeakReferenceMessenger.Default.Send(new SendForeGroundBrush(theme));
            WeakReferenceMessenger.Default.Send(new SendViewContainBackGround(viewContainBackground));
            WeakReferenceMessenger.Default.Send(new SendGlobalBorderBrush(globalBorderBrush));
        }
    }
}
