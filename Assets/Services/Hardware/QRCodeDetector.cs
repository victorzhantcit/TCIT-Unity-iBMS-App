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
            // �إ� ZXing �ѽX��
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
                    Debug.Log("���y�� QRCode: " + result.Text);
                    QRCodeScanned?.Invoke(result.Text);
                }
                Debug.Log("���ձ��yQR...");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("QRCode �ѽX����: " + ex.Message);
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

            // �D�P�B���� webcamTexture �ѪR�״N��
            await WaitForWebcamResolution(webcamTexture);

            // �M�� AspectRatioFitter
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
                    return true; // �w�ǳƦn
                }

                await Task.Yield(); // ���@�V
            }

            // �W�L���ݦ��Ƥ����N��
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

            // �B�z�e��������y���� rotation ���D
            Quaternion adjustedQuaternion = displayBaseRotation * Quaternion.AngleAxis(webCamTexture.videoRotationAngle, Vector3.back);
            display.transform.rotation = adjustedQuaternion;

            // �B�z�e���]�����Y�i�঳ mirror �ѼƳy�����蹳���D
            Vector3 scale = Vector3.one;
            if (webCamTexture.videoVerticallyMirrored)
            {
                scale.y = -1;
            }

            display.transform.localScale = scale;
        }

    }

}
