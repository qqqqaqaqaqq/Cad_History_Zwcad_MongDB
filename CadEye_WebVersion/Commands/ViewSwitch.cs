using CommunityToolkit.Mvvm.Input;

namespace CadEye_WebVersion.Commands
{
    public class ViewSwitch
    {
        private readonly Action<string> OnViewSwitch;

        public ViewSwitch(Action<string> onViewSwitch)
        {
            ViewSwitchCommand = new RelayCommand<object>(ViewSwitchEvent);
            OnViewSwitch = onViewSwitch;
        }

        public RelayCommand<object> ViewSwitchCommand { get; }

        private void ViewSwitchEvent(object? parameter)
        {
            if (parameter is string viewName)
            {
                switch (viewName)
                {
                    case "Main":
                        OnViewSwitch?.Invoke("Main");
                        break;
                    case "Manage":
                        OnViewSwitch?.Invoke("Manage");
                        break;
                }
            }
        }
    }
}
