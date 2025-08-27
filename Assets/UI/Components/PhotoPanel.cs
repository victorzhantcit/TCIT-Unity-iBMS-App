using iBMSApp.App;
using iBMSApp.Shared;
using iBMSApp.Utility;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace iBMSApp.UI.Components
{
    public class PhotoPanel : MonoBehaviour, IObjectPoolItem<ImageFile>
    {
        [SerializeField] private RawImage _photoImage;
        [SerializeField] private Button _deletePhotoButton;

        private Action _deletePhotoEvent = null;
        private Action _refreshUIEvent = null;

        public RawImage PhotoImage => _photoImage;

        private void OnEnable()
        {
            StartCoroutine(AdjustHeightAfterFrame(5));
        }

        /// <inheritdoc/>
        public void Bind(ImageFile imageFileRef, bool isDisabled)
        {
            SetTexture(imageFileRef.base64data);
            SetInteractable(!isDisabled);
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
            ClearPhotoTexture();
        }

        public void BindEventAfterRawImageUpdated(Action refreshUI) => _refreshUIEvent = refreshUI;

        public void BindDeletePhotoEvent(Action action) => _deletePhotoEvent = action;

        public void OnDeletePhotoClicked() => _deletePhotoEvent?.Invoke();

        public void ClearPhotoTexture()
        {
            _photoImage.texture = null;
        }

        public async void SetTexture(string base64Photo)
        {
            Texture texture = await NativeCameraManager.Base64ToTexture(base64Photo);
            _photoImage.texture = texture;
        }

        public void SetInteractable(bool interactable)
        {
            _deletePhotoButton.gameObject.SetActive(interactable);
        }

        private IEnumerator AdjustHeightAfterFrame(int waitForFrames)
        {
            // 等待一幀，讓 LayoutGroup 套用 width
            int frameCount = 0;
            while (frameCount++ < waitForFrames)
            {
                //Debug.Log($"frame[{frameCount}] {_photoImage.rectTransform.rect.width}");
                yield return null;
            }

            Texture texture = _photoImage.texture;
            if (texture == null) yield break;

            float aspectRatio = (float)texture.width / texture.height;
            float photoWidth = _photoImage.rectTransform.rect.width;
            float adjustHeight = photoWidth / aspectRatio;
            Vector2 size = _photoImage.rectTransform.sizeDelta;

            size.y = adjustHeight;
            _photoImage.rectTransform.sizeDelta = size;
            _refreshUIEvent?.Invoke();
            //Debug.Log($"texture {texture.width}x{texture.height} ({aspectRatio}), photo {photoWidth}x{adjustHeight} ({photoWidth / adjustHeight})");
        }
    }
}
