using iBMSApp.Services;
using iBMSApp.UI.Common;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace iBMSApp.UI.Pages
{
    public class ChangePasswordView : NetworkPageMonoBehavior<ChangePasswordView>
    {
        [Header("UI")]
        [SerializeField] private TMP_InputField _passwordOldInputField;
        [SerializeField] private TMP_InputField _passwordNewInputField;
        [SerializeField] private TMP_InputField _passwordConfirmInputField;
        [SerializeField] private VisualToggleSwapper _changePasswordButton;

        private bool _processingChange = false;

        private new void Start()
        {
            base.Start();
        }

        #region Override BaseClass
        /// <inheritdoc/>
        public override void Show()
        {
            this.gameObject.SetActive(true);
        }

        /// <inheritdoc/>
        public override void Hide()
        {
            this.gameObject.SetActive(false);
        }

        /// <inheritdoc/>
        public override Task RefreshPage(PageRefreshParams refreshParams = null)
        {
            _passwordOldInputField.text = "";
            _passwordNewInputField.text = "";
            _passwordConfirmInputField.text = "";
            SetChangePasswordProcessing(false);

            return Task.CompletedTask;
        }
        #endregion

        public void OnChangePasswordClicked() => _ = ChangePassWord();

        private async Task ChangePassWord()
        {
            if (IsChangeProcessing()) return;
            SetChangePasswordProcessing(true);

            string OLDPW = _passwordOldInputField.text;
            string PW1 = _passwordNewInputField.text;
            string PW2 = _passwordConfirmInputField.text;

            var response = await _authService.ChangePassword(OLDPW, PW1, PW2);

            if (!response.IsSuccess)
            {
                base.HandleHttpRequestException(response);
                SetChangePasswordProcessing(false);
                return;
            }
            // retryCnt = 0;

            var result = response?.Data;

            if (result == null)
            {
                _toastService.ShowToast(response?.ErrorMessage ?? "", ToastLevel.Warning);
                SetChangePasswordProcessing(false);
                return;
            }

            if (result.Equals("OK"))
            {
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG040"), ToastLevel.Success); //修改成功！
                OLDPW = "";
                PW1 = "";
                PW2 = "";
            }
            else if (result.Equals("Wrong"))
            {
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG047"), ToastLevel.Warning); //密碼錯誤！
                OLDPW = "";;
            }
            else if (result.Equals("Again"))
            {
                _toastService.ShowToast(Localizer.Instance.GetLocalizedString("MSG048"), ToastLevel.Warning); //請重新輸入新密碼！
                PW1 = "";
                PW2 = "";
            }
            else
            {
                _toastService.ShowToast("Uncatched server status! " + response.ErrorMessage ?? "", ToastLevel.Warning);
            }

            _passwordOldInputField.text = OLDPW;
            _passwordNewInputField.text = PW1;
            _passwordConfirmInputField.text = PW2;
            SetChangePasswordProcessing(false);
        }

        private void SetChangePasswordProcessing(bool isProcessing)
        {
            _changePasswordButton.SetEnumValue(isProcessing ? ToggleState.Deactivated : ToggleState.Activated);
            _processingChange = isProcessing;
        }

        private bool IsChangeProcessing() => _processingChange == true;
    }
}
