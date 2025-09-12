using CadEye_WebVersion.Models.Entity;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CadEye_WebVersion.Services.FileCheck
{
    public interface IFileCheckService
    {
        Task<ConcurrentBag<ChildFile>> AllocateData(string path);
    }
}
