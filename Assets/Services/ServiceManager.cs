using iBMSApp.UI.Common;
using System;
using System.Net.Http;
using UnityEngine;

namespace iBMSApp.Services
{
    public class ServiceManager : MonoBehaviour
    {
        public static ServiceManager Instance { get; private set; }
        public AuthService AuthService { get; private set; }
        public EqptService EqptService { get; private set; }

        public QRCodeDetector QRCodeDetector => _qrCodeDetector;
        public ColorMapper ColorMapper => _colorMapper;
        public Localizer Localizer => _localizer;
        public UnityToastService ToastService => _toastService;

        public string LanguageKeyWord => "i18nextLng";

        [SerializeField] private UnityToastService _toastService;
        [SerializeField] private Localizer _localizer;
        [SerializeField] private ColorMapper _colorMapper;
        [SerializeField] private QRCodeDetector _qrCodeDetector;

        public ILocalStorageService LocalStorageService { get; private set; }

        // «áºÝ API ©I¥s¤J¤f
        [SerializeField] private string BackendUrl = string.Empty;

        void Awake()
        {
            // Unity Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // DI Define
            DIContainer.RegisterLogger<AuthService>();
            DIContainer.RegisterLogger<EqptService>();
            DIContainer.RegisterSingleton<ILocalStorageService, UnityLocalStorageService>();
            DIContainer.RegisterSingleton(() => new HttpClient() { BaseAddress = new Uri(BackendUrl) });
            DIContainer.RegisterSingleton(() => new UnityDbAccessor());

            // Services Initialize
            InitServices();
        }

        private void InitServices()
        {
            LocalStorageService = DIContainer.Resolve<ILocalStorageService>();
            AuthService = new AuthService(
                DIContainer.Resolve<ILocalStorageService>(),
                DIContainer.Resolve<ILogger<AuthService>>(),
                DIContainer.Resolve<HttpClient>()
            );
            EqptService = new EqptService(
                DIContainer.Resolve<ILocalStorageService>(),
                DIContainer.Resolve<ILogger<EqptService>>(),
                DIContainer.Resolve<HttpClient>(),
                DIContainer.Resolve<UnityDbAccessor>()
            );
        }

        public static ILogger<T> GetLogger<T>()
        {
            try
            {
                if (!DIContainer.Has<ILogger<T>>())
                {
                    DIContainer.RegisterLogger<T>();
                }

                return DIContainer.Resolve<ILogger<T>>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get ILogger<{typeof(T).Name}>", ex);
            }
        }

        public bool IsNetworkAvailable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        public bool RecoverHttpClientUrl(string url)
        {

            if (DIContainer.Has<HttpClient>())
            {
                DIContainer.Resolve<HttpClient>()?.Dispose();
            }

            Uri backendUri;

            try
            {
                backendUri = new Uri(url);
            }
            catch (UriFormatException ex)
            {
                Debug.LogWarning("Invalid URL format: " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError("Error setting up HttpClient: " + ex.Message);
                return false;
            }

            BackendUrl = url;
            DIContainer.RegisterSingleton(() => new HttpClient() { BaseAddress = backendUri });
            Debug.Log("Update to " + BackendUrl);
            InitServices();
            return true;
        }
    }
}

