using System.Collections.Generic;

namespace CadEye_WebVersion.Services.Zwcad
{
    public interface IZwcadService
    {
        List<string> WorkFlow_Zwcad(string path);
    }
}
