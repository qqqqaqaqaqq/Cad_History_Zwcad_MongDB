using CadEye_WebVersion.Commands;
using CadEye_WebVersion.Models;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.ViewModels.Messages;
using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CadEye_WebVersion.ViewModels
{
    public class AdminViewModel : INotifyPropertyChanged
    {
        private AdminEntity _admin = new AdminEntity();
        private readonly IUserControlService _userControlService;
        public AdminEntity Admin
        {
            get => _admin;
            set
            {
                _admin = value;
                OnPropertyChanged();
            }
        }

        public AdminViewModel(
            IUserControlService userControlService)
        {
            _userControlService = userControlService;
            AdminSavebtn = new AdminSave(_userControlService);
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (LoginSession.Admin != null)
                {
                    Admin = LoginSession.Admin;
                }
            });
        }


        public AdminSave AdminSavebtn { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
