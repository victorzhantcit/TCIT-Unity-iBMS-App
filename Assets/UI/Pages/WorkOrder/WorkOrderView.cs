using iBMSApp.DataModels;
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
using Random = System.Random;

namespace iBMSApp.UI.Pages
{
    public class WorkOrderView : NetworkPageMonoBehavior<WorkOrderView>
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
        [SerializeField] private WorkDevicesVirtualList _workDevicesVirtualList;
        [SerializeField] private TMP_Text _recordSummaryText;
        [SerializeField] private TMP_InputField _completionReportInputField;
        [SerializeField] private Button _submitButton;
        [SerializeField] private TMP_Text _submitButtonText;

        private List<ImageFile> _prePhotoFiles = new List<ImageFile>();
        private List<ImageFile> _afPhotoFiles = new List<ImageFile>();

        private EqptOrder _order;
        private string _recordSn = string.Empty;
        private string _rBuildingCode = string.Empty;
        private string _deviceCode = string.Empty;
        private float _scrollValue = 1f;
        private int orderDevIndex = -1;
        private bool _updateLocker = true;
        private bool _allDeviceSubmitted = false;

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
            if (refreshParams != null)
            {
                _rBuildingCode = refreshParams.BuildingCode;
                _recordSn = refreshParams.RecordSn;
            }
            _deviceCode = "";

