using CadEye_WebVersion.Services.Google.Models;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;


namespace CadEye_WebVersion.Services.Google
{
    public class GoogleService : IGoogleService
    {
        public async Task<string> GoogleLogin()
        {
            string[] scopes = { "email", "profile" };
            string clientId = AppSettings.MyGoogleId!;
            string clientSecret = AppSettings.MyGoogleSecrete!;

            try
            {
                var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret
                    },
                    scopes,
                    "user",
                    CancellationToken.None
                );

                var payload = await GoogleJsonWebSignature.ValidateAsync(credential.Token.IdToken);
                return payload.Subject;
            }
            catch (Exception ex)
            {
                // 로깅 및 사용자 안내
                throw new ApplicationException("Google 로그인 실패", ex);
            }
        }

        public async Task<LoginEntity> GoogleRegister()
        {
            string[] scopes = { "email", "profile" };
            string clientId = AppSettings.MyGoogleId!;
            string clientSecret = AppSettings.MyGoogleSecrete!;

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                scopes,
                "user",
                CancellationToken.None
            );

            var payload = await GoogleJsonWebSignature.ValidateAsync(credential.Token.IdToken);
            string googleId = payload.Subject;
            string email = payload.Email;

            // DB에 신규 사용자 생성
            var newUser = new LoginEntity
            {
                GoogleId = googleId,
                Email = email,
                Name = payload.Name,
                LoginAt = DateTime.UtcNow
            };

            return newUser;
        }
    }
}
