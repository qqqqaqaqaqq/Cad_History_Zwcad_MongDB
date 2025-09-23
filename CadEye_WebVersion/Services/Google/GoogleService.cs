
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using System.Diagnostics;

namespace CadEye_WebVersion.Services.Google
{
    public class GoogleService : IGoogleService
    {
        public async Task<(string, string, string)> GoogleLogin()
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
                    CancellationToken.None,
                        new NullDataStore()
                );


                if (credential.Token != null && credential.Token.RefreshToken != null)
                {
                    var issued = credential.Token.IssuedUtc;
                    var expiresIn = credential.Token.ExpiresInSeconds ?? 3600;
                    var expiry = issued.AddSeconds(expiresIn);

                    if (expiry <= DateTime.UtcNow)
                    {
                        await credential.RefreshTokenAsync(CancellationToken.None);
                    }
                }

                var payload = await GoogleJsonWebSignature.ValidateAsync(credential.Token.IdToken);
                return (payload.Subject, payload.Email, payload.Name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return (null, null, null)!;
            }
        }
    }
}
