using System.Windows.Forms;

namespace CadEye_WebVersion.Services.FolderService
{
    public class FolderService : iFolderService
    {
        public string OpenFolder()
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "폴더를 선택하세요",
                ShowNewFolderButton = true
            };

            // WPF Window를 Owner로 지정할 수 있음
            var result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                return dialog.SelectedPath;
            }

            return string.Empty;
        }
    }
}
