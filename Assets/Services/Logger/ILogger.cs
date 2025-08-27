namespace iBMSApp.Services
{
    public interface ILogger<T>
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}

