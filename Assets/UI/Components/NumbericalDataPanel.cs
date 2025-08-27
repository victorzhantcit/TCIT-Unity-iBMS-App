using iBMSApp.Shared;
using TMPro;
using UnityEngine;

namespace iBMSApp.UI.Components
{
    public class NumbericalDataPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_InputField _valueInputField;

        private EqptOrderDevNumericalData _numericalData = null;

        public void Bind(EqptOrderDevNumericalData numericalData, bool isDisabled)
        {
            _valueInputField.onEndEdit.RemoveAllListeners();

            _numericalData = numericalData;

            _descriptionText.text = $"{numericalData.TagDescription} ({numericalData.TagUnit})";
            _valueInputField.text = numericalData.Value;
            _valueInputField.interactable = !isDisabled;

            // 綁定值變更事件
            _valueInputField.onEndEdit.AddListener(OnEndEdit);
        }

        private void OnEndEdit(string newValue)
        {
            if (_numericalData == null)
            {
                Debug.LogWarning("Data binding is null on NumbericalDataPanel!");
                return;
            }
            _numericalData.Value = newValue; // 回寫資料
        }
    }
}
