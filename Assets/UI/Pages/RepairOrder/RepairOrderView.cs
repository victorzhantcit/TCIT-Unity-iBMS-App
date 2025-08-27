using iBMSApp.App;
using iBMSApp.Services;
using iBMSApp.Shared;
using iBMSApp.UI.Common;
using iBMSApp.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace iBMSApp.UI.Pages
{
    /// <summary>
    /// 尚未處理完畢 (Copy form <see cref="WorkOrderView"/>)
    /// </summary>
    public class RepairOrderView : NetworkPageMonoBehavior<RepairOrderView>
    {
        private static readonly int ValueIndent = 130;

        private static readonly LocalizedStyle HeaderStyle =
            LocalizedStyle.LabelLocalized | LocalizedStyle.Indent;

        private static readonly LocalizedStyle ContentStyle =
            LocalizedStyle.LabelLocalized | LocalizedStyle.Comma | LocalizedStyle.Indent;

        private static readonly LocalizedStyle ContentBothStyle =
            LocalizedStyle.LabelLocalized | LocalizedStyle.ValueLocalized | LocalizedStyle.Comma | LocalizedStyle.Indent;

        private static readonly LocalizedStyleParams DefaultStyleParams = new LocalizedStyleParams
        {
            Indent = ValueIndent
        };

        // Services(DI)
        private EqptService _eqptService;
        private ILocalStorageService _localStorage;

        [Header("UI")]
        [SerializeField] private ScrollRect _scrollView;
        [SerializeField] private Image _orderStatusBG;
        [SerializeField] private TMP_Text _recordSnText;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _recordInfo;
        [SerializeField] private VisualColorSwapper _photoAreaBackground;
        [SerializeField] private PhotoPanelsVirtualList _photosVirtualList;
        [SerializeField] private RectTransform _addPhotoChunk;
        [SerializeField] private TMP_Text _recordSummaryText;
        [SerializeField] private TMP_InputField _completionReportInputField;
        [SerializeField] private Button _submitButton;
        [SerializeField] private TMP_Text _submitButtonText;

        private List<ImageFile> _photoFiles = new List<ImageFile>();

        private EqptRepairOrder _order = new EqptRepairOrder();
        private string _repairNo = string.Empty;
        private string _rBuildingCode = string.Empty;
        private string _newIssue = string.Empty;
        private float _scrollValue = 1f;
        private bool _updateLocker = true;
        private bool _pageReadOnly = true;

        private static string MakeLocalizedContentRichText(string localizedLabelKey, string value)
        {
            return Localizer.GenerateLocalizedRichText(
                localizedLabelKey,
                value,
                ContentStyle,
                DefaultStyleParams
            );
        }

        private static string MakeLocalizedBothContentRichText(string localizedLabelKey, string value)
        {
            return Localizer.GenerateLocalizedRichText(
                localizedLabelKey,
                value,
                ContentBothStyle,
                DefaultStyleParams
            );
        }

        private static string MakeLocalizedHeaderRichText(string localizedLabelKey, string value)
        {
            return Localizer.GenerateLocalizedRichText(
                localizedLabelKey,
                value,
                HeaderStyle,
                DefaultStyleParams
            );
        }

        #region Override BaseClass
        private new void Start()
        {
            base.Start();

            if (!LoadServices())
                return;

            InitUIOnStart();
            _updateLocker = false;
        }

        private new void Update()
        {
            if (_updateLocker)
                return;

            base.Update();
        }

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
        public override async Task RefreshPage(PageRefreshParams refreshParams = null)
        {
            await Task.Yield();

            if (refreshParams != null)
            {
                _rBuildingCode = refreshParams.BuildingCode;
                _repairNo = refreshParams.RecordSn;
                _pageReadOnly = refreshParams.PageReadOnly;
            }

            DeviceStaticInfo device = await _eqptService.GetDevice();

            _order = new EqptRepairOrder();
            _photoFiles.Clear();
            _newIssue = "";

            _order.BuildingCode = device.BuildingCode;
            _order.DeviceCode = device.DeviceCode;
            _order.DeviceType = device.Type;
            _order.DeviceDescription = device.Description;
            _order.Code = device.Code;
            _order.BuildingName = device.BuildingName;

            if (!string.IsNullOrEmpty(_repairNo))
            {
                await GetRepairOrder();
            }
            else
            {
                UserInfo user = await base.GetUserInfoAsync();
                _order.Department = user.Department;
                _order.Tel = user.Tel;
                _order.Email = user.Email;
                _order.Status = "";
                _order.statusBGColor = "bgc-processing"; //黃色
                await UpdateOrder();
            }
        }
        #endregion

        private void InitUIOnStart()
        {
            Hide();
        }

        private bool LoadServices()
        {
            if (ServiceManager.Instance == null)
                return false;

            _eqptService = ServiceManager.Instance.EqptService;
            _localStorage = ServiceManager.Instance.LocalStorageService;

            return true;
        }

        public void OnSubmitRepairClicked() => _ = RepairSubmit();

        public void OnAddPhotoClicked() => AddPhoto();

        /// <summary>
        /// 上傳格式詳見 <seealso cref="EqptRequestOrder"/>ee
        /// </summary>
        /// <returns></returns>
        private async Task GetRepairOrder()
        {
            base.LoadingPage(true);

            var queryPara = new Dictionary<string, string>();
            queryPara.Add(nameof(EqptRequestOrder.BuildingCode), _rBuildingCode);
            queryPara.Add(nameof(EqptRequestOrder.RecordSn), _repairNo);

            var response = await _eqptService.GetRepairOrder(queryPara);

            if (!response.IsSuccess)
            {
                base.HandleHttpRequestException(response);
                base.LoadingPage(false);
                return;
            }

            var order = response.Data;

            if (order == null)
            {
                _toastService.ShowToast(response.ErrorMessage ?? "", ToastLevel.Warning);
                base.LoadingPage(false);
                return;
            }

            if (order.Status.Equals("Pending") ||
                order.Status.Equals("Processing") ||
                order.Status.Equals("Scheduled"))
            {
                _order.Status = "S_Pending";//"待處理"
                _order.statusBGColor = "bgc-pending";//粉色
            }
            else
            {
                _order.Status = "S_Processed";//"已處理"
                _order.statusBGColor = "bgc-processed";//藍色
            }

            _order.BuildingName = order.BuildingName ?? "";
            _order.RecordSn = order.RecordSn ?? "";
            _order.Issue = order.Issue;
            _newIssue = _order.Issue ?? "";
            _order.CreateTime = order.CreateTime;
            _order.ScheduledDate = order.ScheduledDate;
            _order.ProcessTime = order.ProcessTime;
            _order.CompleteTime = order.CompleteTime;
            _order.Issuer = order.Issuer ?? "";
            _order.IssuerName = order.IssuerName ?? "";
            _order.Staff = order.StaffName ?? "";
            _order.PhotoSns = order.PhotoSns ?? "";
            _order.Code = order.Code ?? "";
            _order.DeviceCode = order.DeviceCode ?? "";
            _order.DeviceType = order.DeviceType ?? "";
            _order.DeviceDescription = order.DeviceDescription;

            _order.WorkSn = order.WorkSn;
            _order.Reply = order.Reply;
            _order.Department = order.Department ?? "";
            _order.Tel = order.Tel ?? "";
            _order.Email = order.Email ?? "";

            _photoFiles.Clear();
            if (!(_order.PhotoSns ?? "").Equals(""))
            {
                var PhotoSnArray = (_order.PhotoSns ?? "")//.Trim('[', ']').Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "")
                    .Split(",")
                    .Select(x => x.Trim('"'))
                    .ToArray();
                foreach (var sn in PhotoSnArray)
                {
                    await GetPhoto(sn);
                }
            }

            _scrollValue = _scrollView.verticalNormalizedPosition;
            await UpdateOrder();
            base.LoadingPage(false);
        }

        private async Task GetPhoto(string sn)
        {
            var response = await _eqptService.GetRequestPhoto(sn);

            if (!response.IsSuccess)
            {
                base.HandleHttpRequestException(response);
                return;
            }
            var responseData = response.Data;

            _photoFiles.Add(new ImageFile { base64data = responseData?.Photo ?? "", contentType = "", fileName = "" });
        }

        // 以 _order 進行更新
        private async Task UpdateOrder()
        {
            UserInfo userInfo = await base.GetUserInfoAsync();
            StringBuilder sb = new StringBuilder();
            string repairNo = string.IsNullOrEmpty(_repairNo) ? "" : _repairNo;
            string description = _pageReadOnly ? _order.Issue : "\n\n."; // 目前使用 descriptionText 作為 layout 高度來調整文字輸入框的高度，暫時這樣解決 UI layout
            bool hitPhotoUploadLimit = _photoFiles.Count >= 5;
            bool hasRepairTime = !string.IsNullOrEmpty(_order.CreateTime?.ToString()) 
                && !_order.CreateTime.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture).Equals("0001-01-01");

            _scrollValue = _scrollView.verticalNormalizedPosition;

            // 頁面標頭
            _recordSnText.text = MakeLocalizedHeaderRichText("T_RepairOrder", repairNo);
            _orderStatusBG.color = ColorMapper.Instance.GetColor(_order.statusBGColor);
            _statusText.gameObject.SetActive(!string.IsNullOrEmpty(_order.Status));
            _statusText.text = Localizer.Instance.GetLocalizedString(_order.Status);
            _descriptionText.text = MakeLocalizedHeaderRichText("T_Issue", description);

            _completionReportInputField.text = "";
            _completionReportInputField.gameObject.SetActive(!_pageReadOnly);
            _completionReportInputField.onEndEdit.RemoveAllListeners();
            if (!_pageReadOnly)
            {
                _completionReportInputField.text = _newIssue;
                _completionReportInputField.onEndEdit.AddListener((newRespond) =>
                {
                    _newIssue = newRespond;
                    //DebugLog($"Respond End Edit: " + newRespond);
                });
            }

            sb.AppendLine(MakeLocalizedContentRichText("T_Building", _order.BuildingName));
            sb.AppendLine(MakeLocalizedContentRichText("報修人", _order.CreateTime != null ? _order.IssuerName : userInfo.Name));
            if (!string.IsNullOrEmpty(_order.Department))
            {
                sb.AppendLine(MakeLocalizedContentRichText("T_Department", _order.Department));
            }
            if (!string.IsNullOrEmpty(_order.Tel))
            {
                sb.AppendLine(MakeLocalizedContentRichText("T_Tel", _order.Tel));
            }
            if (hasRepairTime)
            {
                string repairTime = hasRepairTime ? _order.CreateTime.Value.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture) : "";
                sb.AppendLine(MakeLocalizedContentRichText("T_RepairTime", repairTime));
            }
            if (_order.DeviceType.Equals("Other"))
            {
                sb.AppendLine(MakeLocalizedContentRichText("T_Location", _order.DeviceDescription));
            }
            else
            {
                string deviceDescription = _order.DeviceDescription;
                if (!_order.Code.Equals("")) deviceDescription += $" ({_order.Code})";
                sb.AppendLine(MakeLocalizedContentRichText("T_Device", deviceDescription));
            }
            _recordInfo.text = sb.ToString();
            sb.Clear();

            RefreshPhotos();

            _addPhotoChunk.gameObject.SetActive(!_pageReadOnly && !hitPhotoUploadLimit);

            _submitButton.gameObject.SetActive(!_pageReadOnly);

            if (!string.IsNullOrEmpty(_order.CompleteTime.ToString()))
            {
                string completedTime = _order.CompleteTime.Value.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                sb.AppendLine(MakeLocalizedContentRichText("T_CompleteTime", completedTime));
            }
            else if (!string.IsNullOrEmpty(_order.ReplyTime.ToString()))
            {
                string replyTime = _order.ReplyTime.Value.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                sb.AppendLine(MakeLocalizedContentRichText("T_RespondTime", replyTime));
            }
            else if (!string.IsNullOrEmpty(_order.ProcessTime.ToString()))
            {
                string processingTime = _order.ProcessTime.Value.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                sb.AppendLine(MakeLocalizedContentRichText("T_ProcessingTime", processingTime));
            }

            if (!string.IsNullOrEmpty(_order.Staff))
                sb.AppendLine(MakeLocalizedContentRichText("T_Processor", _order.Staff));
            if (!string.IsNullOrEmpty(_order.Reply))
                sb.AppendLine(MakeLocalizedBothContentRichText("T_ProcessDescription", _order.Reply));
            _recordSummaryText.text = sb.ToString();
            sb.Clear();

            _scrollView.verticalNormalizedPosition = _scrollValue;
        }

        public void RefreshPhotos()
        {
            ToggleState photoEditable = _pageReadOnly ? ToggleState.Deactivated : ToggleState.Activated;

            _photoAreaBackground.SetConfigStatus(photoEditable);
            _photosVirtualList.BindList(_photoFiles, _pageReadOnly, (item, data) =>
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
            if (_completionReportInputField.isActiveAndEnabled)
            {
                _completionReportInputField.text = _newIssue;
            }
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollView.GetComponentInParent<RectTransform>());
        }

        // 呼叫原生相機並於完成後加入圖片至設備處理後附圖
        private void AddPhoto()
        {
            NativeCameraManager.TakePictureBase64(null, base64Img =>
            {
                if (_photoFiles.Count >= 5)
                {
                    _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG016"), ToastLevel.Warning);
                }
                else
                {
                    _photoFiles.Add(new ImageFile { base64data = base64Img, contentType = "", fileName = "" });
                    RefreshPhotos();
                }
            });
        }

        /// <summary>
        /// 上傳格式詳見 <seealso cref="EqptRequestSetRepairOrder"/>eeal
        /// </summary>
        /// <returns></returns>
        private async Task RepairSubmit()
        {
            if (!_isOnline)
            {
                _toastService.ShowToast(NetworkUnreachableText, ToastLevel.Warning);
                return;
            }

            if (_newIssue.Equals(""))
            {
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG038"), ToastLevel.Warning);//"請輸入異常描述!"
                return;
            }
            if (_photoFiles.Count == 0)
            {
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG025"), ToastLevel.Warning);//"請拍照異常情形!"
                return;
            }

            string Photos = "[" + string.Join(",", _photoFiles.Select(f => $"'{f.base64data}'")) + "]";
            if (!string.IsNullOrEmpty(_repairNo))
            {
                _order.RecordSn = _repairNo;
                _order.BuildingCode = _rBuildingCode;
            }
            

            var query = new Dictionary<string, string>();

            query.Add(nameof(EqptRequestSetRepairOrder.BuildingCode), _order.BuildingCode);
            query.Add(nameof(EqptRequestSetRepairOrder.RepairNo), _order.RecordSn);
            query.Add(nameof(EqptRequestSetRepairOrder.DeviceType), _order.DeviceType);
            query.Add(nameof(EqptRequestSetRepairOrder.DeviceCode), _order.DeviceCode);
            query.Add(nameof(EqptRequestSetRepairOrder.Code), _order.Code);
            query.Add(nameof(EqptRequestSetRepairOrder.DeviceDescription), _order.DeviceDescription);
            query.Add(nameof(EqptRequestSetRepairOrder.Issue), _newIssue);
            query.Add(nameof(EqptRequestSetRepairOrder.Photo), Photos);

            Console.WriteLine("POST " + query);
            var response = await _eqptService.PostSetRepairOrder(query);
            if (!response.IsSuccess)
            {
                base.HandleHttpRequestException(response);
                return;
            }

            var order = response.Data;

            if (order == null || string.IsNullOrEmpty(order.RecordSn))
            {
                _toastService.ShowToast(response.ErrorMessage ?? "", ToastLevel.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(_repairNo))
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG039"), ToastLevel.Success);//新增成功！
            else
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG040"), ToastLevel.Success);//修改成功！

            base.GoToPagePrevious();
        }
    }
}
