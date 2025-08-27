using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

namespace iBMSApp.Services
{
    public class QRCodeDetector : MonoBehaviour
    {
        public static QRCodeDetector Instance { get; private set; }

        public delegate void OnQRCodeScanned(string qrText);
        public event OnQRCodeScanned QRCodeScanned;

        [Header("Scanner Settings")]
        [SerializeField] private WebCamManager _webCamManager;
        [SerializeField] private float _scanInterval = 0.5f;

        private IBarcodeReader barcodeReader;
        private float lastScanTime = 0f;

        public bool IsScanning => _webCamManager.IsPlaying();

        public WebCamTexture WebcamTexture => _webCamManager.WebcamTexture;

        void Awake()
        {
            // Unity Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // 建立 ZXing 解碼器
            barcodeReader = new BarcodeReader();
        }

        private void Update()
        {
            if (!_webCamManager.IsPlaying())
                return;

            if (Time.time - lastScanTime > _scanInterval)
            {
                TryScan();
                lastScanTime = Time.time;
            }
        }

        private void TryScan()
        {
            try
            {
                WebCamTexture webcamTexture = _webCamManager.WebcamTexture;
                Color32[] pixels = webcamTexture.GetPixels32();

                var result = barcodeReader.Decode(pixels, webcamTexture.width, webcamTexture.height);
                if (result != null)
                {
                    Debug.Log("掃描到 QRCode: " + result.Text);
                    QRCodeScanned?.Invoke(result.Text);
                }
                Debug.Log("嘗試掃描QR...");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("QRCode 解碼失敗: " + ex.Message);
            }
        }

        public void StopScanning()
        {
            if (_webCamManager.IsPlaying())
                _webCamManager.StopCamera();
        }

        public async Task<bool> StartScanningAsync(RawImage preview = null)
        {
            if (_webCamManager.IsPlaying())
                return false;

            WebCamTexture webcamTexture = _webCamManager.StartCamera();

            if (webcamTexture == null)
                return false;

            preview.texture = webcamTexture;

            // 非同步等待 webcamTexture 解析度就緒
            await WaitForWebcamResolution(webcamTexture);

            // 套用 AspectRatioFitter
            if (preview.TryGetComponent<AspectRatioFitter>(out AspectRatioFitter aspectFitter))
            {
                float ratio = (float)webcamTexture.width / webcamTexture.height;
                aspectFitter.aspectRatio = ratio;
            }

            return true;
        }

        private async Task<bool> WaitForWebcamResolution(WebCamTexture webcamTexture, int maxFrameWait = 30)
        {
            for (int frameCount = 0; frameCount < maxFrameWait; frameCount++)
            {
                if (webcamTexture.width > 16 && webcamTexture.height > 16)
                {
                    return true; // 已準備好
                }

                await Task.Yield(); // 等一幀
            }

            // 超過等待次數仍未就緒
            Debug.LogWarning($"[{nameof(QRCodeDetector)}] WebCamTexture resolution did not initialize in time.");
            return false;
        }


        public bool StartScanning(RawImage preview = null)
        {
            if (!_webCamManager.IsPlaying())
            {
                WebCamTexture webcamTexture = _webCamManager.StartCamera();

                if (webcamTexture != null)
                {
                    preview.texture = webcamTexture;
                    if (preview.TryGetComponent<AspectRatioFitter>(out AspectRatioFitter aspectFitter))
                    {
                        aspectFitter.aspectRatio = (float)webcamTexture.width / webcamTexture.height;
                    }   
                    return true;
                }
                return false;
            }
            return false;
        }

        public static void DisplayCaptureCorrectly(Transform display, Quaternion displayBaseRotation)
        {
            WebCamTexture webCamTexture = Instance.WebcamTexture;

            // 處理畫面的旋轉造成的 rotation 問題
            Quaternion adjustedQuaternion = displayBaseRotation * Quaternion.AngleAxis(webCamTexture.videoRotationAngle, Vector3.back);
            display.transform.rotation = adjustedQuaternion;

            // 處理畫面因為鏡頭可能有 mirror 參數造成的鏡像問題
            Vector3 scale = Vector3.one;
            if (webCamTexture.videoVerticallyMirrored)
            {
                scale.y = -1;
            }

            display.transform.localScale = scale;
        }

    }

}
