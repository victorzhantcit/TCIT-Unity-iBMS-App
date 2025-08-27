using TMPro;
using UnityEngine;

namespace iBMSApp.UI.Common
{
    public class TMPKeyValueText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _keyText;
        [SerializeField] private TMP_Text _valueText;

        public void ShowUpdated(string keyText, string valueText)
        {
            if (_keyText != null) 
                _keyText.text = keyText;
            if (_valueText != null) 
                _valueText.text = valueText;

            Show();
        }

        public void ShowUpdatedKey(string keyText)
        {
            if (_keyText != null) 
                _keyText.text = keyText;

            Show();
        }

        public void ShowUpdatedValue(string valueText)
        {
            string noneNullString = string.IsNullOrEmpty(valueText) ? "--" : valueText;

            if (_valueText != null) 
                _valueText.text = noneNullString;

            Show();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}
