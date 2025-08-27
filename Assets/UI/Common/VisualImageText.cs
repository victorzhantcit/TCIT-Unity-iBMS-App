using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace iBMSApp.UI.Common
{
    public class VisualImageText : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _label;

        public void Show()
        {
            if (_background != null)
                _background.gameObject.SetActive(true);
            if (_label != null)
                _label.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (_background != null)
                _background.gameObject.SetActive(false);
            if (_label != null)
                _label.gameObject.SetActive(false);
        }

        public void UpdateBackgroundColor(Color color)
        {
            if (_background != null)
                _background.color = color;
        }

        public void UpdateIcon(Sprite sprite)
        {
            if (_icon != null)
                _icon.sprite = sprite;
        }

        public void UpdateText(string text)
        {
            if (_label != null)
                _label.text = text;
        }
    }
}
