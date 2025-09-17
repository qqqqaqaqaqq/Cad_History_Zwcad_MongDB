using CadEye_WebVersion.ViewModels.Messages.ThemeBrush;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;

namespace CadEye_WebVersion.Commands
{
    public class RedirectPage
    {
        public event Action<string>? OnRedirectRequested;

        public RelayCommand<object?> RedirectCommand { get; }
        public RedirectPage()
        {
            RedirectCommand = new RelayCommand<object?>(OnRedirect);
        }

        private void OnRedirect(object? parameter)
        {
            string? pagename = parameter as string;
            if (pagename == null) { return; }
            OnRedirectRequested?.Invoke(pagename);
            WeakReferenceMessenger.Default.Send(new SendPageName(pagename));
        }
    }
}