using iBMSApp.DataModels;
using iBMSApp.Services;
using iBMSApp.Shared;
using iBMSApp.UI.Common;
using iBMSApp.UI.Components;
using iBMSApp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace iBMSApp.UI.Pages
{
    public class InspOrderView : NetworkPageMonoBehavior<InspOrderView>
    {
        private static readonly int ValueIndent = 130;

        private static readonly LocalizedStyle HeaderStyle =
            LocalizedStyle.LabelLocalized | LocalizedStyle.Indent;

        private static readonly LocalizedStyle ContentStyle =
            LocalizedStyle.LabelLocalized | LocalizedStyle.Comma | LocalizedStyle.Indent;

        private static readonly LocalizedStyleParams DefaultStyleParams = new LocalizedStyleParams
        {
            Indent = ValueIndent
        };

        private static string MakeLocalizedContentRichText(string localizedLabelKey, string value)
        {
            return Localizer.GenerateLocalizedRichText(
                localizedLabelKey,
                value,
                ContentStyle,
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
        [SerializeField] private RectTransform _deviceListContent;
        [SerializeField] private InspDeviceListItem _deviceListItemPrefab;
        [SerializeField] private TMP_Text _recordSummaryText;
        [SerializeField] private TMP_InputField _completionReportInputField;
        [SerializeField] private Button _submitButton;
        [SerializeField] private TMP_Text _submitButtonText;

        private bool _updateLocker = true;
        private string _recordSn = string.Empty;
        private string _rBuildingCode = string.Empty;
        private string _deviceCode = string.Empty;
        private int orderDevIndex = -1;
        private ObjectPool<InspDeviceListItem> _deviceListItemPool;
        private EqptOrder _order;
        private bool _allDeviceSubmitted = false;
        private List<ImageFile> _filesBase64 = new List<ImageFile>();
        private float _scrollValue = 1f;
        private string _newRespond = string.Empty;


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
            if (refreshParams != null)
            {
                _rBuildingCode = refreshParams.BuildingCode;
                _recordSn = refreshParams.RecordSn;
            }
            _deviceCode = "";

            await GetInspOrder();
        }
        #endregion

        private void InitUIOnStart()
        {
            _deviceListItemPool = new ObjectPool<InspDeviceListItem>(_deviceListItemPrefab, _deviceListContent);
            Hide();
        }

        private bool LoadServices()
        {
            if (ServiceManager.Instance == null)
                return false;

            _eqptService = ServiceManager.Instance.EqptService;
            _localStorage = ServiceManager.Instance.LocalStorageService;
            _toastService = ServiceManager.Instance.ToastService;

            return true;
        }

        public void OnSubmitInspClicked() => _ = InspSubmit();

        private async Task GetInspOrder()
        {
            if (_deviceCode.Equals("0")) _deviceCode = "";
            _order = await _eqptService.GetInspOrder(_rBuildingCode, _recordSn);
            bool allDeviveSubmitted = true;
            foreach (var dev in _order.OrderDevices)
            {
                if (dev.Code != null)
                {
                    var deviceData = await _eqptService.GetDeviceData(x => x.Code == dev.Code);
                    var DeviceName = deviceData?.Description;
                    if (DeviceName != null)
                        dev.DeviceDescription = DeviceName;
                    else
                    {
                        DebugLog("DeviceName is null!!");
                    }
                    if (dev.Respond.Equals("Start")) dev.Respond = "";
                    if (!dev.Status.Equals("Submitted")) allDeviveSubmitted = false; 
                }
            }
            _allDeviceSubmitted = allDeviveSubmitted;
            if (!_deviceCode.Equals(""))
            {
                orderDevIndex = _order.OrderDevices.FindIndex(dev => dev.BuildingCode == _rBuildingCode && dev.DeviceCode == _deviceCode);
                DebugLog("orderDevIndex:" + orderDevIndex);
                _filesBase64.Clear();
                if (!(_order.OrderDevices[orderDevIndex].AfPhotoSns ?? "").Equals(""))
                {
                    var PhotoSnArray = (_order.OrderDevices[orderDevIndex].AfPhotoSns ?? "")//.Trim('[', ']').Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "")
                            .Split(",")
                            .Select(x => x.Trim('"'))
                            .ToArray();
                    foreach (var sn in PhotoSnArray)
                    {
                        await GetPhoto(sn);
                    }
                }
            }

            await UpdateOrder();
        }

        // 以 _order 進行更新
        private async Task UpdateOrder()
        {
            bool isRejected = _order.RejectTime != null;
            bool isCompleted = _order.CompleteTime != null;
            bool isSubmitted = _order.SubmitTime != null; 
            bool isUndone = isRejected || (!isCompleted && !isSubmitted);
            string orderTypeKey = _order.OrderType == "Plan" ? "C_AnnualPlan" : "C_Single";
            string orderTypeValue = Localizer.Instance.GetLocalizedString(orderTypeKey);
            StringBuilder sb = new StringBuilder();

            // 頁面標頭
            _recordSnText.text = MakeLocalizedHeaderRichText("T_InspOrderNo", _order.RecordSn);
            _orderStatusBG.color = ColorMapper.Instance.GetColor(_order.StatusBGColor);
            _statusText.text = Localizer.Instance.GetLocalizedString($"S_{_order.Status}");
            _descriptionText.text = MakeLocalizedHeaderRichText("T_InspItems", _order.Description);

            // 基本資訊
            sb.AppendLine(MakeLocalizedContentRichText("T_Type", orderTypeValue));
            sb.AppendLine(MakeLocalizedContentRichText("T_Building", _order.BuildingName));
            sb.AppendLine(MakeLocalizedContentRichText("T_ScheduledDate", _order.ScheduledDate));
            if (!string.IsNullOrEmpty(_order.StartTime))
                sb.AppendLine(MakeLocalizedContentRichText("T_StartingTime", _order.StartTime));
            _recordInfo.text = sb.ToString();
            sb.Clear();

            // 設備列表
            UpdateDeviceList();

            // 退件資訊
            _recordSummaryText.text = "";
            if (isRejected)
            {
                string userName = await base.GetUidAsync();

                sb.AppendLine(MakeLocalizedContentRichText("T_RejectTime", _order.RejectTime));
                if (!_order.StaffName.Equals(userName))
                    sb.AppendLine(MakeLocalizedContentRichText("T_Respondent", _order.StaffName));
                sb.AppendLine(MakeLocalizedContentRichText("T_Reject", _order.Comment));
            }

            // 總結(或最新資訊)
            _completionReportInputField.gameObject.SetActive(false);
            _submitButton.gameObject.SetActive(false);
            if (_allDeviceSubmitted)
            {
                if (isUndone)
                {
                    sb.AppendLine(MakeLocalizedContentRichText("T_CompletionReport", string.Empty));
                    _completionReportInputField.gameObject.SetActive(true);
                }

                if (isCompleted)
                {
                    sb.AppendLine(MakeLocalizedContentRichText("T_CompleteTime", _order.CompleteTime));
                }
                else if (isRejected)
                {
                    _submitButtonText.text = Localizer.Instance.GetLocalizedString("BTN013");
                    _submitButton.gameObject.SetActive(true);
                }
                else if (isSubmitted)
                {
                    sb.AppendLine(MakeLocalizedContentRichText("T_UploadTime", _order.SubmitTime));
                }
                else
                {
                    _submitButtonText.text = Localizer.Instance.GetLocalizedString("BTN012");
                    _submitButton.gameObject.SetActive(true);
                }
            }

            _recordSummaryText.text = sb.ToString();
            sb.Clear();

            _scrollView.verticalNormalizedPosition = _scrollValue;
        }

        private void ClearOrderList()
        {
            foreach (Transform item in _deviceListContent)
            {
                if (item.gameObject.activeSelf == false)
                {
                    return;
                }
                if (item.TryGetComponent<InspDeviceListItem>(out var itemComponent))
                {
                    _deviceListItemPool.Release(itemComponent);
                }
            }
            Canvas.ForceUpdateCanvases();
        }

        private void UpdateDeviceList()
        {
            ClearOrderList();
            int siblingIndex = 0;
            foreach (EqptOrderDevice device in _order.OrderDevices)
            {
                InspDeviceListItem inspDeviceItem = _deviceListItemPool.Get();

                inspDeviceItem.SetPageContentRef(_scrollView.GetComponentInParent<RectTransform>());
                inspDeviceItem.BindData(device, _filesBase64, _order.RejectTime != null);
                inspDeviceItem.transform.SetSiblingIndex(siblingIndex);
                siblingIndex++;

                // 更新圖示與事件綁定
                if (device.DeviceCode == _deviceCode)
                {
                    inspDeviceItem.SetFunctionalStatus(EnumDeviceListItemStatus.Collapse, () =>
                    {
                        CloseDeviceCode(device.DeviceCode);
                    });
                }
                else if (device.StartTime == null || device.PauseTime != null)
                {
                    inspDeviceItem.SetFunctionalStatus(EnumDeviceListItemStatus.QRScan, () =>
                    {
                        ScanCode(device.DeviceCode, 0);
                    });
                }
                else
                {
                    inspDeviceItem.SetFunctionalStatus(EnumDeviceListItemStatus.Expand, () => 
                    {
                        ScanCode(device.DeviceCode, 1); 
                    });
                }
                inspDeviceItem.BindEventSaveNumericalData(() => _ = SaveNumericalData());
                inspDeviceItem.BindEventSaveConsumableData(() => _ = SaveConsumables(inspDeviceItem));
                inspDeviceItem.BindEventToastMessage(_toastService.ShowToast);
                inspDeviceItem.BindEventCollapseDevice(() => CloseDeviceCode(device.DeviceCode));
                inspDeviceItem.BindEventUpdateDevice(() => _ = DeviceInspUpdate());
                inspDeviceItem.BindEventSubmitDevice(() => _ = DeviceInspSubmit());
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(_deviceListContent);
        }

        private async Task GetPhoto(string sn)
        {
            PhotoStorageDto photo = await _eqptService.GetInspPhoto(sn);
            if (photo == null) return;

            _filesBase64.Add(new ImageFile { base64data = photo.Img, contentType = "", fileName = "" });
        }

        private async void ScanCode(string deviceCode, int type)
        {
            if (type == 0)
            {
                base.GoToPageChild(new PageRefreshParams {
                    BuildingCode = _rBuildingCode,
                    RecordSn = _recordSn,
                    DeviceCode = deviceCode
                });
            }
            else
            {
                _deviceCode = deviceCode;
                _scrollValue = _scrollView.verticalNormalizedPosition; // 展開項目時保持滑動固定
                await GetInspOrder();
            }
        }

        private void CloseDeviceCode(string dev)
        {
            _scrollValue = _scrollView.verticalNormalizedPosition; // 收起項目時保持滑動固定
            if (_deviceCode.Equals(dev))
            {
                _deviceCode = "";
                UpdateDeviceList();
            }
        }

        /// <summary>
        /// 上傳格式詳見<seealso cref="EqptRequestSaveNumericalData"/>
        /// </summary>
        /// <returns></returns>
        private async Task SaveNumericalData()
        {
            var queryPara = new Dictionary<string, string>() { };
            queryPara.Add(nameof(EqptRequestSaveNumericalData.BuildingCode), _rBuildingCode);
            queryPara.Add(nameof(EqptRequestSaveNumericalData.DeviceCode), _deviceCode);
            queryPara.Add(nameof(EqptRequestSaveNumericalData.RecordSn), _recordSn);

            var NowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string NumericalData = "";
            foreach (var ind in _order.OrderDevices[orderDevIndex].NumericalData)
            {
                if (!NumericalData.Equals("")) NumericalData = NumericalData + ",";
                NumericalData = NumericalData + "{\"TagName\":\"" + ind.TagName + "\",\"TagUnit\":\"" + ind.TagUnit
                + "\",\"TagDescription\":\"" + ind.TagDescription + "\",\"Value\":\"" + ind.Value + "\"}";
            }
            NumericalData = "[" + NumericalData + "]";
            queryPara.Add(nameof(EqptRequestSaveNumericalData.NumericalData), NumericalData);

            if (!_isOnline)
            {
                queryPara.Add("NowTime", NowTime);
                queryPara.Add("Method", _eqptService.TaskMethod_SaveNumericalData);
                await _eqptService.SetTask(queryPara);

                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("S_Saved"), ToastLevel.Success);//"已儲存"
            }
            else
            {
                var response = await _eqptService.PostSaveNumericalData(queryPara);

                if (!response.IsSuccess)
                {
                    base.HandleHttpRequestException(response);
                    return;
                }

                string result = response.Data;

                if (string.IsNullOrEmpty(result))
                {
                    _toastService.ShowToast(response.ErrorMessage ?? "", ToastLevel.Warning);
                    return;
                }

                if (!result.Equals("Done"))
                {
                    _toastService.ShowToast(result, ToastLevel.Warning);
                    return;
                }

                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("S_Saved"), ToastLevel.Success);
                _order.OrderDevices[orderDevIndex].DataTime = NowTime.Substring(0, 16);
            }
            await _eqptService.SetInspOrder(_order);
        }

        /// <summary>
        /// 上傳格式詳見<seealso cref="EqptRequestSaveOrderConsumables"/>
        /// </summary>
        /// <returns></returns>
        private async Task SaveConsumables(InspDeviceListItem refreshUIReference)
        {
            var queryPara = new Dictionary<string, string>() { };
            queryPara.Add(nameof(EqptRequestSaveOrderConsumables.BuildingCode), _rBuildingCode);
            queryPara.Add(nameof(EqptRequestSaveOrderConsumables.DeviceCode), _deviceCode);
            queryPara.Add(nameof(EqptRequestSaveOrderConsumables.RecordSn), _recordSn);

            var NowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string Consumables = "";
            foreach (var cons in _order.OrderDevices[orderDevIndex].Consumables)
            {
                if (cons.isChecked == true)
                    cons.ReplaceDate = NowTime;
                if (!Consumables.Equals("")) Consumables = Consumables + ",";
                Consumables = Consumables + "{\"Name\":\"" + cons.Name + "\",\"AvailableNum\":\"" + cons.AvailableNum
                + "\",\"AvailableUnit\":\"" + cons.AvailableUnit + "\",\"ReplaceDate\":\"" + cons.ReplaceDate + "\"}";
            }
            Consumables = "[" + Consumables + "]";
            queryPara.Add(nameof(EqptRequestSaveOrderConsumables.Consumables), Consumables);
            if (!_isOnline)
            {
                queryPara.Add("NowTime", NowTime);
                queryPara.Add("Method", _eqptService.TaskMethod_SaveInspConsumables);
                await _eqptService.SetTask(queryPara);

                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("S_Saved"), ToastLevel.Success);//"已儲存"
            }
            else
            {
                var response = await _eqptService.PostSaveInspConsumables(queryPara);

                if (!response.IsSuccess)
                {
                    base.HandleHttpRequestException(response);
                    return;
                }

                string result = response.Data;

                if (string.IsNullOrEmpty(result))
                {
                    _toastService.ShowToast(response.ErrorMessage ?? "", ToastLevel.Warning);
                    return;
                }

                if (!result.Equals("Done"))
                {
                    _toastService.ShowToast(result, ToastLevel.Warning);
                    return;
                }

                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("S_Saved"), ToastLevel.Success);//"已儲存"
            }

            await _eqptService.SetInspOrder(_order);
            refreshUIReference.RefreshConsumables();
        }

        /// <summary>
        /// 上傳格式詳見<seealso cref="EqptRequestUpdateOrderDevice"/>
        /// </summary>
        private async Task DeviceInspUpdate() //原本是完工上傳的狀態，變更成再做一次，給新的開始時間
        {
            var queryPara = new Dictionary<string, string>() { };
            queryPara.Add(nameof(EqptRequestUpdateOrderDevice.BuildingCode), _rBuildingCode);
            queryPara.Add(nameof(EqptRequestUpdateOrderDevice.DeviceCode), _deviceCode);
            queryPara.Add(nameof(EqptRequestUpdateOrderDevice.RecordSn), _recordSn);

            var NowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (!_isOnline)
            {
                queryPara.Add("NowTime", NowTime);
                queryPara.Add("Method", _eqptService.TaskMethod_UpdateInspDevice);
                await _eqptService.SetTask(queryPara);

                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("S_Saved"), ToastLevel.Success);//"已儲存"
            }
            else
            {
                var response = await _eqptService.PostUpdateInspDevice(queryPara);

                if (!response.IsSuccess)
                {
                    base.HandleHttpRequestException(response);
                    return;
                }
                
                EqptRespondSubmitDevice responseData = response.Data;
                
                if (responseData == null)
                {
                    _toastService.ShowToast((response.ErrorMessage ?? "").ToString(), ToastLevel.Warning);
                    return;
                }

                if (responseData?.Result != "Done")
                {
                    _toastService.ShowToast((responseData?.Result ?? "").ToString(), ToastLevel.Warning);
                    return;
                }

                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG040"), ToastLevel.Success);
            }
            _order.SubmitTime = null;
            _order.Status = "Processing";
            _order.StatusColor = "c-processing";
            _order.StatusBGColor = "bgc-processing";
            _allDeviceSubmitted = false;
            _order.OrderDevices[orderDevIndex].StartTime = NowTime.Substring(0, 16);
            _order.OrderDevices[orderDevIndex].PauseTime = null;
            _order.OrderDevices[orderDevIndex].SubmitTime = null;
            _order.OrderDevices[orderDevIndex].Status = "Processing";
            _order.OrderDevices[orderDevIndex].IsDisabled = false;
            EqptOrderRecord record = new EqptOrderRecord();
            record.RespondTime = NowTime.Substring(0, 16);
            record.StaffName = await base.GetUidAsync();
            record.Status = _order.OrderDevices[orderDevIndex].Status;
            record.Respond = "Start";
            _order.OrderDevices[orderDevIndex].Records.Add(record);
            await _eqptService.SetInspOrder(_order);

            await GetInspOrder();
        }

        private async Task DeviceInspSubmit()//PutInspOrder
        {
            string newRespond = _order.OrderDevices[orderDevIndex].Respond;
            if (newRespond.Equals(""))
            {
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG024"), ToastLevel.Warning);//"請輸入設備巡檢總結!"
                return;
            }
            if (_filesBase64.Count == 0)
            {
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG020"), ToastLevel.Warning);//"請附加拍照！"
                return;
            }

            string Photos = "[" + string.Join(",", _filesBase64.Select(f => f.base64data)) + "]";

            var queryPara = new Dictionary<string, string>() { }; // EqptRequestSubmitInspDevice
            queryPara.Add(nameof(EqptRequestSubmitInspDevice.BuildingCode), _rBuildingCode);
            queryPara.Add(nameof(EqptRequestSubmitInspDevice.RecordSn), _recordSn);
            queryPara.Add(nameof(EqptRequestSubmitInspDevice.DeviceCode), _deviceCode);
            queryPara.Add(nameof(EqptRequestSubmitInspDevice.DeviceCount), _order.OrderDevices.Count().ToString());

            string Items = "", itemStatus = "";
            _order.OrderDevices[orderDevIndex].Summarize = 0;
            int idx = 0;
            foreach (var ind in _order.OrderDevices[orderDevIndex].Items)
            {
                idx++;
                if (!Items.Equals("")) Items = Items + ",";
                itemStatus = "";
                foreach (KeyValuePair<string, string> kvp in ind.Status)
                {
                    if (!itemStatus.Equals("")) itemStatus = itemStatus + ",";
                    itemStatus = itemStatus + "\"" + kvp.Key + "\":\"" + kvp.Value + "\"";
                }
                itemStatus = "{" + itemStatus + "}";
                Items = Items + "{\"ItemName\":\"" + ind.ItemName + "\",\"Status\":" + itemStatus
                + ",\"Method\":\"" + ind.Method + "\",\"Running\":\"" + ind.Running + "\",\"Reference\":\"" + ind.Reference
                + "\",\"Selected\":\"" + ind.Selected + "\",\"Note\":\"" + ind.Note + "\"}";

                if (!ind.Selected.Equals("0")) _order.OrderDevices[orderDevIndex].Summarize = 1;
                if (ind.Selected.Equals(""))
                {
                    string msg = Localizer.Instance.GetLocalizedString("MSG026");
                    _toastService.ShowToast(msg.Substring(0, msg.IndexOf(" N ")) + " " + idx + " " + msg.Substring(msg.IndexOf(" N ") + 2), ToastLevel.Warning);
                    return;
                }
            }
            queryPara.Add(nameof(EqptRequestSubmitInspDevice.FormSns), string.Join(", ", _order.OrderDevices[orderDevIndex].FormSns));
            queryPara.Add(nameof(EqptRequestSubmitInspDevice.Summarize), _order.OrderDevices[orderDevIndex].Summarize.ToString());
            queryPara.Add(nameof(EqptRequestSubmitInspDevice.Result), newRespond);
            Items = "[" + Items + "]";
            queryPara.Add(nameof(EqptRequestSubmitInspDevice.Items), Items);
            queryPara.Add(nameof(EqptRequestSubmitInspDevice.Photos), Photos);
            var NowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var PhotoSns = "";
            if (!_isOnline)
            {
                queryPara.Add("NowTime", NowTime);
                queryPara.Add("Method", _eqptService.TaskMethod_SubmitInspDevice);
                Random rnd = new Random();  //產生亂數初始值
                for (int i = 0; i < _filesBase64.Count; i++)
                {
                    var sn = "T" + rnd.Next(1, 1000);   //亂數產生，亂數產生的範圍是1~1000
                    PhotoSns = PhotoSns + sn + ",";
                    await _eqptService.SetInspPhoto(sn, _filesBase64[i].base64data, DateTime.Now);
                }
                PhotoSns = PhotoSns.Trim(',');

                Console.WriteLine(queryPara);
                await _eqptService.SetTask(queryPara);
            }
            else
            {
                Console.WriteLine(queryPara);
                var response = await _eqptService.PostSubmitInspDevice(queryPara);

                if (!response.IsSuccess)
                {
                    base.HandleHttpRequestException(response);
                    return;
                }

                EqptRespondSubmitDevice responseData = response.Data;

                if (responseData == null)
                {
                    _toastService.ShowToast(response.ErrorMessage ?? "", ToastLevel.Warning);
                    return;
                }

                if (!responseData.Result.Equals("Done"))
                {
                    _toastService.ShowToast(responseData.Result, ToastLevel.Warning);
                    return;
                }

                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("S_Saved"), ToastLevel.Success);
                PhotoSns = responseData.PhotoSns ?? "";
            }
            var StartTime = _order.OrderDevices[orderDevIndex].StartTime;
            if (StartTime != null)
            {
                DateTime dt1 = DateTime.ParseExact(StartTime, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                DateTime dt2 = DateTime.ParseExact(NowTime, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                int ManMinute = 0;
                _order.OrderDevices[orderDevIndex].SubmitTime = NowTime.Substring(0, 16);
                _order.OrderDevices[orderDevIndex].Status = "Submitted";
                _order.OrderDevices[orderDevIndex].IsDisabled = true;
                TimeSpan ts = new TimeSpan(dt2.Ticks - dt1.Ticks);
                ManMinute = Convert.ToInt32(ts.TotalMinutes);

                EqptOrderRecord record = new EqptOrderRecord();
                record.RespondTime = NowTime.Substring(0, 16);
                record.StaffName = await base.GetUidAsync();
                record.Status = _order.OrderDevices[orderDevIndex].Status;
                record.Respond = newRespond;
                record.ManMinute = ManMinute;
                _order.OrderDevices[orderDevIndex].Records.Add(record);
                _order.OrderDevices[orderDevIndex].ManMinute = _order.OrderDevices[orderDevIndex].ManMinute + ManMinute;
                _order.OrderDevices[orderDevIndex].AfPhotoSns = PhotoSns;
                if (_isOnline && !PhotoSns.Equals(""))
                {
                    var l_photo = PhotoSns.Split(',').ToList();
                    if (l_photo != null)
                    {
                        for (int i = 0; i < l_photo.Count; i++)
                        {
                            await _eqptService.SetInspPhoto(l_photo[i], _filesBase64[i].base64data, DateTime.Now);
                        }
                    }
                }
                _order.OrderDevices[orderDevIndex].Respond = newRespond;
                newRespond = "";
                ManMinute = 0;//計算總花費
                bool allDeviceSubmitted = true;
                foreach (var dev in _order.OrderDevices)
                {
                    ManMinute = ManMinute + dev.ManMinute;
                    if (dev.SubmitTime == null) allDeviceSubmitted = false;
                }
                _order.ManMinute = ManMinute;
                if (allDeviceSubmitted && _order.RejectTime == null)
                {
                    _order.SubmitTime = NowTime.Substring(0, 16);
                    //_order.RejectTime = null;
                    _order.Status = "Submitted";
                    _order.StaffName = await base.GetUidAsync();
                    _order.StatusType = 2;
                    _order.StatusColor = "c-processed";
                    _order.StatusBGColor = "bgc-processed";
                }
            }
            await _eqptService.SetInspOrder(_order);

            await GetInspOrder();
        }

        private async Task InspSubmit() //已改為最後一個設備巡檢時變更巡檢單為完工上傳
        {
            string workRecord = _completionReportInputField.text;
            if (workRecord.Equals(""))
            {
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG027"), ToastLevel.Warning);//"請輸入完工說明!"
                return;
            }
            var queryPara = new Dictionary<string, string>() { };
            queryPara.Add(nameof(EqptRequestSubmitOrder.BuildingCode), _rBuildingCode);
            queryPara.Add(nameof(EqptRequestSubmitOrder.RecordSn), _recordSn);
            queryPara.Add(nameof(EqptRequestSubmitOrder.CompletionReport), workRecord);

            var NowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (!_isOnline)
            {
                queryPara.Add("NowTime", NowTime);
                queryPara.Add("Method", _eqptService.TaskMethod_SubmitInspOrder);

                Console.WriteLine(queryPara);
                await _eqptService.SetTask(queryPara);
            }
            else
            {
                var response = await _eqptService.PostSubmitInspOrder(queryPara);

                if (!response.IsSuccess)
                {
                    base.HandleHttpRequestException(response);
                    return;
                }
                // retryCnt = 0;
                EqptRespondSubmitOrder responseData = response.Data;

                if (responseData == null)
                {
                    _toastService.ShowToast(response.ErrorMessage ?? "", ToastLevel.Warning);
                    return;
                }

                if (!responseData.Result.Equals("Done"))
                {
                    _toastService.ShowToast(responseData.Result, ToastLevel.Warning);
                    return;
                }

                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("S_Submitted"), ToastLevel.Success);
            }
            _order.SubmitTime = NowTime.Substring(0, 16);
            _order.RejectTime = null;
            _order.Status = "Submitted";
            _order.StaffName = await base.GetUidAsync();
            _order.StatusType = 2;
            _order.StatusColor = "c-processed";
            _order.StatusBGColor = "bgc-processed";
            int ManMinute = 0;
            foreach (var dev in _order.OrderDevices)
                ManMinute = ManMinute + dev.ManMinute;
            _order.ManMinute = ManMinute;
            await _eqptService.SetInspOrder(_order);

            await GetInspOrder();
        }
    }
}