            await GetWorkOrder();
        }
        #endregion

        private void InitUIOnStart()
        {
            Hide();
        }

        private bool LoadServices()
        {
            DebugLog("LoadServices");
            if (ServiceManager.Instance == null)
                return false;

            _eqptService = ServiceManager.Instance.EqptService;
            _localStorage = ServiceManager.Instance.LocalStorageService;
            _toastService = ServiceManager.Instance.ToastService;

            DebugLog("LoadServices true");
            return true;
        }

        public void OnSubmitWorkClicked() => _ = WorkSubmit();

        private async Task GetWorkOrder()
        {
            if (_deviceCode.Equals("0")) _deviceCode = "";
            _order = await _eqptService.GetWorkOrder(_rBuildingCode, _recordSn);
            bool allDeviceSubmitted = true;
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
                    if (!dev.Status.Equals("Submitted")) allDeviceSubmitted = false; 
                }
            }
            _allDeviceSubmitted = allDeviceSubmitted;
            if (!_deviceCode.Equals(""))
            {
                orderDevIndex = _order.OrderDevices.FindIndex(dev => dev.BuildingCode == _rBuildingCode && dev.DeviceCode == _deviceCode);
                DebugLog("orderDevIndex:" + orderDevIndex);

                _prePhotoFiles.Clear();
                if (!string.IsNullOrEmpty(_order.OrderDevices[orderDevIndex].PrePhotoSns))
                {
                    var PhotoSnArray = (_order.OrderDevices[orderDevIndex].PrePhotoSns ?? "")//.Trim('[', ']').Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "")
                            .Split(",")
                            .Select(x => x.Trim('"'))
                            .ToArray();
                    foreach (var sn in PhotoSnArray)
                    {
                        await GetPhoto(0, sn);
                    }
                }

                _afPhotoFiles.Clear();
                if (!string.IsNullOrEmpty(_order.OrderDevices[orderDevIndex].AfPhotoSns))
                {
                    var PhotoSnArray = (_order.OrderDevices[orderDevIndex].AfPhotoSns ?? "")//.Trim('[', ']').Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "")
                            .Split(",")
                            .Select(x => x.Trim('"'))
                            .ToArray();
                    foreach (var sn in PhotoSnArray)
                    {
                        await GetPhoto(1, sn);
                    }
                }
            }

            UpdateOrder();
        }

        private async Task GetPhoto(int type, string sn)
        {
            PhotoStorageDto photo = await _eqptService.GetWorkPhoto(sn);
            if (photo == null) return;

            ImageFile imageFile = new ImageFile { base64data = photo.Img, contentType = "", fileName = "" };
            if (type == 0)
                _prePhotoFiles.Add(imageFile);
            else if (type == 1)
                _afPhotoFiles.Add(imageFile);

            DebugLog($"type{type}.Add {photo.Img.Substring(0, 30)}...");
        }

        // 以 _order 進行更新
        private void UpdateOrder()
        {
            bool isRejected = _order.RejectTime != null;
            bool isCompleted = _order.CompleteTime != null;
            bool isSubmitted = _order.SubmitTime != null; 
            bool isUndone = isRejected || (!isCompleted && !isSubmitted);
            StringBuilder sb = new StringBuilder();

            // 頁面標頭
            _recordSnText.text = MakeLocalizedHeaderRichText("T_WorkOrderNo", _order.RecordSn);
            _orderStatusBG.color = ColorMapper.Instance.GetColor(_order.StatusBGColor);
            _statusText.text = Localizer.Instance.GetLocalizedString($"S_{_order.Status}");
            _descriptionText.text = MakeLocalizedHeaderRichText("T_Description", _order.Description);

            // 基本資訊
            sb.AppendLine(MakeLocalizedContentRichText("T_Building", _order.BuildingName));
            if(!string.IsNullOrEmpty(_order.StaffName))
                sb.AppendLine(MakeLocalizedContentRichText("T_WorkLeader", _order.StaffName));
            else if (!string.IsNullOrEmpty(_order.ExecutorName))
                sb.AppendLine(MakeLocalizedContentRichText("T_WorkLeader", _order.ExecutorName));
            else
                sb.AppendLine(MakeLocalizedBothContentRichText("T_WorkLeader", "S_NotSet"));
            sb.AppendLine(MakeLocalizedContentRichText("T_ExecutionTime", _order.ScheduledDate));

            _recordInfo.text = sb.ToString();
            sb.Clear();

            // 設備列表
            UpdateDeviceList();

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
                else
                {
                    if (!string.IsNullOrEmpty(_completionReportInputField.text))
                        sb.AppendLine(MakeLocalizedContentRichText("T_CompletionReport", _completionReportInputField.text));
                }

                if (isCompleted)
                {
                    sb.AppendLine(MakeLocalizedContentRichText("T_CompleteTime", _order.CompleteTime));
                }
                else if (isRejected)
                {
                    sb.AppendLine(MakeLocalizedContentRichText("T_RejectTime", _order.RejectTime));
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

            RectTransform pageRef = _scrollView.GetComponentInParent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(pageRef);
            _scrollView.verticalNormalizedPosition = _scrollValue;
        }

        private void UpdateDeviceList()
        {
            _workDevicesVirtualList.ClearList();

            if (_order.OrderDevices.Count <= 0)
                return;

            RectTransform pageRef = _scrollView.GetComponentInParent<RectTransform>();
            foreach (EqptOrderDevice device in _order.OrderDevices)
            {
                _workDevicesVirtualList.BindItem(null, false , (item, data) =>
                {
                    item.SetPageContentRef(pageRef);
                    item.BindData(device, _prePhotoFiles, _afPhotoFiles, _order.RejectTime != null);

                    // 更新圖示與事件綁定
                    if (device.DeviceCode == _deviceCode)
                    {
                        item.SetFunctionalStatus(EnumDeviceListItemStatus.Collapse, () =>
                        {
                            CloseDeviceCode(device.DeviceCode);
                        });
                    }
                    else if (device.StartTime == null || device.PauseTime != null)
                    {
                        item.SetFunctionalStatus(EnumDeviceListItemStatus.QRScan, () =>
                        {
                            ScanCode(device.DeviceCode, 0);
                        });
                    }
                    else
                    {
                        item.SetFunctionalStatus(EnumDeviceListItemStatus.Expand, () =>
                        {
                            ScanCode(device.DeviceCode, 1);
                        });
                    }

                    item.BindEventSavePrePhotos(() => _ = SavePrePhoto(device.WorkRecordsdSn, item));
                    item.BindEventSaveConsumableData(() => _ = SaveConsumables(item));
                    item.BindEventDeviceSubmit(type => _ = DeviceWorkSubmit(type));
                    item.BindEventDeviceWorkStart(() => _ = DeviceWorkStart());
                    item.BindEventToastMessage(_toastService.ShowToast);
                    item.BindEventDeviceCollapse(() => CloseDeviceCode(device.DeviceCode));
                });
            }
            _workDevicesVirtualList.RefreshLayout();
        }

        private async void ScanCode(string deviceCode, int type)
        {
            if (type == 0)
            {
                base.GoToPageChild(new PageRefreshParams
                {
                    BuildingCode = _rBuildingCode,
                    RecordSn = _recordSn,
                    DeviceCode = deviceCode
                });
            }
            else
            {
                _deviceCode = deviceCode;
                _scrollValue = _scrollView.verticalNormalizedPosition; // 展開項目時保持滑動固定
                await GetWorkOrder();
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
        /// 上傳格式詳見<seealso cref="EqptRequestUpdateOrderDevice"/>
        /// </summary>
        /// <returns></returns>
        private async Task DeviceWorkStart() // 理論上是在 OrderScan 執行
        {
            var queryPara = new Dictionary<string, string>() { };
            queryPara.Add(nameof(EqptRequestUpdateOrderDevice.BuildingCode), _rBuildingCode);
            queryPara.Add(nameof(EqptRequestUpdateOrderDevice.RecordSn), _recordSn);
            queryPara.Add(nameof(EqptRequestUpdateOrderDevice.DeviceCode), _deviceCode);
            var NowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (!_isOnline)
            {
                queryPara.Add("NowTime", NowTime);
                queryPara.Add("Method", _eqptService.TaskMethod_StartWorkOrder);

                Console.WriteLine(queryPara);
                await _eqptService.SetTask(queryPara);
            }
            else
            {
                var response = await _eqptService.PostStartWorkOrder(queryPara);

                if (!response.IsSuccess)
                {
                    base.HandleHttpRequestException(response);
                    return;
                }
                // retryCnt = 0;
            }
            NowTime = NowTime.Substring(orderDevIndex, 16);
            _order.StartTime = NowTime;
            _order.Status = "Processing";
            _order.StatusType = 1;
            _order.StatusColor = "c-processing";
            _order.StatusBGColor = "bgc-processing";
            _allDeviceSubmitted = false;
            _order.OrderDevices[orderDevIndex].StartTime = NowTime;
            _order.OrderDevices[orderDevIndex].PauseTime = null;
            _order.OrderDevices[orderDevIndex].Status = "Processing";
            _order.OrderDevices[orderDevIndex].IsDisabled = false;
            EqptOrderRecord record = new EqptOrderRecord();
            record.RespondTime = NowTime;
            record.StaffName = await base.GetUidAsync();
            record.Status = _order.OrderDevices[orderDevIndex].Status;
            record.Respond = "Start";
            _order.OrderDevices[orderDevIndex].Records.Add(record);
            await _eqptService.SetWorkOrder(_order);

            await GetWorkOrder();
        }

        /// <summary>
        /// 上傳格式詳見<seealso cref="EqptRequestSaveOrderConsumables"/>
        /// </summary>
        /// <returns></returns>
        private async Task SaveConsumables(WorkDeviceListItem refreshUIReference)
        {
            var queryPara = new Dictionary<string, string>() { };
            int recordsdSn = _order.OrderDevices[orderDevIndex].WorkRecordsdSn;
            queryPara.Add(nameof(EqptRequestSaveOrderConsumables.BuildingCode), _rBuildingCode);
            queryPara.Add(nameof(EqptRequestSaveOrderConsumables.DeviceCode), _deviceCode);
            queryPara.Add(nameof(EqptRequestSaveOrderConsumables.RecordSn), recordsdSn.ToString());

            var NowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var Consumables = "";
            foreach (var cons in _order.OrderDevices[orderDevIndex].Consumables)
            {
                if (cons.isChecked == true)
                    cons.ReplaceDate = NowTime.Substring(0, 16);
                if (!Consumables.Equals("")) Consumables = Consumables + ",";
                Consumables = Consumables + "{\"Name\":\"" + cons.Name + "\",\"AvailableNum\":\"" + cons.AvailableNum
                + "\",\"AvailableUnit\":\"" + cons.AvailableUnit + "\",\"ReplaceDate\":\"" + cons.ReplaceDate + "\"}";
            }
            Consumables = "[" + Consumables + "]";
            queryPara.Add(nameof(EqptRequestSaveOrderConsumables.Consumables), Consumables);
            if (!_isOnline)
            {
                queryPara.Add("NowTime", NowTime);
                queryPara.Add("Method", _eqptService.TaskMethod_SaveWorkConsumables);
                await _eqptService.SetTask(queryPara);

                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("S_Saved"), ToastLevel.Success);//"已儲存"
            }
            else
            {
                var response = await _eqptService.PostSaveWorkConsumables(queryPara);

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
                await _eqptService.SetWorkOrder(_order);
            }

            refreshUIReference.RefreshConsumables();
        }

        /// <summary>
        /// 上傳格式詳見<seealso cref="EqptRequestSavePrePhoto"/>
        /// </summary>
        /// <param name="WorkRecordsdSn"></param>
        /// <returns></returns>
        private async Task SavePrePhoto(int WorkRecordsdSn, WorkDeviceListItem refreshUIReference)
        {
            if (_prePhotoFiles.Count == 0)
            {
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG043"), ToastLevel.Warning); //"請進行處理前拍照!"
                return;
            }

            string Photos = "[";
            for (int i = 0; i < _prePhotoFiles.Count; i++)
            {
                if (Photos.Equals("[")) Photos = "[" + _prePhotoFiles[i].base64data;
                else Photos = Photos + "," + _prePhotoFiles[i].base64data;
            }
            Photos = Photos + "]";

            var queryPara = new Dictionary<string, string>() { };
            queryPara.Add(nameof(EqptRequestSavePrePhoto.BuildingCode), _rBuildingCode);
            queryPara.Add(nameof(EqptRequestSavePrePhoto.RecordSn), _recordSn);
            //當 WorkRecordsdSn == 0 需要 DeviceCode 來找最新一筆 eqpt_work_records
            queryPara.Add(nameof(EqptRequestSavePrePhoto.DeviceCode), _deviceCode);
            queryPara.Add(nameof(EqptRequestSavePrePhoto.Photos), Photos);
            queryPara.Add(nameof(EqptRequestSavePrePhoto.WorkRecordsdSn), WorkRecordsdSn.ToString());

            var PrePhotoSns = "";
            if (!_isOnline)
            {
                var NowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                queryPara.Add("NowTime", NowTime);
                queryPara.Add("Method", _eqptService.TaskMethod_SaveWorkPrePhotos);

                Console.WriteLine(queryPara);
                await _eqptService.SetTask(queryPara);

                Random rnd = new Random();  //產生亂數初始值
                for (int i = 0; i < _prePhotoFiles.Count; i++)
                {
                    var sn = "T" + rnd.Next(1, 1000);   //亂數產生，亂數產生的範圍是1~1000
                    PrePhotoSns = PrePhotoSns + sn + ",";
                    await _eqptService.SetWorkPhoto(sn, _prePhotoFiles[i].base64data, DateTime.Now);
                }
                PrePhotoSns = PrePhotoSns.Trim(',');

                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("S_Saved"), ToastLevel.Success);//"已儲存"
            }
            else
            {
                var response = await _eqptService.PostSaveWorkPrePhoto(queryPara);

                if (!response.IsSuccess)
                {
                    base.HandleHttpRequestException(response);
                    return;
                }

                EqptRespondSavePrePhoto responseData = response.Data;

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

                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("S_Saved"), ToastLevel.Success);//"已儲存"
                await _eqptService.SetWorkOrder(_order);
            }

            //SavePrePhoto
            _order.OrderDevices[orderDevIndex].WorkRecordsdSn = WorkRecordsdSn;//一開始 WorkRecordsdSn 可能為 0
            _order.OrderDevices[orderDevIndex].PrePhotoSns = PrePhotoSns;
            if (_isOnline && !PrePhotoSns.Equals(""))
            {
                var l_photo = PrePhotoSns.Split(',').ToList();
                if (l_photo != null)
                {
                    for (int i = 0; i < l_photo.Count; i++)
                    {
                        await _eqptService.SetWorkPhoto(l_photo[i], _prePhotoFiles[i].base64data, DateTime.Now);
                    }
                }
                await _eqptService.SetWorkOrder(_order);
            }

            refreshUIReference.RefreshPrePhotos();
        }

        /// <summary>
        /// 上傳格式詳見<seealso cref="EqptRequestSubmitWorkDevice"/>
        /// </summary>
        /// <param name="type">0:紀錄，1:暫結，2:完工</param>
        /// <returns></returns>
        private async Task DeviceWorkSubmit(int type)
        {
            string newRespond = _order.OrderDevices[orderDevIndex].Respond;
            if (newRespond.Equals(""))
            {
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG033"), ToastLevel.Warning);//請輸入設備備註說明！
                return;
            }

            if (_afPhotoFiles.Count == 0 && type == 2)
            {
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG034"), ToastLevel.Warning);//"請附加處理後照片!"
                return;
            }
            string Photos = "[";
            for (int i = 0; i < _afPhotoFiles.Count; i++)
            {
                if (Photos.Equals("[")) Photos = "[" + _afPhotoFiles[i].base64data;
                else Photos = Photos + "," + _afPhotoFiles[i].base64data;
            }
            Photos = Photos + "]";
            string RecordType = type.ToString();
            var queryPara = new Dictionary<string, string>() { };
            queryPara.Add(nameof(EqptRequestSubmitWorkDevice.RecordType), RecordType);
            queryPara.Add(nameof(EqptRequestSubmitWorkDevice.BuildingCode), _rBuildingCode);
            queryPara.Add(nameof(EqptRequestSubmitWorkDevice.RecordSn), _recordSn);
            queryPara.Add(nameof(EqptRequestSubmitWorkDevice.DeviceCode), _deviceCode);
            queryPara.Add(nameof(EqptRequestSubmitWorkDevice.DeviceCount), _order.OrderDevices.Count().ToString());

            queryPara.Add(nameof(EqptRequestSubmitWorkDevice.Respond), newRespond);
            queryPara.Add(nameof(EqptRequestSubmitWorkDevice.Photos), Photos);
            var NowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var AfPhotoSns = "";
            if (!_isOnline)
            {
                queryPara.Add("NowTime", NowTime);
                queryPara.Add("Method", _eqptService.TaskMethod_SubmitWorkDevice);
                Random rnd = new Random();  //產生亂數初始值
                for (int i = 0; i < _afPhotoFiles.Count; i++)
                {
                    var sn = "T" + rnd.Next(1, 1000);   //亂數產生，亂數產生的範圍是1~1000
                    AfPhotoSns = AfPhotoSns + sn + ",";
                    await _eqptService.SetWorkPhoto(sn, _afPhotoFiles[i].base64data, DateTime.Now);
                }
                AfPhotoSns = AfPhotoSns.Trim(',');
                Console.WriteLine(queryPara);
                await _eqptService.SetTask(queryPara);
            }
            else
            {
                var response = await _eqptService.PostSubmitWorkDevice(queryPara);

                if (!response.IsSuccess)
                {
                    base.HandleHttpRequestException(response);
                    return;
                }

                EqptRespondSubmitWorkDevice responseData = response.Data;

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

                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("S_Saved"), ToastLevel.Success);// "已儲存"
                AfPhotoSns = responseData.AfPhotoSns ?? "";
            }

            var StartTime = _order.OrderDevices[orderDevIndex].StartTime;
            if (StartTime != null)
            {
                DateTime dt1 = DateTime.ParseExact(StartTime, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                DateTime dt2 = DateTime.ParseExact(NowTime, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                int ManMinute = 0;
                if (type == 1)//0:紀錄，1:暫結，2:完工
                {
                    _order.OrderDevices[orderDevIndex].PauseTime = NowTime.Substring(0, 16);
                    _order.OrderDevices[orderDevIndex].Status = "Pause";
                    _order.OrderDevices[orderDevIndex].IsDisabled = true;
                    TimeSpan ts = new TimeSpan(dt2.Ticks - dt1.Ticks);
                    ManMinute = Convert.ToInt32(ts.TotalMinutes);
                }
                else if (type == 2)
                {
                    _order.OrderDevices[orderDevIndex].SubmitTime = NowTime.Substring(0, 16);
                    _order.OrderDevices[orderDevIndex].Status = "Submitted";
                    _order.OrderDevices[orderDevIndex].IsDisabled = true;
                    TimeSpan ts = new TimeSpan(dt2.Ticks - dt1.Ticks);
                    ManMinute = Convert.ToInt32(ts.TotalMinutes);

                    if (_order.OrderDevices.Count == 1)
                    {
                        _order.SubmitTime = NowTime.Substring(0, 16);
                        _order.RejectTime = null;
                        _order.Status = "Submitted";
                        _order.StaffName = await base.GetUidAsync();
                        _order.StatusType = 2;
                        _order.StatusColor = "c-processed";
                        _order.StatusBGColor = "bgc-processed";
                    }
                }
                EqptOrderRecord record = new EqptOrderRecord();
                record.RespondTime = NowTime.Substring(0, 16);
                record.StaffName = await base.GetUidAsync();
                record.Status = _order.OrderDevices[orderDevIndex].Status;
                record.Respond = newRespond;
                record.ManMinute = ManMinute;
                _order.OrderDevices[orderDevIndex].Records.Add(record);
                _order.OrderDevices[orderDevIndex].ManMinute = _order.OrderDevices[orderDevIndex].ManMinute + ManMinute;
                _order.OrderDevices[orderDevIndex].AfPhotoSns = AfPhotoSns;
                if (_isOnline && !AfPhotoSns.Equals(""))
                {
                    var l_photo = AfPhotoSns.Split(',').ToList();
                    if (l_photo != null)
                    {
                        for (int i = 0; i < l_photo.Count; i++)
                        {
                            await _eqptService.SetWorkPhoto(l_photo[i], _afPhotoFiles[i].base64data, DateTime.Now);
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
                    if (!dev.Status.Equals("Submitted")) allDeviceSubmitted = false;
                }
                _allDeviceSubmitted = allDeviceSubmitted;
                _order.ManMinute = ManMinute;
                await _eqptService.SetWorkOrder(_order);
            }

            await GetWorkOrder();
        }

        /// <summary>
        /// 上傳格式 <seealso cref="EqptRequestSubmitOrder"/>
        /// </summary>
        /// <returns></returns>
        private async Task WorkSubmit()
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
                queryPara.Add("Method", _eqptService.TaskMethod_SubmitWorkOrder);

                Console.WriteLine(queryPara);
                await _eqptService.SetTask(queryPara);

                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("S_Saved"), ToastLevel.Success);//"已儲存"
            }
            else
            {
                var response = await _eqptService.PostSubmitWorkOrder(queryPara);

                if (!response.IsSuccess)
                {
                    base.HandleHttpRequestException(response);
                    return;
                }

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

                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("S_Sent"), ToastLevel.Success);//"已送出"
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
            await _eqptService.SetWorkOrder(_order);

            await GetWorkOrder();
        }
    }
}
