using iBMSApp.Services;
using iBMSApp.Shared;
using iBMSApp.UI.Components;
using iBMSApp.Utility;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace iBMSApp.UI.Pages
{
    public abstract class NetworkPageMonoBehavior<T> : BasePageMonoBehavior
    {
        private static bool _disconnected = false;

        [Header("NetworkPage Base Config")]
        [SerializeField] private PageTitlePanel _pageTitlePanel;
        [SerializeField] private LocalizedString _pageTitleText;
        [SerializeField] private Image _pageLoadingIcon;
        [SerializeField] private Image _progressBar;

        public string NetworkUnreachableText => _pageTitlePanel.NetworkUnreachableText;

        protected AuthService _authService;
        protected IToastService _toastService;
        protected bool _isOnline = false;

        private ILogger<T> _logger;
        private bool _networkUpdateLocker = true;
        private float _targetProgress = 0f;
        private float _lerpSpeed = 3f;

        #region Unity Events
        protected new void Start()
        {
            base.Start();

            _logger = ServiceManager.GetLogger<T>();

            // InitServices
            if (ServiceManager.Instance == null)
                return;

            _toastService = ServiceManager.Instance.ToastService;
            _authService = ServiceManager.Instance.AuthService;

            // InitUI
            Localizer localizer = ServiceManager.Instance.Localizer;

            if (_pageTitlePanel != null)
            {
                _pageTitlePanel.SetNetworkUnreachableText(localizer.GetLocalizedString("T_NoNetwork"));
                _pageTitlePanel.SetPageTitle(_pageTitleText.GetLocalizedString());
            }
            ResetProgressBar();
            UpdateNetworkStatus();
            _networkUpdateLocker = false;
        }

        protected void Update()
        {
            if (_networkUpdateLocker)
                return;

            UpdateNetworkStatus();
            UpdateProgressBarAnimation();
        }
        #endregion

        #region Internal Logic
        private void UpdateNetworkStatus()
        {
            bool currentNetworkStatus = ServiceManager.Instance.IsNetworkAvailable();

            if (currentNetworkStatus != _isOnline)
            {
                if (_pageTitlePanel != null)
                    _pageTitlePanel.SetNetworkIcon(currentNetworkStatus);
                _isOnline = currentNetworkStatus;
                NotifyNetworkChanged(currentNetworkStatus);
            }
        }

        private void UpdateProgressBarAnimation()
        {
            if (_progressBar == null) return;

            // 緩慢逼近目標值
            float current = _progressBar.fillAmount;
            if (Mathf.Abs(current - _targetProgress) > 0.001f)
            {
                _progressBar.fillAmount = Mathf.Lerp(current, _targetProgress, Time.deltaTime * _lerpSpeed);
            }
        }

        private void NotifyNetworkChanged(bool networkReachable)
        {
            if (networkReachable)
            {
                OnNetworkConnected();
            }
            else
            {
                OnNetworkDisconnected();
            }
        }
        #endregion

        #region Protected API for child class

        /// <summary>
        /// 當網路恢復連線時的事件處理
        /// </summary>
        protected virtual void OnNetworkConnected()
        {
            _disconnected = false;
        }

        /// <summary>
        /// 當網路斷線時的事件處理
        /// </summary>
        protected virtual void OnNetworkDisconnected()
        {
            if (_disconnected) return;

            _disconnected = true;
            _toastService.ShowToast(NetworkUnreachableText, ToastLevel.Warning);
        }

        protected async void HandleHttpRequestException<TResult>(ApiResponse<TResult> response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                await _authService.LogoutAsync();
                base.GoToSceneIndex();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) //401
            {
                base.GoToSceneLogin();
            }
            else
            {
                _toastService.ShowToast(response.ErrorMessage ?? "", ToastLevel.Warning);
            }
        }

        protected async Task<UserInfo> GetUserInfoAsync()
        {
            while (_authService == null)
            {
                await Task.Yield();
            }
            return await _authService.GetUserData();
        } 

        protected async Task<string> GetUidAsync()
        {
            UserInfo userInfo = await GetUserInfoAsync();
            return userInfo.Name;
        }

        protected void LoadingPage(bool isActive)
        {
            if (_pageLoadingIcon == null) return;
            _pageLoadingIcon.gameObject.SetActive(isActive);
        }

        protected void SetProgressBar(float value)
        {
            if (!_progressBar.gameObject.activeSelf)
                _progressBar.gameObject.SetActive(true);

            _targetProgress = Mathf.Clamp01(value);
        }

        protected void ResetProgressBar()
        {
            if (_progressBar == null) return;

            _progressBar.fillAmount = 0f;

            if (_progressBar.gameObject.activeSelf)
                _progressBar.gameObject.SetActive(false);
        }

        protected void DebugLog(string message, LogType logType = LogType.Log)
        {
            if (_logger == null) return;

            switch (logType)
            {
                case LogType.Warning:
                    _logger.LogWarning(message);
                    break;
                case LogType.Exception:
                case LogType.Error:
                    _logger.LogError(message);
                    break;
                default: // LogType.Log or any other type
                    _logger.LogInformation(message);
                    break;
            }
        }
        #endregion
    }
}
