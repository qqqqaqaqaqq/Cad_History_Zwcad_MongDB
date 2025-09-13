using System.IO;

namespace CadEye_WebVersion.Infrastructure.Utils
{
    public static class FileCopyProvider
    {
        public static async Task<bool> FileCopy(string source, string target)
        {
            try
            {
                using var src = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var dest = new FileStream(target, FileMode.Create, FileAccess.Write, FileShare.None);

                await src.CopyToAsync(dest);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}
