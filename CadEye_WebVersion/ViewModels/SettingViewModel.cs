using CadEye_WebVersion.Commands;

namespace CadEye_WebVersion.ViewModels
{
    public class SettingViewModel
    {
        public SettingViewModel()
        {
            OnThemeToggle = new ThemeToggle();
        }

        public ThemeToggle OnThemeToggle { get; }
    }
}
