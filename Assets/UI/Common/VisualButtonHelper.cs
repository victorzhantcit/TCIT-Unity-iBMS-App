using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace iBMSApp.UI.Common
{
    [RequireComponent(typeof(Button))]
    public class VisualButtonHelper : VisualImageText
    {
        [Header("VisualToggleSwapper take over control by this component, \nset default display to Deactivated, and loading display to Activated")]
        [SerializeField] private VisualToggleSwapper _contentDisplaySwapper;

        private Button _button;

        private Action _onClickAction = null;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            ShowContentDefault();
        }

        /// <summary>
        /// �u���\�@���I���ƥ�A�|�������ª��A�]�s���C
        /// </summary>
        public void BindOnClickedEvent(Action action)
        {
            _onClickAction = action;
        }

        public void SetButtonInteractable(bool interactable)
        {
            _button.interactable = interactable;
        }

        public void ShowContentActivated()
        {
            if (_contentDisplaySwapper != null)
                _contentDisplaySwapper.SetEnumValue(ToggleState.Activated);
        }

        public void ShowContentDefault()
        {
            if (_contentDisplaySwapper != null)
                _contentDisplaySwapper.SetEnumValue(ToggleState.Deactivated);
        }

        public void OnButtonClicked()
        {
            ShowContentActivated();
            _onClickAction?.Invoke();
        }

        public new void Show()
        {
            gameObject.SetActive(true);
        }

        public new void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
