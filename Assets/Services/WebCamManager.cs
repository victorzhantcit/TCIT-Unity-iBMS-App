using System;
using System.Collections;
using UnityEngine;


#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace iBMSApp.Services
{
    public class WebCamManager : MonoBehaviour
    {
        #region Unity WebCam code example
#if UNITY_IOS || UNITY_WEBGL
    private bool CheckPermissionAndRaiseCallbackIfGranted(UserAuthorization authenticationType, Action authenticationGrantedAction)
    {
        if (Application.HasUserAuthorization(authenticationType))
        {
            if (authenticationGrantedAction != null)
                authenticationGrantedAction();

            return true;
        }
        return false;
    }

    private IEnumerator AskForPermissionIfRequired(UserAuthorization authenticationType, Action authenticationGrantedAction)
    {
        if (!CheckPermissionAndRaiseCallbackIfGranted(authenticationType, authenticationGrantedAction))
        {
            yield return Application.RequestUserAuthorization(authenticationType);
            if (!CheckPermissionAndRaiseCallbackIfGranted(authenticationType, authenticationGrantedAction))
                Debug.LogWarning($"Permission {authenticationType} Denied");
        }
    }
#elif UNITY_ANDROID
        private void PermissionCallbacksPermissionGranted(string permissionName)
        {
            StartCoroutine(DelayedCameraInitialization());
        }

        private IEnumerator DelayedCameraInitialization()
        {
            yield return null;
            InitializeCamera();
        }

        private void PermissionCallbacksPermissionDenied(string permissionName)
        {
            Debug.LogWarning($"Permission {permissionName} Denied");
        }

        private void AskCameraPermission()
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += PermissionCallbacksPermissionDenied;
            callbacks.PermissionGranted += PermissionCallbacksPermissionGranted;
            Permission.RequestUserPermission(Permission.Camera, callbacks);
        }
#endif

        void Start()
        {
            // Unity Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

#if UNITY_IOS || UNITY_WEBGL
            StartCoroutine(AskForPermissionIfRequired(UserAuthorization.WebCam, () => { InitializeCamera(); }));
            return;
#elif UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                AskCameraPermission();
                return;
            }
#endif
#pragma warning disable CS0162 // Unreachable code detected
            InitializeCamera();
#pragma warning restore CS0162 // Unreachable code detected
        }
        #endregion
        [SerializeField] private int _cameraIndex = 0;
        [SerializeField] private int _cameraTextureWidth = 640;
        [SerializeField] private int _cameraTextureHeight = 640;

        public static WebCamManager Instance { get; private set; }
        public WebCamTexture WebcamTexture { get; private set; } = null;
        private bool _isInitialized = false;

        public bool IsInitialized => _isInitialized;

        public bool IsPlaying() => _isInitialized == true && WebcamTexture != null && WebcamTexture.isPlaying;

        private bool InitializeCamera()
        {
            _isInitialized = false;

            WebCamDevice[] devices = WebCamTexture.devices;

            for (int i = 0; i < devices.Length; i++)
            {
                Debug.Log($"Camera {i}: {devices[i].name}");
            } 

            if (devices.Length > 0)
            {
                string camName = devices[_cameraIndex].name;
                //Resolution[] resolution = devices[_cameraIndex].availableResolutions;

                //if (resolution.Length == 0)
                //{
                //    Debug.LogWarning($"No available resolutions for camera: {camName}");
                //    return false;
                //}

                //Resolution selectedResolution = resolution[0];

                //Debug.Log(string.Join(", ", resolution.Select(r => $"{r.width}x{r.height}")));
                WebcamTexture = new WebCamTexture(camName, _cameraTextureWidth, _cameraTextureHeight);

                _isInitialized = true;
            }

            return _isInitialized;
        }

        public bool RefreshCameraResolution(int width, int height, int cameraIndex = 0)
        {
            _cameraTextureWidth = width;
            _cameraTextureHeight = height;

            if (!_isInitialized)
                return false;

            return InitializeCamera();
        }

        public WebCamTexture StartCamera()
        {
            if (IsPlaying()) 
                return null;

            try
            {
                WebcamTexture.Play();
                return WebcamTexture;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        public void StopCamera()
        {
            if (!IsPlaying())
                return;

            WebcamTexture.Stop();
        }
    }

}
