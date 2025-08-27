using iBMSApp.Services;
using iBMSApp.Shared;
using iBMSApp.UI.Common;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace iBMSApp.UI.Pages
{
    public class InspScanView : NetworkPageMonoBehavior<InspScanView>
    {
        [Header("UI")]
        [SerializeField] private RectTransform _webcamPreviewContent;
        [SerializeField] private RawImage _webcamPreviewImage;
        [SerializeField] private Button _startScanButton;
        [SerializeField] private Button _stopScanButton;

        // Services(DI)
        private EqptService _eqptService;
        private ILocalStorageService _localStorage;
        private QRCodeDetector _qrcodeScanner;

        private Quaternion _previewBaseRotation;
        private string _recordSn = string.Empty;
        private string _rBuildingCode = string.Empty;
        private string _deviceCode;
        private bool _updateLocker = true;
        private bool _scanLocker = false;

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

            // 更新預覽圖片元件的旋轉修正
            if (_qrcodeScanner.IsScanning) 
            {
                QRCodeDetector.DisplayCaptureCorrectly(_webcamPreviewContent, _previewBaseRotation);
            }
        }

        /// <inheritdoc/>
        public override void Show()
        {
            StopScanningQR();
            this.gameObject.SetActive(true);
        }

        /// <inheritdoc/>
        public override void Hide()
        {
            this.gameObject.SetActive(false);
            StopScanningQR();
        }

        /// <inheritdoc/>
        public override async Task RefreshPage(PageRefreshParams refreshParams = null)
        {
            if (refreshParams != null)
            {
                _rBuildingCode = refreshParams.BuildingCode;
                _recordSn = refreshParams.RecordSn;
                _deviceCode = refreshParams.DeviceCode;
            }

            await Task.CompletedTask;
        }
        #endregion

        private bool LoadServices()
        {
            if (ServiceManager.Instance == null)
                return false;

            _eqptService = ServiceManager.Instance.EqptService;
            _localStorage = ServiceManager.Instance.LocalStorageService;
            _toastService = ServiceManager.Instance.ToastService;
            _qrcodeScanner = ServiceManager.Instance.QRCodeDetector;
            return true;
        }

        private void InitUIOnStart()
        {
            _previewBaseRotation = _webcamPreviewContent.transform.rotation;
            Hide();
        }

        public void OnStartScanningClicked() => StartScanningQR();

        public void OnStopScanningClicked() => StopScanningQR();

        private async void StartScanningQR()
        {
            bool startSuccess = await _qrcodeScanner.StartScanningAsync(_webcamPreviewImage);

            if (!startSuccess)
            {
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("T_NoDetectCamera"), ToastLevel.Error);
                return;
            }

            _qrcodeScanner.QRCodeScanned += ScanQREvent;

            _webcamPreviewContent.gameObject.SetActive(true);
            _startScanButton.gameObject.SetActive(false);
            _stopScanButton.gameObject.SetActive(true);
        }

        private void StopScanningQR()
        {
            _webcamPreviewContent.gameObject.SetActive(false);
            _startScanButton.gameObject.SetActive(true);
            _stopScanButton.gameObject.SetActive(false);
            _webcamPreviewImage.texture = null;

            _qrcodeScanner.QRCodeScanned -= ScanQREvent;
            _qrcodeScanner.StopScanning();
        }

        private void ScanQREvent(string qrText) => _ = HandleQRcodeScan(qrText);

        private async Task HandleQRcodeScan(string barcode)
        {
            if (_scanLocker) return;
            _scanLocker = true;

            var scanResult = barcode.Trim();
            DebugLog("InputChanged. scanResult:" + scanResult);
            if (scanResult.IndexOf("queryPara=") < 0)
            {
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG023"), ToastLevel.Warning);//"QR code 格式不正確 !"
                _scanLocker = false;
                return;
            }
            string ss = scanResult?.Substring(scanResult.IndexOf("queryPara=") + 10) ?? "";
            var jsonDoc = JsonDocument.Parse(ss);
            var root = jsonDoc.RootElement;
            var qr_DeviceCode = "";
            var qr_Code = "";

            if (root.TryGetProperty("BuildingCode", out JsonElement val1))
            {
                string qr_BuildingCode = val1.GetString() ?? "";
                if (!_rBuildingCode.Equals(qr_BuildingCode))
                {
                    _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG042"), ToastLevel.Warning);//"非本設備 QR Code，請重新掃描！"
                    _scanLocker = false;
                    return;
                }
            }

            if (root.TryGetProperty("DeviceCode", out JsonElement val3))
            {
                qr_DeviceCode = val3.GetString() ?? "";
            }

            if (root.TryGetProperty("deviceCode", out JsonElement val4))
            {
                qr_DeviceCode = val4.GetString() ?? "";
            }

            if (root.TryGetProperty("Code", out JsonElement val5))
            {
                qr_Code = val5.GetString() ?? "";
            }
            else qr_Code = qr_DeviceCode;

            if (qr_Code.Equals(""))
            {
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG041"), ToastLevel.Warning);//"掃描內容無設備 QR Code，請重新掃描！"
                _scanLocker = false;
                return;
            }
            else
            {
                var matchedDevice = await _eqptService.GetDeviceData(x => x.Code == qr_Code);
                
                if (matchedDevice == null || matchedDevice?.DeviceCode != _deviceCode)
                {
                    _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG042"), ToastLevel.Warning);//"非本設備 QR Code，請重新掃描！"
                    _scanLocker = false;
                    return;
                }
            }

            if (!_isOnline)
            {
                if (await _eqptService.SetDevice(qr_Code))
                {
                    await StartInsp();
                    base.GoToPagePrevious();
                }
                else
                {
                    _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG046"), ToastLevel.Warning);//"系統無此設備!"
                    _scanLocker = false;
                    return;
                }
            }
            else
            {
                await GetDeviceInfo(qr_Code);
            }
            _scanLocker = false;
        }

        /// <summary>
        /// 上傳格式詳見<seealso cref="EqptRequestUpdateOrderDevice"/>
        /// </summary>
        private async Task StartInsp()
        {
            string uid = await base.GetUidAsync();
            string NowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var queryPara = new Dictionary<string, string>() { };

            queryPara.Add(nameof(EqptRequestUpdateOrderDevice.BuildingCode), _rBuildingCode);
            queryPara.Add(nameof(EqptRequestUpdateOrderDevice.RecordSn), _recordSn);
            queryPara.Add(nameof(EqptRequestUpdateOrderDevice.DeviceCode), _deviceCode);

            if (!_isOnline)
            {
                queryPara.Add("NowTime", NowTime);
                queryPara.Add("Method", _eqptService.TaskMethod_StartInsp);

                //DebugLog(queryPara);
                await _eqptService.SetTask(queryPara);
            }
            else
            {
                var response = await _eqptService.PostStartInsp(queryPara);

                if (!response.IsSuccess)
                {
                    base.HandleHttpRequestException(response);
                    return;
                }
            }
            NowTime = NowTime.Substring(0, 16);//去掉秒的顯示
            var order = await _eqptService.GetInspOrder(_rBuildingCode, _recordSn);
            order.StartTime = NowTime;
            order.Status = "Processing";
            order.StatusType = 1;
            order.StatusColor = "c-processing";
            order.StatusBGColor = "bgc-processing";
            int orderDevIndex = order.OrderDevices.FindIndex(dev => dev.BuildingCode == _rBuildingCode && dev.DeviceCode == _deviceCode);

            order.OrderDevices[orderDevIndex].StartTime = NowTime;
            order.OrderDevices[orderDevIndex].PauseTime = null;
            order.OrderDevices[orderDevIndex].Status = "Processing";
            order.OrderDevices[orderDevIndex].IsDisabled = false;

            EqptOrderRecord record = new EqptOrderRecord();
            record.RespondTime = NowTime;
            record.StaffName = uid;
            record.Status = order.OrderDevices[orderDevIndex].Status;
            record.Respond = "Start";
            order.OrderDevices[orderDevIndex].Records.Add(record);

            await _eqptService.SetInspOrder(order);
        }

        /// <summary>
        /// 上傳格式詳見<seealso cref="EqptRequestDeviceBasic"/>
        /// </summary>
        /// <param name="code"><seealso cref="DeviceStaticInfo.Code"/></param>
        /// <returns></returns>
        private async Task GetDeviceInfo(string code)
        {
            var queryPara = new Dictionary<string, string>() { };

            queryPara.Add(nameof(EqptRequestDeviceBasic.BuildingCode), _rBuildingCode);
            queryPara.Add(nameof(EqptRequestDeviceBasic.DeviceCode), code);

            var response = await _eqptService.PostDeviceBasic(queryPara);

            if (!response.IsSuccess)
            {
                base.HandleHttpRequestException(response);
                return;
            }

            var result = response.Data;
            if (result?.DeviceCode == null)
            {
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG046"), ToastLevel.Warning); // "系統無此設備!"
                return;
            }

            //DebugLog(result);
            await _eqptService.SetDevice(code);
            await StartInsp();

            base.GoToPagePrevious();
        }
    }
}
