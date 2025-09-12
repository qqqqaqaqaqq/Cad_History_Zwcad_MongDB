using CadEye_WebVersion.Services.FolderService;
using CommunityToolkit.Mvvm.Input;

namespace CadEye_WebVersion.Commands
{
    public class FolderSelect
    {
        private readonly iFolderService _folderService;
        public RelayCommand FolderSelectCommand { get; }
        private readonly Action<string> _onFolderSelected;

        public FolderSelect(iFolderService folderService, Action<string> onFolderSelected)
        {
            _folderService = folderService;
            _onFolderSelected = onFolderSelected;
            FolderSelectCommand = new RelayCommand(FolderSelectEvent);
        }

        public void FolderSelectEvent()
        {
            var path = _folderService.OpenFolder();
            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("폴더를 선택해주세요", "알림");
                return;
            }

            _onFolderSelected?.Invoke(path);
        }
    }
}
