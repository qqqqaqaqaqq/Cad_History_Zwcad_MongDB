using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Windows;

namespace CadEye_WebVersion.ViewModels
{
    public class EnvCreateWindowModel
    {
        public RelayCommand<Tuple<string, string, string, Window>> KeySenderCommand { get; }

        public EnvCreateWindowModel()
        {
            KeySenderCommand = new RelayCommand<Tuple<string, string, string, Window>>(CreateEnv);
        }

        private void CreateEnv(Tuple<string, string, string, Window> keys)
        {
            if (keys.Item1 == null || keys.Item2 == null || keys.Item3 == null) return;

            string mongoDbKey = keys.Item1;
            string googleOAuthId = keys.Item2;
            string googleOAuthSecret = keys.Item3;
            Window window = keys.Item4;

            // .env 파일 경로 (실제 경로로 바꿀 수 있음)
            string envFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");

            try
            {
                // .env 내용 작성
                string envContent =
                    $@"MONGO_URI={mongoDbKey}
                    GOOGLE_ID={googleOAuthId}
                    GOOGLE_SECRETE={googleOAuthSecret}";

                // 파일 쓰기
                File.WriteAllText(envFilePath, envContent);

                // 완료 메시지 (원하면 MessageBox 등으로)
                Console.WriteLine($".env 파일 생성 완료: {envFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"파일 생성 중 오류 발생: {ex.Message}");
            }

            window?.Close();
        }
    }
}
