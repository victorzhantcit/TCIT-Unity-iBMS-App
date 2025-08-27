using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace iBMSApp.UI.Layout
{
    public class MainLayout : MonoBehaviour
    {
        private enum DeviceOrientation
        {
            Portrait,
            Landscape,
            Unknown
        }

        [Serializable]
        private class LayoutVisualOptions
        {
            public RectTransform VisualObject;
            public RectTransform PortraitModeParent;
            public RectTransform LandscapeModeParent;
        }

        [Serializable]
        private class LayoutBoolOptions
        {
            public RectTransform VisualObject;
            public DeviceOrientation VisibleOrientation;
            public DeviceOrientation AutoToggledOrientation;
        }

        [Tooltip("寬高比大於這個值會切換到橫式排版")]
        public float aspectThreshold = 1.33f;

        [SerializeField] List<LayoutVisualOptions> _layoutVisualObjects;
        [SerializeField] List<LayoutBoolOptions> _layoutBoolOptions;
        
        private DeviceOrientation _deviceOrientation = DeviceOrientation.Unknown;

        public bool IsPortraitMode => _deviceOrientation == DeviceOrientation.Portrait;

        void Start()
        {
            RecordOrientation();
            UpdateLayout();
        }

        void LateUpdate()
        {
            if (IsDeviceOrientationChanged())
            {
                RecordOrientation();
                UpdateLayout();
            }
        }

        private bool IsDeviceOrientationChanged()
        {
            return GetCurrentDeviceOrientation() != _deviceOrientation;
        }

        private DeviceOrientation GetCurrentDeviceOrientation()
        {
            float aspect = (float)Screen.width / Screen.height;
            DeviceOrientation deviceOrientation = aspect > aspectThreshold ? DeviceOrientation.Landscape : DeviceOrientation.Portrait;

            return deviceOrientation;
        }

        private void RecordOrientation()
        {
            _deviceOrientation = GetCurrentDeviceOrientation();
        }

        private void UpdateLayout()
        {
            foreach (LayoutVisualOptions options in _layoutVisualObjects)
            {
                RectTransform targetModeParent = _deviceOrientation == DeviceOrientation.Landscape ? options.LandscapeModeParent : options.PortraitModeParent;
                options.VisualObject.SetParent(targetModeParent);
                SetFullStretch(options.VisualObject);
            }

            foreach (LayoutBoolOptions options in _layoutBoolOptions)
            {
                bool shouldShow = options.VisibleOrientation == _deviceOrientation;

                options.VisualObject.gameObject.SetActive(shouldShow);

                if (options.VisualObject.TryGetComponent<Toggle>(out var toggle))
                {
                    bool isToggled = options.AutoToggledOrientation == _deviceOrientation;

                    toggle.isOn = isToggled;
                    toggle.onValueChanged.Invoke(isToggled);
                }
            }

            //Canvas.ForceUpdateCanvases();
        }

        private void SetFullStretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;             // (0, 0)
            rectTransform.anchorMax = Vector2.one;              // (1, 1)
            rectTransform.pivot = new Vector2(0.5f, 0.5f);       // 中心點
            rectTransform.anchoredPosition = Vector2.zero;      // 不偏移
            rectTransform.sizeDelta = Vector2.zero;             // 完全填滿
        }

    }
}
