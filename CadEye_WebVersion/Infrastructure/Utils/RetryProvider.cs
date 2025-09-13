namespace CadEye_WebVersion.Infrastructure.Utils
{
    public static class RetryProvider
    {
        public static async Task<bool> Retry(Func<bool> action, int retry = 10, int delayMs = 100)
        {
            while (retry-- > 0)
            {
                try
                {
                    if (action())
                        return true;
                    await Task.Delay(delayMs);
                }
                catch (Exception)
                {
                    await Task.Delay(delayMs);
                }
            }
            return false;
        }

        public static async Task<bool> RetryAsync(Func<Task<bool>> action, int retry = 10, int delayMs = 100)
        {
            while (retry-- > 0)
            {
                try
                {
                    if (await action())
                        return true;
                    await Task.Delay(delayMs);
                }
                catch (Exception)
                {
                    await Task.Delay(delayMs);
                }
            }
            return false;
        }
    }
}
