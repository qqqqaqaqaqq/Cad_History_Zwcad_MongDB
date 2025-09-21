using CadEye_WebVersion.Models;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.ViewModels.Messages;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;


namespace CadEye_WebVersion.Commands
{
    
    public class AdminSave
    {
        public AsyncRelayCommand<object?> AdminSaveCommand { get; }
        private readonly IUserControlService _userControlService;

        public AdminSave(
            IUserControlService userControlService) 
        {
            _userControlService = userControlService;
            AdminSaveCommand = new AsyncRelayCommand<object?>(OnAdminSave);
        }

        public async Task OnAdminSave(object? parameter)
        {
            _userControlService.Init();

            if (parameter == null) return;
            if (parameter is AdminEntity admin)
            {
                var node = await _userControlService.FindAsync(admin.Googleid);
                if(node == null) return;

                node.Theme = admin.Theme;
                node.LastLogin = DateTime.Now;
                node.CADThread = admin.CADThread;
                node.PDFThread = admin.PDFThread;

                await _userControlService.UpdateAsync(node);
            }

            WeakReferenceMessenger.Default.Send(new SendStatusMessage("Admin Update Completed"));
        }
    }
}
