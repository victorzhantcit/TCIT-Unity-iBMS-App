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
    public class RepairScanView : NetworkPageMonoBehavior<RepairScanView>
    {
        [SerializeField] private RepairOrderPageRouter _pageRouter;

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
        private string _rBuildingCode = "RG";
        private string _deviceCode;
        private bool _updateLocker = true;

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
        public override Task RefreshPage(PageRefreshParams refreshParams = null)
        {
            DebugLog("RefreshPage");
            if (refreshParams != null)
            {
                _rBuildingCode = refreshParams.BuildingCode;
                _recordSn = refreshParams.RecordSn;
                _deviceCode = refreshParams.DeviceCode;
            }

            StopScanningQR();
            return Task.CompletedTask;
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

        /// 初始化的 UI 顯示狀態由 <seealso cref="RepairOrderPageRouter"/> 管理
        private void InitUIOnStart()
        {
            _previewBaseRotation = _webcamPreviewContent.transform.rotation;
        }

        //public void NavigatePreviousPage()
        //{
        //    Hide();
        //    _ = _previousPage.RefreshPage();
        //    _previousPage.Show();
        //}

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
            
            if (_qrcodeScanner != null)
            {
                _qrcodeScanner.QRCodeScanned -= ScanQREvent;
                _qrcodeScanner.StopScanning();
            }
        }

        private void ScanQREvent(string qrText) => _ = HandleQRcodeScan(qrText);

        private async Task HandleQRcodeScan(string barcode)
        {
            var scanResult = barcode.Trim();
            Console.WriteLine("InputChanged. scanResult:" + scanResult);
            if (scanResult.IndexOf("queryPara=") < 0)
            {
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG023"), ToastLevel.Warning);//"QR code 格式不正確 !"
                return;
            }
            string ss = scanResult?.Substring(scanResult.IndexOf("queryPara=") + 10) ?? "";
            var jsonDoc = JsonDocument.Parse(ss);
            var root = jsonDoc.RootElement;
            var qr_DeviceCode = "";
            var qr_Code = "";
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
                return;
            }
            //以 Code 為主
            if (!_isOnline)
            {
                var foundDevice = await _eqptService.GetDeviceData(x => x.Code == qr_Code);
                if (foundDevice != null) // 將 DeviceCode 對應的設備資訊存入 LocalStorage
                {
                    GoToOrderView();
                }
                else
                {
                    _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG046"), ToastLevel.Warning);//"系統無此設備!"
                    return;
                }
            }
            else
            {
                await GetDeviceInfo(qr_Code);
                GoToOrderView();
            }
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
        }

        private void GoToOrderView()
        {
            _pageRouter.SilenceToggleListPageTag();
            base.GoToPageChild(new PageRefreshParams { PageReadOnly = false });
        }
    }
}
