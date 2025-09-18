using CadEye_WebVersion.ViewModels.Messages;
using CadEye_WebVersion.ViewModels.Messages.SplashMessage;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CadEye_WebVersion.ViewModels
{
    public class SplashWindowModel : INotifyPropertyChanged
    {

        private string _initStatus = "Cad Start...";
        public string InitStatus
        {
            get => _initStatus;
            set
            {
                _initStatus = value;
                OnPropertyChanged();
            }
        }
        public SplashWindowModel()
        {
            WeakReferenceMessenger.Default.Register<SplashMessage>(this, (r, m) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    InitStatus = m.Value;
                });
            });

        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
