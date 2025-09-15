using CadEye_WebVersion.Services.Google.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadEye_WebVersion.Services.Google
{
    public interface IGoogleService
    {
        Task<string> GoogleLogin();
        Task<LoginEntity> GoogleRegister();
    }
}
