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
    public class WorkDeviceListItem : MonoBehaviour, IObjectPoolItem<WorkDeviceItemData>
    {
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

        [Header("Overview")]
        [SerializeField] private TMP_Text _deviceOverview;
        [SerializeField] private VisualImageText _imageSummarize;

        [Header("Content")]
        [SerializeField] private RectTransform _deviceContent;
        [SerializeField] private Button _collapseDeviceButton;
        [SerializeField] private RectTransform[] _breakLines;

        [Header("Respond Records")]
        [SerializeField] private TMP_Text _respondRecordsTitle;
        [SerializeField] private RespondRecordVirtualList _respondRecordsVirtualList;

        [Header("Pre-Photos")]
        [SerializeField] private TMP_Text _prePhotosTitle;
        [SerializeField] private PhotoPanelsVirtualList _prePhotosVirtualList;
        [SerializeField] private RectTransform _addPrePhotoArea; // Content AddPrePhotoButton
        [SerializeField] private Button _savePrePhotosButton;

        [Header("Consumables")]
        [SerializeField] private TMP_Text _consumablesTitle;
        [SerializeField] private ConsumablesVirtualList _consumablesVirtualList;
        [SerializeField] private Button _saveConsumablesDataButton;

        [Header("Af-Photos")]
        [SerializeField] private TMP_Text _afPhotosTitle;
        [SerializeField] private PhotoPanelsVirtualList _afPhotosVirtualList;
        [SerializeField] private RectTransform _addAfPhotoArea; // Content AddAfPhotoButton

        [Header("Process Records")]
        [SerializeField] private TMP_Text _processRecordsTitle;
        [SerializeField] private RespondRecordVirtualList _processRecordsVirtualList;
        
        [Header("Respond")]
        [SerializeField] private TMP_Text _respondTitle;
        [SerializeField] private TMP_InputField _respondInputField;
        [SerializeField] private TMP_Text _lastUpdatedTime;

        [Header("Action Buttons")]
        [SerializeField] private RectTransform _processingButtonArea;
        [SerializeField] private Button _startDeviceWorkButton;

        #endregion

        private List<ImageFile> _prePhotoFiles = new List<ImageFile>();
        private List<ImageFile> _afPhotoFiles = new List<ImageFile>();

        private RectTransform _pageContentRef;
        private EqptOrderDevice _device;
        private bool _orderIsRejected;

        private Action<string, ToastLevel> _toastMessageEvent = null;
        private Action<int> _submitDeviceEvent = null;
        private Action _startDeviceWorkEvent = null;
        private Action _functionalButtonEvent = null;
        private Action _savePrePhotosEvent = null;
        private Action _saveConsumableDataEvent = null;
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
                LocalizedStyle.LabelLocalized | LocalizedStyle.Comma
            );
        }
        private static string MakeLabelLocalizedDualInfo(string key, string value)
        {
            return Localizer.GenerateLocalizedRichText(
                key,
                value,
                LocalizedStyle.LabelLocalized | LocalizedStyle.DualAligned
            );
        }

        public void BindEventToastMessage(Action<string, ToastLevel> action) => _toastMessageEvent = action;

        public void OnFunctionalButtonClicked() => _functionalButtonEvent?.Invoke();

        public void BindEventSavePrePhotos(Action action) => _savePrePhotosEvent = action;

        public void OnSavePrePhotosClicked() => _savePrePhotosEvent?.Invoke();

        public void BindEventSaveConsumableData(Action action) => _saveConsumableDataEvent = action;

        public void OnSaveConsumableDataClicked() => _saveConsumableDataEvent?.Invoke();

        public void OnAddPrePhotoClicked() => AddPrePhoto();

        public void OnAddAfPhotoClicked() => AddAfPhoto();

        public void OnDeviceRecordClicked() => _submitDeviceEvent?.Invoke(0);

        public void OnDevicePauseClicked() => _submitDeviceEvent?.Invoke(1);

        public void OnDeviceSubmitClicked() => _submitDeviceEvent?.Invoke(2);

        public void BindEventDeviceSubmit(Action<int> action) => _submitDeviceEvent = action;

        public void OnDeviceWorkStartClicked() => _startDeviceWorkEvent?.Invoke();

        public void BindEventDeviceWorkStart(Action action) => _startDeviceWorkEvent = action;

        public void OnCollapseClicked() => _collapseDeviceEvent?.Invoke();

        public void BindEventDeviceCollapse(Action action) => _collapseDeviceEvent = action;

        /// <summary>
        /// 有時UI更新需要以"頁面"為基準更新RectTransform會需要此設定
        /// </summary>
        /// <param name="pageContent">應指向匯集頁面UI的根物件</param>
        public void SetPageContentRef(RectTransform pageContent) => _pageContentRef = pageContent;

        /// 因為要綁定的 ref 多於一個，因此 Bind 應該由 <see cref="BindData(EqptOrderDevice, List{ImageFile}, List{ImageFile}, bool)"/> 執行
        /// <inheritdoc/>
        public void Bind(WorkDeviceItemData bindingReference, bool isDisabled)
        {
            
        }

        /// <inheritdoc/>
        public void Show()
        {
            this.gameObject.SetActive(true);
        }

        /// <inheritdoc/>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }

        public void BindData(EqptOrderDevice device, List<ImageFile> prePhotoList, List<ImageFile> afPhotoList, bool isRejected)
        {
            _device = device;
            _prePhotoFiles = prePhotoList;
            _afPhotoFiles = afPhotoList;
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

            string spendTimeTitle = $"{Localizer.Instance.GetLocalizedString("T_Spend")} ({Localizer.Instance.GetLocalizedString("分鐘")})";
            string spendTimeValue = (_device.ManMinute >= 0) ? _device.ManMinute.ToString() : "--";
            string respond = string.IsNullOrEmpty(_device.Respond) ? "--" : _device.Respond;
            string remarkInfo = MakeLabelLocalizedInfo("T_Remark", respond);

            SelectDisplayContent(false);

            sb.AppendLine(MakeInfo(spendTimeTitle, spendTimeValue));
            sb.AppendLine(remarkInfo);
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
        }

        private void UpdateContent()
        {
            SelectDisplayContent(true);
            RefreshBaseInfo();
            RefreshReasonRecords();
            RefreshPrePhotos();
            RefreshConsumables();
            RefreshAfPhotos();
            RefreshProcessRecords();
            RefreshIssue();

            foreach (RectTransform breakLine in _breakLines)
            {
                breakLine.gameObject.SetActive(!_device.IsDisabled);
            }
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
                string warrantyValue = _device.WarrantyTime.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                string warrantyInfo = MakeLabelLocalizedInfo("T_WarrantyDate", warrantyValue);
                if (_device.WarrantyTime < DateTime.Now)
                    warrantyInfo += $"<color=red>（{Localizer.Instance.GetLocalizedString("S_OverWarranty")}）</color>";
                sb.AppendLine(warrantyInfo);
            }

            //_deviceInfo.text = sb.ToString();
            sb.Clear();
        }

        public void RefreshReasonRecords()
        {
            bool hasOrderFroms = _device.Froms.Count > 0;

            _respondRecordsTitle.gameObject.SetActive(hasOrderFroms);
            _respondRecordsVirtualList.ClearList();

            if (!hasOrderFroms)
                return;

            foreach (var source in _device.Froms)
            {
                string orderTypeLocalizedKey = source.OrderType switch
                {
                    "R" => "T_Repair",
                    "I" => "T_s_Insp",
                    _ => "--"
                };
                RespondRecordDto display = new RespondRecordDto
                {
                    Title = MakeLabelLocalizedDualInfo(orderTypeLocalizedKey, source.FromAnOrder),
                    Content = source.OrderDescription
                };

                _respondRecordsVirtualList.BindItem(display, _device.IsDisabled);
            }
            _respondRecordsVirtualList.RefreshLayout();
        }

        public void RefreshProcessRecords()
        {
            bool hasOrderRecords = _device.Records.Count > 0;

            _processRecordsTitle.gameObject.SetActive(hasOrderRecords);
            _processRecordsVirtualList.ClearList();

            if (!hasOrderRecords)
                return;

            foreach (var source in _device.Records)
            {
                string staffProcessStatus = $"{source.StaffName} {Localizer.Instance.GetLocalizedString($"S_{source.Status}")}";
                string respondLocalizedKey = $"S_{source.Respond}";
                string localizedRespond = Localizer.Instance.GetLocalizedString(respondLocalizedKey);

                RespondRecordDto display = new RespondRecordDto
                {
                    Title = MakeLabelLocalizedDualInfo(source.RespondTime, staffProcessStatus),
                    Content = (localizedRespond == respondLocalizedKey) ? source.Respond : localizedRespond
                };
                _processRecordsVirtualList.BindItem(display, _device.IsDisabled);
            }
            _processRecordsVirtualList.RefreshLayout();
        }

        public void RefreshConsumables()
        {
            bool ownedConsumable = _device.Consumables.Count > 0;

            _consumablesTitle.gameObject.SetActive(ownedConsumable);
            _saveConsumablesDataButton.gameObject.SetActive(!_device.IsDisabled && ownedConsumable);
            _consumablesVirtualList.BindList(_device.Consumables, _device.IsDisabled);
        }

        public void RefreshPrePhotos()
        {
            bool hasPrePhotos = _prePhotoFiles.Count > 0;
            bool hitPhotoUploadLimit = _prePhotoFiles.Count >= 5;
            bool isDisabled = _device.IsDisabled;

            _addPrePhotoArea.gameObject.SetActive(!isDisabled && !hitPhotoUploadLimit);
            _savePrePhotosButton.gameObject.SetActive(!isDisabled);
            _prePhotosVirtualList.BindList(_prePhotoFiles, isDisabled, (item, data) =>
            {
                item.BindEventAfterRawImageUpdated(RefreshPageLayout);
                item.BindDeletePhotoEvent(() =>
                {
                    _prePhotosVirtualList.ClearItem(item);
                    _prePhotoFiles.Remove(data);
                    RefreshPageLayout();
                });
            });
        }

        public void RefreshAfPhotos()
        {
            bool hasAfPhotos = _afPhotoFiles.Count > 0;
            bool hitPhotoUploadLimit = _afPhotoFiles.Count >= 5;
            bool isDisabled = _device.IsDisabled;

            _addAfPhotoArea.gameObject.SetActive(!isDisabled && !hitPhotoUploadLimit);
            _afPhotosVirtualList.BindList(_afPhotoFiles, isDisabled, (item, data) =>
            {
                item.BindEventAfterRawImageUpdated(RefreshPageLayout);
                item.BindDeletePhotoEvent(() =>
                {
                    _afPhotosVirtualList.ClearItem(item);
                    _afPhotoFiles.Remove(data);
                    RefreshPageLayout();
                });
            });
        }

        public void RefreshPageLayout()
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_pageContentRef);
        }

        // 呼叫原生相機並於完成後加入圖片至設備處理前附圖
        private void AddPrePhoto()
        {
            NativeCameraManager.TakePictureBase64(null, base64Img =>
            {
                if (_afPhotoFiles.Count >= 5)
                {
                    _toastMessageEvent?.Invoke(Localizer.Instance.GetLocalizedString("MSG016"), ToastLevel.Warning);
                }
                else
                {
                    _prePhotoFiles.Add(new ImageFile { base64data = base64Img, contentType = "", fileName = "" });
                    RefreshPrePhotos();
                }
            });
        }

        // 呼叫原生相機並於完成後加入圖片至設備處理後附圖
        private void AddAfPhoto()
        {
            NativeCameraManager.TakePictureBase64(null, base64Img =>
            {
                if (_afPhotoFiles.Count >= 5)
                {
                    _toastMessageEvent?.Invoke(Localizer.Instance.GetLocalizedString("MSG016"), ToastLevel.Warning);
                }
                else
                {
                    _afPhotoFiles.Add(new ImageFile { base64data = base64Img, contentType = "", fileName = "" });
                    RefreshAfPhotos();
                }
            });
        }

        public void RefreshIssue()
        {
            bool isDisabled = _device.IsDisabled;
            bool isProcessing = _device.StartTime != null && _device.PauseTime == null;
            StringBuilder sb = new StringBuilder();

            if (_device.SubmitTime != null)
                sb.AppendLine(MakeLabelLocalizedInfo("T_UploadTime", _device.SubmitTime));
            if (_device.PauseTime != null)
                sb.AppendLine(MakeLabelLocalizedInfo("T_PauseTime", _device.PauseTime));
            _lastUpdatedTime.text = sb.ToString();
            sb.Clear();

            _respondTitle.gameObject.SetActive(!isDisabled);
            _respondInputField.gameObject.SetActive(!isDisabled);
            _respondInputField.onEndEdit.RemoveAllListeners();
            if (!isDisabled)
            {
                _respondInputField.onEndEdit.AddListener((newRespond) =>
                {
                    _device.Respond = newRespond;
                    //Debug.Log("[InspDeviceListItem] End Edit: " + newRespond);
                });
            }
            _respondInputField.text = _device.Respond ?? string.Empty;

            _collapseDeviceButton.gameObject.SetActive(isDisabled);
            _processingButtonArea.gameObject.SetActive(!isDisabled && isProcessing);
            _startDeviceWorkButton.gameObject.SetActive(!isDisabled && !isProcessing);
        }
    }
}
