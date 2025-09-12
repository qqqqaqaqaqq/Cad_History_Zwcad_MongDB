using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms.Integration;

namespace CadEye_WebVersion.Services.WindowService
{
    public class WindowService : IWindowsService
    {
        public System.Windows.Window Form_View(WindowsFormsHost host)
        {

            var window = new System.Windows.Window
            {
                Title = "PDF Viewer",
                Width = 800,
                Height = 600
            };

            var grid = new Grid();
            grid.Children.Add(host);
            window.Content = grid;
            return window;
        }
    }
}
