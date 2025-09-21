using CadEye_WebVersion.ViewModels.Messages.ThemeBrush;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
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
            bool? mode = parameter as bool?;

            if (mode == true)
            {
                globalColor = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2E2E2E"));
                theme = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                viewContainBackground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF1E1E1E"));
                globalBorderBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF444444"));
            }
            else
            {
                globalColor = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF0F0F0"));
                theme = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                viewContainBackground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                globalBorderBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFCCCCCC"));
            }

            // 브러시 전송
            WeakReferenceMessenger.Default.Send(new SendGlobalBackgroundColor(globalColor));
            WeakReferenceMessenger.Default.Send(new SendForeground(theme));
            WeakReferenceMessenger.Default.Send(new SendGlobalBackgroundColor_View(viewContainBackground));
            WeakReferenceMessenger.Default.Send(new SendGlobalBorderBrush(globalBorderBrush));
        }
    }
}
