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
            // ��l�ƹϤ�
            UpdateSprite(toggle.isOn);

            // �q�\�ƥ�
            toggle.onValueChanged.AddListener(UpdateSprite);
        }

        private void OnDestroy()
        {
            // �����ƥ��קK memory leak
            toggle.onValueChanged.RemoveListener(UpdateSprite);
        }

        private void UpdateSprite(bool isOn)
        {
            targetImage.sprite = isOn ? onSprite : offSprite;
        }
    }
}
