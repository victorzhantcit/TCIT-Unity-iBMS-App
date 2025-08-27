using iBMSApp.Services;
using iBMSApp.Shared;
using iBMSApp.UI.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace iBMSApp.UI.Pages
{
    public class LoginView : BasePageMonoBehavior
    {
        [Header("Localization Texts")]
        public LocalizedString LoginFailedText;
        public LocalizedString NoRoleDefinedText;
        public LocalizedString NoNetworkText;

        [Header("UI")]
        [SerializeField] private TMP_InputField _idInputField;
        [SerializeField] private TMP_InputField _pwInputField;
        [SerializeField] private Toggle _rememberLogin;
        [SerializeField] private VisualToggleSwapper _loginButton;

        // Services(DI)
        private AuthService _authService;
        private IToastService _toastService;
        private ILocalStorageService _localStorage;

        private bool _processingLogin = false;

        #region Override BaseClass
        /// <inheritdoc/>
        private new void Start()
        {
            base.Start(); 
            
            if (!LoadServices())
                return;

            TryAutoLogin(); // 加這行
        }

        public override void Show()
        {
            this.gameObject.SetActive(true);
        }

        /// <inheritdoc/>
        public override void Hide()
        {
            this.gameObject.SetActive(false);
        }

        /// <inheritdoc/>
        public override Task RefreshPage(PageRefreshParams refreshParams = null)
        {
            return Task.CompletedTask;
        }
        #endregion

        private bool LoadServices()
        {
            if (ServiceManager.Instance == null)
                return false;

            _authService = ServiceManager.Instance.AuthService;
            _toastService = ServiceManager.Instance.ToastService;
            _localStorage = ServiceManager.Instance.LocalStorageService;
            return true;
        }

        private async void TryAutoLogin()
        {
            bool enabledAutoLogin = await _localStorage.GetItemAsync<bool>("AutoLogin");

            if (!enabledAutoLogin) // 登出過，不嘗試自動登入
            {
                return;
            }

            string savedLoginString = await _localStorage.GetItemAsStringAsync("SavedLogin");
            if (string.IsNullOrWhiteSpace(savedLoginString)) return;

            if (!ServiceManager.Instance.IsNetworkAvailable())
            {
                Dictionary<UserRole, Action<bool>> roleNavMap = new()
                {
                    { UserRole.QC, GoToSceneInspOrderList },
                    { UserRole.Staff, GoToSceneInspOrderList },
                    { UserRole.Insp, GoToSceneInspOrderList },
                    { UserRole.Maint, GoToSceneWorkOrderList },
                    { UserRole.DeviceMaint, GoToSceneWorkOrderList }
                };

                // 找到第一個符合角色的導向路徑
                foreach (var (role, navigationAction) in roleNavMap)
                {
                    if (await _authService.isInRole(role))
                    {
                        navigationAction?.Invoke(true);
                        _loginButton.SetEnumValue(ToggleState.Activated);
                        _toastService.ShowToast(NoNetworkText.GetLocalizedString(), ToastLevel.Warning);
                        return;
                    }
                }

                // 沒有任何角色符合 → 顯示提示
                Debug.LogWarning(NoRoleDefinedText.GetLocalizedString()); // "無符合之角色"
                //_toastService.ShowToast(NoRoleDefinedText.GetLocalizedString(), ToastLevel.Warning);
                _toastService.ShowToast(NoNetworkText.GetLocalizedString(), ToastLevel.Warning);
                _loginButton.SetEnumValue(ToggleState.Activated);
            }

            string decrypted = Decrypt(savedLoginString);

            string[] user = decrypted.Split(',');
            if (user.Length == 2)
            {
                _idInputField.text = user[0];
                _pwInputField.text = user[1];

                _rememberLogin.isOn = true;

                // 自動登入
                _ = LogUser(user[0], user[1]);
            }
            else
            {
                _rememberLogin.isOn = false;
            }
        }

        public void OnBackClicked() => GoToPreviousPage();

        private async void GoToPreviousPage()
        {
            await _localStorage.SetItemAsStringAsync(ServiceManager.Instance.LanguageKeyWord, "");
            base.GoToSceneIndex();
        }

        public void Login()
        {
            _ = LogUser(_idInputField.text, _pwInputField.text);
        }

        private async Task LogUser(string id, string password)
        {            
            if (_processingLogin)
            {
                return;
            }

            _processingLogin = true;
            _loginButton.SetEnumValue(ToggleState.Deactivated);

            UserInfo userInfo = new UserInfo()
            {
                Id = id,
                Password = password
            };

            string result = await _authService.LoginAsync(userInfo);

            if (!result.Equals("OK"))
            {
                Debug.LogWarning(LoginFailedText.GetLocalizedString()); // "登入失敗"
                _toastService.ShowToast(LoginFailedText.GetLocalizedString(), ToastLevel.Warning);

                _loginButton.SetEnumValue(ToggleState.Activated);
                _processingLogin = false;
                return;
            }

            // 這隻API回應時間平均超過5秒 因此搬到背景處理 (ServiceManager 是 Don't Remove On Load)
            _ = ServiceManager.Instance.EqptService.GetDevices("RG");

            Dictionary<UserRole, Action> roleNavMap = new()
            {
                { UserRole.QC, () => GoToSceneInspOrderList() },
                { UserRole.Staff, () => GoToSceneInspOrderList() },
                { UserRole.Insp, () => GoToSceneInspOrderList() },
                { UserRole.Maint, () => GoToSceneWorkOrderList() },
                { UserRole.DeviceMaint, () => GoToSceneWorkOrderList() }
            };

            // 找到第一個符合角色的導向路徑
            foreach (var (role, goToDefaultPage) in roleNavMap)
            {
                if (await _authService.isInRole(role))
                {
                    _processingLogin = false;
                    SaveRememberInfo(id, password);
                    _loginButton.SetEnumValue(ToggleState.Activated);
                    Debug.Log($"User {id} logged in with role: {role}, GoTo ");
                    goToDefaultPage();
                    return;
                }
            }

            // 沒有任何角色符合 → 顯示提示
            Debug.LogWarning(NoRoleDefinedText.GetLocalizedString()); // "無符合之角色"
            _toastService.ShowToast(NoRoleDefinedText.GetLocalizedString(), ToastLevel.Warning);
            _loginButton.SetEnumValue(ToggleState.Activated);
            _processingLogin = false;
            SaveRememberInfo(id, password);
        }

        private void SaveRememberInfo(string id, string password)
        {
            if (_rememberLogin.isOn)
            {
                string encrypted = Encrypt($"{id},{password}");
                _localStorage.SetItemAsStringAsync("SavedLogin", encrypted);
            }
            else
            {
                _localStorage.RemoveItemAsync("SavedLogin");
            }

            _localStorage.SetItemAsync<bool>("AutoLogin", _rememberLogin.isOn);
        }

        private static string EncryptionKey = "TCT2023EncryptionKey";

        private static string Encrypt(string inputString) //非使用
        {
            // 將字串轉換成位元組陣列
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputString);

            // 將金鑰轉換成位元組陣列
            byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);

            // 進行加密
            byte[] encryptedBytes = new byte[inputBytes.Length];
            for (int i = 0; i < inputBytes.Length; i++)
            {
                encryptedBytes[i] = (byte)(inputBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            // 將加密結果轉換成Base64字串
            string encryptedString = Convert.ToBase64String(encryptedBytes);

            return encryptedString;
        }

        private static string Decrypt(string encryptedString)
        {
            // 將加密字串轉換成位元組陣列
            byte[] encryptedBytes = Convert.FromBase64String(encryptedString);

            // 將金鑰轉換成位元組陣列
            byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);

            // 進行解密
            byte[] decryptedBytes = new byte[encryptedBytes.Length];
            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                decryptedBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            // 將解密結果轉換成字串
            string decryptedString = Encoding.UTF8.GetString(decryptedBytes);

            return decryptedString;
        }
    }
}
