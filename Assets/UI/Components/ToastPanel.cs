using iBMSApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace iBMSApp.UI.Components
{
    public class ToastPanel : MonoBehaviour
    {
        [Header("Canvas Override Value")]
        [SerializeField] private int _canvasSortOrder = 999;

        [Header("UI")]
        [SerializeField] private Canvas _toastCanvas;
        [SerializeField] private TextMeshProUGUI _headingText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Image _levelIcon;
        [SerializeField] private Image _backgroundImage;

        [Serializable]
        public class ToastSetting
        {
            public ToastLevel Level;
            public Sprite Icon;
            public LocalizedString LocalizedHeader;
            public Color BackgroundColor;
        }

        public List<ToastSetting> ToastSettings;

        private void Start()
        {
            var toastService = ServiceManager.Instance.ToastService;
            toastService.OnShow += Show;
            toastService.OnHide += Hide;

            Hide(); // 預設隱藏
            _toastCanvas.sortingOrder = _canvasSortOrder;
            
            if (_backgroundImage.TryGetComponent<Canvas>(out var backgroundCanvas))
            {
                backgroundCanvas.sortingOrder = _canvasSortOrder;
            }
        }

        private void Show(string message, ToastLevel level)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                message = " ";
            }

            _messageText.text = message;
            _toastCanvas.gameObject.SetActive(true);
            _toastCanvas.enabled = true;

            ToastSetting setting = ToastSettings.FirstOrDefault(setting => setting.Level == level);

            if (setting == null)
            {
                _levelIcon.sprite = null;
                _headingText.text = "Unknown toast level";
                _backgroundImage.color = Color.gray;
                return;
            }

            // 根據 Toast 等級設定
            _levelIcon.sprite = setting.Icon;
            _headingText.text = setting.LocalizedHeader.GetLocalizedString();
            _backgroundImage.color = setting.BackgroundColor;
        }

        private void Hide()
        {
            _toastCanvas.gameObject.SetActive(false);
            _toastCanvas.enabled = false;
        }
    }
}
