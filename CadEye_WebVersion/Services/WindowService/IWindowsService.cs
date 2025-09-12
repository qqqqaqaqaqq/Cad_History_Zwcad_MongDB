using System.Threading.Tasks;
using System.Windows.Forms.Integration;

namespace CadEye_WebVersion.Services.WindowService
{
    public interface IWindowsService
    {
        System.Windows.Window Form_View(WindowsFormsHost host);
    }
}
