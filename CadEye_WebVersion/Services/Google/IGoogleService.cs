namespace CadEye_WebVersion.Services.Google
{
    public interface IGoogleService
    {
        Task<(string, string, string)> GoogleLogin();
    }
}
