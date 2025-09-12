namespace CadEye_WebVersion.Services.FileSystem
{
    public interface IFileSystem
    {
        Task FileCopy(string source, string target);
        bool isRead(string path);
    }
}
