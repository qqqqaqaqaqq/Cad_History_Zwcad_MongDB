using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadEye_WebVersion.Infrastructure.Utils
{
    public static class ExtFilterProvider
    {
        public static Task<bool> ExtFilter(string path)
        {
            var ext = System.IO.Path.GetExtension(path);
            if (ext.ToUpper() == ".DWG" || ext.ToUpper() == ".DXF")
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }
    }
}
