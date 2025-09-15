using CadEye_WebVersion.Services.Google;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.ViewModels.Messages;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CadEye_WebVersion.Commands
{
    public class CadEyeLogin
    {
        public AsyncRelayCommand LoginCommand { get; }
        public AsyncRelayCommand RegisterCommand { get; }
        public readonly IGoogleService _googleService;
        private readonly IUserControlService _userControlService;

        public CadEyeLogin(
            IGoogleService googleService,
            IUserControlService userControlService
            )
        {
            LoginCommand = new AsyncRelayCommand(OnLoginCheck);
            RegisterCommand = new AsyncRelayCommand(OnRegister);
            _googleService = googleService;
            _userControlService = userControlService;
        }

        public async Task OnLoginCheck()
        {
            await Task.Run(async () =>
            {
                string id = await _googleService.GoogleLogin();
                _userControlService.Init("Users");
                if (await _userControlService.FindAsync(id) == null)
                {
                    System.Windows.MessageBox.Show("등록되지 않은 사용자입니다. 회원가입을 진행해주세요.");
                }
                else
                {
                    var node = await _userControlService.FindAsync(id);
                    WeakReferenceMessenger.Default.Send(new SendUserName(node.Name));
                }
            });
        }

        public async Task OnRegister()
        {
            await Task.Run(async () =>
            {
                var insert_node = await _googleService.GoogleRegister();
                _userControlService.Init("Users");
                await _userControlService.AddAsync(insert_node);
            });
        }
    }
}
