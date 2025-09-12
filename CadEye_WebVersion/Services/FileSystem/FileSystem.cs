using System.Diagnostics;
using System.IO;

namespace CadEye_WebVersion.Services.FileSystem
{
    public class FileSystem : IFileSystem
    {
        public async Task FileCopy(string source, string target)
        {
            bool safecheck = isRead(source);
            if (!safecheck) { return; }


            const int retries = 5;

            for (int i = 0; i < retries; i++)
            {
                try
                {
                    using var src = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var dest = new FileStream(target, FileMode.Create, FileAccess.Write, FileShare.None);

                    await src.CopyToAsync(dest);
                    return;
                }
                catch (IOException)
                {
                    await Task.Delay(50);
                }
            }

            return;
        }

        public bool isRead(string path)
        {
            int retry = 100;
            while (retry-- > 0)
            {
                {
                    try
                    {
                        using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                        if (stream != null)
                        {
                            return true;
                        }
                    }
                    catch (IOException) { }
                }

                Thread.Sleep(100);
                Debug.WriteLine($"[{path}] 파일 접근 실패, 남은 시도: {retry}");
            }

            Debug.WriteLine($"[{path}] 최종 실패: 파일 접근 불가");
            return false;
        }
    }
}
