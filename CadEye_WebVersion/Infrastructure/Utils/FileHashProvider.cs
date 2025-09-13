using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace CadEye_WebVersion.Infrastructure.Utils
{
    public static class FileHashProvider
    {
        public static async Task<byte[]> Hash_Allocated_Unique(string fullName)
        {
            byte[] contentHash = Array.Empty<byte>();

            using (var stream = new FileStream(fullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sha = SHA256.Create())
            {
                contentHash = await Task.Run(() => sha.ComputeHash(stream));
            }

            string combined = Convert.ToBase64String(contentHash);
            using (var sha = SHA256.Create())
            {
                return sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combined));
            }
        }
    }
}
