namespace iBMSApp.Services
{
    public class UnityLogger<T> : ILogger<T>
    {
        private readonly string _category;

        public UnityLogger()
        {
            _category = typeof(T).Name;
        }

        public void LogInformation(string message)
        {
            UnityEngine.Debug.Log($"[INFO][{_category}] {message}");
        }

        public void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning($"[WARN][{_category}] {message}");
        }

        public void LogError(string message)
        {
            UnityEngine.Debug.LogError($"[ERROR][{_category}] {message}");
        }
    }
}

