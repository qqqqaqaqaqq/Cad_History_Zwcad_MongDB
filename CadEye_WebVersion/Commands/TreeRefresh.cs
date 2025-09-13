using CadEye_WebVersion.Infrastructure.Utils;
using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.ViewModels.Messages;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.IO;

namespace CadEye_WebVersion.Commands
{
    public class TreeRefresh
    {
        public AsyncRelayCommand TreeRefreshCommand { get; }
        public IChildFileService _childFileService { get; }
        public TreeRefresh(
            IChildFileService childFileService)
        {
            TreeRefreshCommand = new AsyncRelayCommand(OnTreeRefresh);
            _childFileService = childFileService;
        }

        public async Task OnTreeRefresh()
        {
            var allFiles = await _childFileService.FindAllAsync() ?? new List<ChildFile>();
            var path = AppSettings.ProjectPath;
            if (path == null) return;

            var projectname = Path.GetFileName(path);

            var send = BuildTree.BuildTreeFromFiles(allFiles, path, projectname);

            WeakReferenceMessenger.Default.Send(new SendFileTreeNode(send));
            WeakReferenceMessenger.Default.Send(new SendStatusMessage("TreeView Refresh Succed"));
        }
    }
}
