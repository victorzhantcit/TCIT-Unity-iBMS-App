using UnityEngine;
using UnityEngine.UI;

namespace iBMSApp.UI.Common
{
    public class VisualToggleImage : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private Image targetImage;
        [SerializeField] private Sprite onSprite;
        [SerializeField] private Sprite offSprite;

        private void Start()
        {
            // 初始化圖片
            UpdateSprite(toggle.isOn);

            // 訂閱事件
            toggle.onValueChanged.AddListener(UpdateSprite);
        }

        private void OnDestroy()
        {
            // 移除事件避免 memory leak
            toggle.onValueChanged.RemoveListener(UpdateSprite);
        }

        private void UpdateSprite(bool isOn)
        {
            targetImage.sprite = isOn ? onSprite : offSprite;
        }
    }
}
