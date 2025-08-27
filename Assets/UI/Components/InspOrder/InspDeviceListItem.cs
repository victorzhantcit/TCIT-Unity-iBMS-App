using iBMSApp.App;
using iBMSApp.Services;
using iBMSApp.Shared;
using iBMSApp.UI.Common;
using iBMSApp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace iBMSApp.UI.Components
{
    public class InspDeviceListItem : MonoBehaviour
    {
        private static readonly LocalizedStyle LabelLocalizedStyle =
            LocalizedStyle.LabelLocalized | LocalizedStyle.Comma;

        [Serializable]
        public class OptionSprite
        {
            public EnumDeviceListItemStatus Option;
            public Sprite Icon;
        }

        #region UI From Inspector
        [Header("Config")]
        [SerializeField] private List<OptionSprite> _statusVisualConfigs;

        [Header("Title")]
        [SerializeField] private TMP_Text _deviceDescription;
        [SerializeField] private TMP_Text _statusLabel;
        [SerializeField] private Image _statusBackground;
        [SerializeField] private VisualButtonHelper _deviceFunctionalButton;

        [Header("SubTitle")]
        [SerializeField] private TMP_Text _formNames;

        [Header("Overview")]
        [SerializeField] private TMP_Text _deviceOverview;
        [SerializeField] private VisualImageText _imageSummarize;

        [Header("Content")]
        [SerializeField] private RectTransform _deviceContent;
        [SerializeField] private Button _collapseDeviceButton;
        [SerializeField] private RectTransform[] _breakLines;

        [Header("Device Info")]
        [SerializeField] private TMP_Text _deviceInfo;

        [Header("Numerical Data")]
        [SerializeField] private RectTransform _numericalDataContent;
        [SerializeField] private NumericalDataRow _numericalDataContentPrefab;
        [SerializeField] private Button _saveNumericalDataButton;

        [Header("Consumable Data")]
        [SerializeField] private TMP_Text _consumableTitle;
        [SerializeField] private RectTransform _consumableDataContent;
        [SerializeField] private ConsumableDataPanel _consumableDataContentPrefab;
        [SerializeField] private Button _saveConsumableDataButton;

        [Header("Check Items")]
        [SerializeField] private TMP_Text _checkItemTitle;
        [SerializeField] private RectTransform _checkItemsContent;
        [SerializeField] private CheckItemPanel _checkItemPrefab;

        [Header("Photos")]
        [SerializeField] private PhotoPanelsVirtualList _photosVirtualList;
        [SerializeField] private Button _addPhotoButton;

        [Header("Issue")]
        [SerializeField] private TMP_Text _issueInfo;
        [SerializeField] private TMP_InputField _issueInputField;
        [SerializeField] private Button _updateDeviceButton;
        [SerializeField] private Button _submitDeviceButton;
        #endregion

        private ObjectPool<NumericalDataRow> _numericalDataContentPool;
        private ObjectPool<ConsumableDataPanel> _consumableDataContentPool;
        private ObjectPool<CheckItemPanel> _checkItemsContentPool;
        private List<ImageFile> _photoFiles = new List<ImageFile>();

        private RectTransform _pageContentRef;
        private EqptOrderDevice _device;
        private bool _orderIsRejected;

        private Action<string, ToastLevel> _toastMessageEvent = null;
        private Action _functionalButtonEvent = null;
        private Action _saveNumericalDataEvent = null;
        private Action _saveConsumableDataEvent = null;
        private Action _submitDeviceEvent = null;
        private Action _updateDeviceEvent = null;
        private Action _collapseDeviceEvent = null;

        private static string MakeInfo(string label, string value)
        {
            return Localizer.GenerateLocalizedRichText(
                label,
                value,
                LocalizedStyle.Comma
            );
        }

        private static string MakeLabelLocalizedInfo(string key, string value)
        {
            return Localizer.GenerateLocalizedRichText(
                key,
                value,
                LabelLocalizedStyle
            );
        }

        public void Start()
        {
            _numericalDataContentPool = new ObjectPool<NumericalDataRow>(_numericalDataContentPrefab, _numericalDataContent);
            _consumableDataContentPool = new ObjectPool<ConsumableDataPanel>(_consumableDataContentPrefab, _consumableDataContent);
            _checkItemsContentPool = new ObjectPool<CheckItemPanel>(_checkItemPrefab, _checkItemsContent);
        }

        public void BindEventToastMessage(Action<string, ToastLevel> action) => _toastMessageEvent = action;

        public void OnFunctionalButtonClicked() => _functionalButtonEvent?.Invoke();

        public void BindEventSaveNumericalData(Action action) => _saveNumericalDataEvent = action;

        public void OnSaveNumericalDataClicked() => _saveNumericalDataEvent?.Invoke();

        public void BindEventSaveConsumableData(Action action) => _saveConsumableDataEvent = action;

        public void OnSaveConsumableDataClicked() => _saveConsumableDataEvent?.Invoke();

        public void OnAddPhotoClicked() => AddPhoto();

        public void OnSubmitDeviceClicked() => _submitDeviceEvent?.Invoke();

        public void BindEventSubmitDevice(Action action) => _submitDeviceEvent = action;

        public void OnUpdateDeviceClicked() => _updateDeviceEvent?.Invoke();

        public void BindEventUpdateDevice(Action action) => _updateDeviceEvent = action;

        public void OnCollapseDeviceClicked() => _collapseDeviceEvent?.Invoke();

        public void BindEventCollapseDevice(Action action) => _collapseDeviceEvent = action;

        /// <summary>
        /// 有時UI更新需要以"頁面"為基準更新RectTransform會需要此設定
        /// </summary>
        /// <param name="pageContent">應指向匯集頁面UI的根物件</param>
        public void SetPageContentRef(RectTransform pageContent) => _pageContentRef = pageContent;

        public void BindData(EqptOrderDevice device, List<ImageFile> filesBase64, bool isRejected)
        {
            _device = device;
            _photoFiles = filesBase64;
            _orderIsRejected = isRejected;
            _deviceDescription.text = device.DeviceDescription;

            string statusKey;
            string statusColorKey;
            if (device.SubmitTime != null)
            {
                statusKey = "S_Processed";
                statusColorKey = "bgc-processed";
            }
            else if (device.PauseTime != null)
            {
                statusKey = "S_Pause";
                statusColorKey = "bgc-processing";
            }
            else if (device.StartTime != null)
            {
                statusKey = "S_Processing";
                statusColorKey = "bgc-processing";
            }
            else
            {
                statusKey = "S_Pending";
                statusColorKey = "bgc-pending";
            }

            _statusLabel.text = Localizer.Instance.GetLocalizedString(statusKey);
            _statusBackground.color = ColorMapper.Instance.GetColor(statusColorKey);
            _formNames.text = MakeLabelLocalizedInfo("T_CheckForm", string.Join(",", device.FormNames));
        }

        public void SetFunctionalStatus(EnumDeviceListItemStatus infoStatus, Action buttonAction)
        {
            var statusVisualConfig = _statusVisualConfigs.FirstOrDefault(x => x.Option == infoStatus);

            if (statusVisualConfig == null)
            {
                Debug.LogWarning($"Status not defined in {typeof(InspDeviceListItem).Name} config");
                return;
            }

            _deviceFunctionalButton.UpdateIcon(statusVisualConfig.Icon);
            _functionalButtonEvent = buttonAction;

            if (infoStatus == EnumDeviceListItemStatus.Expand || infoStatus == EnumDeviceListItemStatus.QRScan)
                UpdateOverview();
            else
                UpdateContent();
        }

        private void SelectDisplayContent(bool isDetailed)
        {
            _deviceContent.gameObject.SetActive(isDetailed);
            _deviceOverview.gameObject.SetActive(!isDetailed);
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
        }

        private void UpdateOverview()
        {
            StringBuilder sb = new StringBuilder();

            string spendTimeTitle = $"{Localizer.Instance.GetLocalizedString("T_Spend")}" +
                $" ({Localizer.Instance.GetLocalizedString("分鐘")})";
            string spendTimeValue = (_device.ManMinute >= 0) ? _device.ManMinute.ToString() : "--";
            string respond = string.IsNullOrEmpty(_device.Respond) ? "--" : _device.Respond;
            string respondInfo = MakeLabelLocalizedInfo("T_Content", respond);

            sb.AppendLine(MakeInfo(spendTimeTitle, spendTimeValue));
            sb.AppendLine(respondInfo);
            _deviceOverview.text = sb.ToString();
            sb.Clear();

            int num = 0;
            if (!string.IsNullOrEmpty(_device.AfPhotoSns))
            {
                num = _device.AfPhotoSns.Split(',').Length;
            }
            if (num != 0 && _device.AfPhotoSns != null)
            {
                _imageSummarize.UpdateText($"({num})");
                _imageSummarize.Show();
            }
            else
            {
                _imageSummarize.Hide();
            }

            SelectDisplayContent(false);
        }

        private void UpdateContent()
        {
            RefreshBaseInfo();
            RefreshNumericalDatas();
            RefreshConsumables();
            RefreshCheckItems();
            RefreshPhotos();
            RefreshIssue();

            foreach (RectTransform breakLine in _breakLines)
            {
                breakLine.gameObject.SetActive(!_device.IsDisabled);
            }

            _submitDeviceButton.gameObject.SetActive(!_device.IsDisabled);

            SelectDisplayContent(true);
        }

        public void RefreshBaseInfo()
        {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(_device.Manufacturer))
                sb.AppendLine(MakeLabelLocalizedInfo("T_Manufacturer", _device.Manufacturer));
            if (!string.IsNullOrEmpty(_device.ModelNumber))
                sb.AppendLine(MakeLabelLocalizedInfo("T_ModelNumber", _device.ModelNumber));
            if (!string.IsNullOrEmpty(_device.Executor))
                sb.AppendLine(MakeLabelLocalizedInfo("T_InspBy", _device.Executor));
            if (_device.WarrantyTime != null)
            {
                string warrantyInfo = MakeLabelLocalizedInfo("T_WarrantyDate", _device.WarrantyEndDate);
                if (_device.WarrantyTime < DateTime.Now)
                    warrantyInfo += $"<color=red>（{Localizer.Instance.GetLocalizedString("S_OverWarranty")}）</color>";
                sb.AppendLine(warrantyInfo);
            }

            string estimatedTimeTitle = $"{Localizer.Instance.GetLocalizedString("T_Estimated")} ({Localizer.Instance.GetLocalizedString("分鐘")})";
            string estimatedInfo = MakeInfo(estimatedTimeTitle, _device.EstManMinute.ToString());
            sb.AppendLine(estimatedInfo);

            if (_device.NumericalData.Count > 0)
            {
                string dataTimeInfo = Localizer.GenerateLocalizedRichText(
                    "T_DeviceData",
                    (_device.DataTime == null) ? "" : _device.DataTime,
                    LocalizedStyle.LabelLocalized | LocalizedStyle.DualAligned | LocalizedStyle.Comma
                );
                sb.AppendLine(dataTimeInfo);
            }

            _deviceInfo.text = sb.ToString();
            sb.Clear();
        }

        public void RefreshNumericalDatas()
        {
            ClearDeviceNumbericalDataList();
            bool hasNumericalData = _device.NumericalData.Count > 0;

            if (!hasNumericalData)
            {
                _saveNumericalDataButton.gameObject.SetActive(false);
                return;
            }

            int siblingIndex = 0;

            for (int i = 0; i < _device.NumericalData.Count; i += 2)
            {
                int nextIndex = i + 1;
                var data1 = _device.NumericalData[i];
                var data2 = (nextIndex < _device.NumericalData.Count) ? _device.NumericalData[nextIndex] : null;
                NumericalDataRow numericalDataRow = _numericalDataContentPool.Get();

                numericalDataRow.UpdateNumericalDatas(_device.IsDisabled, data1, data2);
                numericalDataRow.transform.SetSiblingIndex(siblingIndex);
                siblingIndex++;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(_numericalDataContent);
            _saveNumericalDataButton.gameObject.SetActive(!_device.IsDisabled);
        }

        private void ClearDeviceNumbericalDataList()
        {
            foreach (Transform item in _numericalDataContent)
            {
                if (item.gameObject.activeSelf == false)
                {
                    return;
                }
                if (item.TryGetComponent<NumericalDataRow>(out var itemComponent))
                {
                    _numericalDataContentPool.Release(itemComponent);
                }
            }
            Canvas.ForceUpdateCanvases();
        }

        public void RefreshConsumables()
        {
            bool ownedConsumable = _device.Consumables.Count > 0;
            _consumableTitle.gameObject.SetActive(ownedConsumable);
            ClearDeviceConsumableDataList();
            if (ownedConsumable)
            {
                int siblingIndex = 0;
                foreach (var consumable in _device.Consumables)
                {
                    ConsumableDataPanel consumableDataRow = _consumableDataContentPool.Get();

                    consumableDataRow.Bind(consumable, _device.IsDisabled);
                    consumableDataRow.transform.SetSiblingIndex(siblingIndex);
                    siblingIndex++;
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(_consumableDataContent);
            }
            _saveConsumableDataButton.gameObject.SetActive(!_device.IsDisabled && ownedConsumable);
        }

        private void ClearDeviceConsumableDataList()
        {
            foreach (Transform item in _consumableDataContent)
            {
                if (item.gameObject.activeSelf == false)
                {
                    return;
                }
                if (item.TryGetComponent<ConsumableDataPanel>(out var itemComponent))
                {
                    _consumableDataContentPool.Release(itemComponent);
                }
            }
            Canvas.ForceUpdateCanvases();
        }

        public void RefreshCheckItems()
        {
            ClearCheckItemList();
            int siblingIndex = 0;
            foreach (EqptOrderItem checkItem in _device.Items)
            {
                CheckItemPanel checkItemPanel = _checkItemsContentPool.Get();

                checkItemPanel.Bind(checkItem, _device.IsDisabled, siblingIndex + 1);
                checkItemPanel.transform.SetSiblingIndex(siblingIndex);
                siblingIndex++;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(_checkItemsContent);
        }

        private void ClearCheckItemList()
        {
            foreach (Transform item in _checkItemsContent)
            {
                if (item.gameObject.activeSelf == false)
                {
                    return;
                }
                if (item.TryGetComponent<CheckItemPanel>(out var itemComponent))
                {
                    _checkItemsContentPool.Release(itemComponent);
                }
            }
            Canvas.ForceUpdateCanvases();
        }

        public void RefreshPhotos()
        {
            bool hasPhotos = _photoFiles.Count > 0;
            bool hitPhotoUploadLimit = _photoFiles.Count >= 5;
            bool isDisabled = _device.IsDisabled;

            _addPhotoButton.gameObject.SetActive(!isDisabled && !hitPhotoUploadLimit);

            _photosVirtualList.BindList(_photoFiles, isDisabled, (item, data) =>
            {
                item.BindEventAfterRawImageUpdated(RefreshPageLayout);
                item.BindDeletePhotoEvent(() =>
                {
                    _photosVirtualList.ClearItem(item);
                    _photoFiles.Remove(data);
                    RefreshPageLayout();
                });
            });
        }

        public void RefreshPageLayout()
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_pageContentRef);
        }

        // 呼叫原生相機並於完成後加入圖片至設備附圖
        private void AddPhoto()
        {
            NativeCameraManager.TakePictureBase64(null, base64Img =>
            {
                if (_photoFiles.Count >= 5)
                {
                    _toastMessageEvent?.Invoke(Localizer.Instance.GetLocalizedString("MSG016"), ToastLevel.Warning);
                }
                else
                {
                    _photoFiles.Add(new ImageFile { base64data = base64Img, contentType = "", fileName = "" });
                    RefreshPhotos();
                }
            });
        }

        public void RefreshIssue()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(MakeLabelLocalizedInfo("T_Summarize", (_device.IsDisabled) ? _device.Respond : ""));
            if (_device.IsDisabled && _device.SubmitTime != null)
                sb.AppendLine(MakeLabelLocalizedInfo("T_UploadTime", _device.SubmitTime));
            _issueInfo.text = sb.ToString();
            sb.Clear();

            _issueInputField.gameObject.SetActive(!_device.IsDisabled);
            _issueInputField.onEndEdit.RemoveAllListeners();
            if (!_device.IsDisabled)
                _issueInputField.onEndEdit.AddListener((newRespond) =>
                {
                    _device.Respond = newRespond;
                    Debug.Log("[InspDeviceListItem] End Edit: " + newRespond);
                });
            _collapseDeviceButton.gameObject.SetActive(_device.IsDisabled);
            _updateDeviceButton.gameObject.SetActive(_device.IsDisabled && _orderIsRejected);
            _submitDeviceButton.gameObject.SetActive(!_device.IsDisabled);
        }
    }
}
