using iBMSApp.Services;
using iBMSApp.UI.Common;
using System.Threading.Tasks;
using UnityEngine;

namespace iBMSApp.UI.Pages
{
    public class NavMenu : BasePageMonoBehavior
    {
        [Header("UI")]
        [SerializeField] private NavMenuVisualizer _visualizer;

        #region Override BaseClass
        public override void Hide()
        {
            _visualizer.Hide();
        }

        public override Task RefreshPage(PageRefreshParams refreshParams = null)
        {
            throw new System.NotImplementedException();
        }

        public override void Show()
        {
            _visualizer.Show();
        }
        #endregion

        public void ToggleNavMenu(bool enabled)
        {
            _visualizer.VisualNavMenuByRoles(enabled);
        }

        public void LogoutUser() => PrepareLogout();

        private async void PrepareLogout()
        {
            // �n�X�᭶���i��w�g���s�b �浹�I���B�z
            IAuthService authService = ServiceManager.Instance.AuthService;

            await authService.LogoutAsync();
            base.GoToSceneIndex();
        }
    }
}
