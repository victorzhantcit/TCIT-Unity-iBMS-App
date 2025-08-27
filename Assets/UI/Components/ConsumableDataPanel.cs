using iBMSApp.Services;
using iBMSApp.Shared;
using iBMSApp.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace iBMSApp.UI.Components
{
    public class ConsumableDataPanel : MonoBehaviour, IObjectPoolItem<EqptOrderDevConsumable>
    {
        [SerializeField] private Toggle _checkBox;
        [SerializeField] private TMP_Text _consumableName;
        [SerializeField] private float _paddingNameWhenEnabled = 30f;

        private EqptOrderDevConsumable _consumableData;

        /// <inheritdoc/>
        public void Bind(EqptOrderDevConsumable consumableData, bool isDisabled)
        {
            _checkBox.onValueChanged.RemoveAllListeners();

            _consumableData = consumableData;
            _checkBox.isOn = consumableData.isChecked;
            _checkBox.gameObject.SetActive(!isDisabled);
            _consumableName.margin = new Vector4(isDisabled ? 0 : _paddingNameWhenEnabled, 0, 0, 0);
            RefreshDate();

            _checkBox.onValueChanged.AddListener(OnValueChanged);
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
        }

        private void OnValueChanged(bool newValue)
        {
            if (_consumableData == null)
            {
                Debug.LogWarning("Data binding is null on NumbericalDataPanel!");
                return;
            }
            _consumableData.isChecked = newValue; // ¦^¼g¸ê®Æ
        }

        public void RefreshDate()
        {
            if (_consumableData == null)
            {
                Debug.LogWarning("Data binding is null on NumbericalDataPanel!");
                return;
            }

            string replaceDate = (_consumableData.ReplaceDate == "")
                ? Localizer.Instance.GetLocalizedString("C_NoChange")
                : _consumableData.ReplaceDate.Substring(0, 16).Replace('T', ' ');

            _consumableName.text = Localizer.GenerateLocalizedRichText(
                _consumableData.Name,
                replaceDate,
                LocalizedStyle.Comma
            );
        }
    }
}
