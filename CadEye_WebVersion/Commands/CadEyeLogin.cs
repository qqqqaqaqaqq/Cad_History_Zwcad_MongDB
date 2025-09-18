using CadEye_WebVersion.Services.Google;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.ViewModels.Messages;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MongoDB.Driver;
namespace CadEye_WebVersion.Commands
{
    public class CadEyeLogin
    {
        public AsyncRelayCommand LoginCommand { get; }
        public readonly IGoogleService _googleService;
        private readonly IUserControlService _userControlService;

        public CadEyeLogin(
            IGoogleService googleService,
            IUserControlService userControlService
            )
        {
            LoginCommand = new AsyncRelayCommand(OnLoginCheck);
            _googleService = googleService;
            _userControlService = userControlService;
        }

        public async Task OnLoginCheck()
        {
            await Task.Run(async () =>
            {
                (string subject, string name) = await _googleService.GoogleLogin();
                if (subject == null || name == null)
                {
                    MessageBox.Show("Login Failed, Please try again.");
                    return;
                }
                WeakReferenceMessenger.Default.Send(new SendUserName(name!));


                // Mongo User DB 불러오기
                var settings = MongoClientSettings.FromConnectionString(AppSettings.ServerIP);
                settings.ServerApi = new ServerApi(ServerApiVersion.V1);
                var client = new MongoClient(settings);
                var dbnamelist = client.ListDatabaseNames().ToList();

                var dbfiltered = dbnamelist
                    .Where(name => name.Contains($"{subject}_&_"))
                    .Select(name => name.Replace($"{subject}_&_", ""))
                    .ToList();

                WeakReferenceMessenger.Default.Send(new SendGoogleId(subject));
                WeakReferenceMessenger.Default.Send(new SendUsersDatabase(dbfiltered));
            });
        }
    }
}
