using iBMSApp.Services;
using iBMSApp.Shared;
using TMPro;
using UnityEngine;

namespace iBMSApp.UI.Components
{
    public class TitleBar : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _userWelcomeLabel;

        private void Start()
        {
            
        }

        private void OnEnable()
        {
            UpdateUserWelcomeLabel();
        }

        public async void UpdateUserWelcomeLabel()
        {
            if (ServiceManager.Instance == null)
            {
                return;
            }

            ILocalStorageService localStorage = ServiceManager.Instance.LocalStorageService;
            AuthService authService = ServiceManager.Instance.AuthService;
            UserInfo userInfo = await authService.GetUserData();
            string localizedWelcomeString = Localizer.Instance.GetLocalizedString(",±z¦n");

            if (string.IsNullOrEmpty(userInfo.Name))
            {
                userInfo.Name = "LocalUser";
            }

            _userWelcomeLabel.text = $"{userInfo.Name} {localizedWelcomeString}";
        }
    }
}
