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
        /// 此頁面應放置於 Unity 首個被載入的 Scene，<seealso cref="BasePageMonoBehavior.Start()"/> 設計為若遇到非預期頁面載入，則跳轉至此頁 <br></br>
        /// 除此頁面以外所有繼承 <see cref="BasePageMonoBehavior"/> 的頁面，都應該 new void Start() 並且優先呼叫 base.Start()
        /// </summary>
        private new void Start() // 隱藏 <seealso cref="BasePageMonoBehavior.Start()"/>
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
        /// 設定語言
        /// </summary>
        /// <param name="localeCode">根據語言設定 語言列表在 Unity Editor > [Windows] > [Asset Management] > [Localization Tables]</param>
        private async Task SetLanguageAsync(string localeCode)
        {
            await LocalizationSettings.InitializationOperation.Task; // 等 localization 初始化

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

