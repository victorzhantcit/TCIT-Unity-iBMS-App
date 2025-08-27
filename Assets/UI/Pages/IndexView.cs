using iBMSApp.Services;
using iBMSApp.UI.Common;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace iBMSApp.UI.Pages
{
    public class IndexView : BasePageMonoBehavior
    {
        [SerializeField] private TMP_InputField _ipInputField;
        [SerializeField] private Button _ipApplyButton;

        private ILocalStorageService _localStorage;

        /// <summary>
        /// ����������m�� Unity ���ӳQ���J�� Scene�A<seealso cref="BasePageMonoBehavior.Start()"/> �]�p���Y�J��D�w���������J�A�h����ܦ��� <br></br>
        /// ���������H�~�Ҧ��~�� <see cref="BasePageMonoBehavior"/> �������A������ new void Start() �åB�u���I�s base.Start()
        /// </summary>
        private new void Start() // ���� <seealso cref="BasePageMonoBehavior.Start()"/>
        {
            InitializeOnStart();
        }

        #region Override BaseClass
        /// <inheritdoc/>
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

        private async void InitializeOnStart()
        {
            _localStorage = ServiceManager.Instance.LocalStorageService;
            string localeCode = await _localStorage.GetItemAsStringAsync(ServiceManager.Instance.LanguageKeyWord);
            await SetLanguageAsync(localeCode);

            if (!string.IsNullOrEmpty(localeCode))
            {
                base.GoToSceneLogin();
            }

            _ipApplyButton.onClick.RemoveAllListeners();
            _ipApplyButton.onClick.AddListener(() =>
            {
                if (ServiceManager.Instance.RecoverHttpClientUrl(_ipInputField.text))
                    _ipInputField.gameObject.SetActive(false);
            });
        }

        public void SetLanguage(string localeCode) => _ = SetLanguageAsync(localeCode);

        /// <summary>
        /// �]�w�y��
        /// </summary>
        /// <param name="localeCode">�ھڻy���]�w �y���C��b Unity Editor > [Windows] > [Asset Management] > [Localization Tables]</param>
        private async Task SetLanguageAsync(string localeCode)
        {
            await LocalizationSettings.InitializationOperation.Task; // �� localization ��l��

            var locales = LocalizationSettings.AvailableLocales.Locales;
            var locale = locales.FirstOrDefault(l => l.Identifier.Code == localeCode);

            DebugLog("Locales: " + string.Join(",", locales));
            if (locale == null)
            {
                localeCode = "zh-TW"; // fallback
                locale = locales.FirstOrDefault(l => l.Identifier.Code == localeCode);
            }

            DebugLog("await LocalizationSettings.InitializationOperation.Task");
            LocalizationSettings.SelectedLocale = locale;

            DebugLog("Set locale = " + locale);
            await _localStorage.SetItemAsStringAsync(ServiceManager.Instance.LanguageKeyWord, localeCode);
        }

        private void DebugLog(string message)
        {
            Debug.Log($"[{nameof(IndexView)}] {message}");
        }
    }
}

