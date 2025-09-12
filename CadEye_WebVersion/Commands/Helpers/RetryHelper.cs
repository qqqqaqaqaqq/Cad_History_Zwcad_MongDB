namespace CadEye_WebVersion.Commands.Helpers
{
    public class RetryHelper
    {
        public async Task<bool> Retry(Func<Task> action, int retry = 10)
        {
            while (retry-- > 0)
            {
                try
                {
                    await action();
                    return true;
                }
                catch (Exception)
                {
                    await Task.Delay(100);
                }
            }

            return false;
        }
    }
}
