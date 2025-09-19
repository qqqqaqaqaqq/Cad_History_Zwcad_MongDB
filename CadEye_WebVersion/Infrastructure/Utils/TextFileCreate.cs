using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CadEye_WebVersion.Infrastructure.Utils
{
    public class TextFileCreate
    {
        public static void TxtCreate(string path)
        {
            System.IO.File.Create(path);
        }

        public static void TxtRead(string path)
        {

        }
    }
}
