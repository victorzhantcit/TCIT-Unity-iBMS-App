using iBMSApp.Services;
using iBMSApp.Shared;
using iBMSApp.Utility;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace iBMSApp.UI.Components
{
    public class CheckItemPanel : MonoBehaviour
    {
        private static readonly LocalizedStyle BothLocalizedStyle =
            LocalizedStyle.LabelLocalized | LocalizedStyle.ValueLocalized | LocalizedStyle.Comma;

        private static readonly LocalizedStyle LabelLocalizedStyle =
            LocalizedStyle.LabelLocalized | LocalizedStyle.Comma;

        [SerializeField] private TMP_Text _checkIndex;
        [SerializeField] private TMP_Text _checkInfo;
        [SerializeField] private TMP_Text _resultTitle;
        [SerializeField] private ToggleGroup _resultOptionGroup;
        [SerializeField] private RectTransform _resultOptionContent;
        [SerializeField] private RadioToggleButton _resultOptionPrefab;
        [SerializeField] private TMP_Text _remarkTitle;
        [SerializeField] private TMP_InputField _remarkInputField;

        private ObjectPool<RadioToggleButton> _resultOptionPool;
        private EqptOrderItem _checkItem;
        private bool _isDisabled;

        private static string MakeLabelLocalizedInfo(string key, string value)
        {
            return Localizer.GenerateLocalizedRichText(
                key,
                value,
                LabelLocalizedStyle
            );
        }

        private static string MakeBothLocalizedInfo(string key, string value)
        {
            return Localizer.GenerateLocalizedRichText(
                key,
                value,
                BothLocalizedStyle
            );
        }

        private void Awake()
        {
            _resultOptionPool = new ObjectPool<RadioToggleButton>(_resultOptionPrefab, _resultOptionContent);
        }

        public void Bind(EqptOrderItem checkItem, bool isDisabled, int checkIndex)
        {
            StringBuilder sb = new StringBuilder();
            bool isItemRunning = !checkItem.Running.Equals("") && bool.Parse(checkItem.Running);
            string runningStatus = isItemRunning ? "C_Running" : "C_Stop";

            _isDisabled = isDisabled;
            _checkItem = checkItem;
            _checkIndex.text = $"<b>{checkIndex.ToString("00")}.</b>";
            sb.AppendLine($"<b>{checkItem.ItemName}</b>");
            sb.AppendLine(MakeBothLocalizedInfo("T_Method", checkItem.Method));
            sb.AppendLine(MakeBothLocalizedInfo("T_RunStatus", runningStatus));
            if(checkItem.Reference != "")
                sb.AppendLine(MakeLabelLocalizedInfo("T_Reference", checkItem.Reference));
            _checkInfo.text = sb.ToString();
            sb.Clear();

            RefreshResultOptions();

            _remarkTitle.gameObject.SetActive(!isDisabled);
            _remarkTitle.text = MakeLabelLocalizedInfo("T_Remark", "");
            _remarkInputField.gameObject.SetActive(!isDisabled);
            _remarkInputField.onEndEdit.RemoveAllListeners();
            if (!isDisabled)
            {
                _remarkInputField.text = checkItem.Note;
                _remarkInputField.onEndEdit.AddListener(textResult => checkItem.Note = textResult);
            }
            else
            {
                if (!string.IsNullOrEmpty(checkItem.Note))
                    _remarkTitle.text += checkItem.Note;
            }
        }

        private void ClearResultOptions()
        {
            //if (_resultOptionContent == null)
            //    _resultOptionContent = _resultOptionGroup.GetComponent<RectTransform>();
            foreach (Transform item in _resultOptionContent)
            {
                if (item.gameObject.activeSelf == false)
                {
                    return;
                }
                if (item.TryGetComponent<RadioToggleButton>(out var itemComponent))
                {
                    //itemComponent.Toggle.isOn = false;
                    _resultOptionPool.Release(itemComponent);
                }
            }
            Canvas.ForceUpdateCanvases();
        }

        public void RefreshResultOptions()
        {
            ClearResultOptions();
            int siblingIndex = 0;
            _resultTitle.gameObject.SetActive(_checkItem.Status.Count > 0);
            if (_resultOptionPool == null)
            {
                _resultOptionPool = new ObjectPool<RadioToggleButton>(_resultOptionPrefab, _resultOptionContent);

            }
            foreach (var option in _checkItem.Status)
            {
                string optionValue = option.Value;
                string displayValue = Localizer.Instance.GetLocalizedString(optionValue);

                RadioToggleButton optionButton = _resultOptionPool.Get();
                
                BindResultOption(optionButton.Toggle, option.Key);
                optionButton.UpdateToggleName(displayValue);
                optionButton.transform.SetSiblingIndex(siblingIndex);
                siblingIndex++;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_resultOptionContent);
            Canvas.ForceUpdateCanvases();
        }

        private void BindResultOption(Toggle toggle, string toggleId)
        {
            toggle.group = _resultOptionGroup;
            toggle.onValueChanged.RemoveAllListeners();
            toggle.isOn = _checkItem.Selected == toggleId;
            toggle.interactable = !_isDisabled;
            toggle.onValueChanged.AddListener((newValue) => OnResultValueChanged(newValue, toggleId));
        }

        private void OnResultValueChanged(bool newValue, string toggleId)
        {
            if (newValue)
                _checkItem.Selected = toggleId;
        }
    }
}
