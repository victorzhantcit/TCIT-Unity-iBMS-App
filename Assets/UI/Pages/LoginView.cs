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

            TryAutoLogin(); // �[�o��
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

            if (!enabledAutoLogin) // �n�X�L�A�����զ۰ʵn�J
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

                // ���Ĥ@�ӲŦX���⪺�ɦV���|
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

                // �S�����󨤦�ŦX �� ��ܴ���
                Debug.LogWarning(NoRoleDefinedText.GetLocalizedString()); // "�L�ŦX������"
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

                // �۰ʵn�J
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
                Debug.LogWarning(LoginFailedText.GetLocalizedString()); // "�n�J����"
                _toastService.ShowToast(LoginFailedText.GetLocalizedString(), ToastLevel.Warning);

                _loginButton.SetEnumValue(ToggleState.Activated);
                _processingLogin = false;
                return;
            }

            // �o��API�^���ɶ������W�L5�� �]���h��I���B�z (ServiceManager �O Don't Remove On Load)
            _ = ServiceManager.Instance.EqptService.GetDevices("RG");

            Dictionary<UserRole, Action> roleNavMap = new()
            {
                { UserRole.QC, () => GoToSceneInspOrderList() },
                { UserRole.Staff, () => GoToSceneInspOrderList() },
                { UserRole.Insp, () => GoToSceneInspOrderList() },
                { UserRole.Maint, () => GoToSceneWorkOrderList() },
                { UserRole.DeviceMaint, () => GoToSceneWorkOrderList() }
            };

            // ���Ĥ@�ӲŦX���⪺�ɦV���|
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

            // �S�����󨤦�ŦX �� ��ܴ���
            Debug.LogWarning(NoRoleDefinedText.GetLocalizedString()); // "�L�ŦX������"
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

        private static string Encrypt(string inputString) //�D�ϥ�
        {
            // �N�r���ഫ���줸�հ}�C
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputString);

            // �N���_�ഫ���줸�հ}�C
            byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);

            // �i��[�K
            byte[] encryptedBytes = new byte[inputBytes.Length];
            for (int i = 0; i < inputBytes.Length; i++)
            {
                encryptedBytes[i] = (byte)(inputBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            // �N�[�K���G�ഫ��Base64�r��
            string encryptedString = Convert.ToBase64String(encryptedBytes);

            return encryptedString;
        }

        private static string Decrypt(string encryptedString)
        {
            // �N�[�K�r���ഫ���줸�հ}�C
            byte[] encryptedBytes = Convert.FromBase64String(encryptedString);

            // �N���_�ഫ���줸�հ}�C
            byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);

            // �i��ѱK
            byte[] decryptedBytes = new byte[encryptedBytes.Length];
            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                decryptedBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            // �N�ѱK���G�ഫ���r��
            string decryptedString = Encoding.UTF8.GetString(decryptedBytes);

            return decryptedString;
        }
    }
}
