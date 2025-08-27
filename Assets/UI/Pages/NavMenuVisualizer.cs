using iBMSApp.Services;
using iBMSApp.Shared;
using iBMSApp.UI.Common;
using System.Collections.Generic;

namespace iBMSApp.UI.Pages
{
    public class NavMenuVisualizer : VisualGroupByEnum<UserRole>
    {
        /// <summary>
        /// 隱藏 base.Start()，因為這個 Visualizer 的生命週期跟隨 <seealso cref="NavMenu"/>
        /// </summary>
        private new void Start() { }

        /// <summary>
        /// 根據已儲存的角色顯示有權限的頁面跳轉標籤
        /// </summary>
        /// <param name="enabled"></param>
        public async void VisualNavMenuByRoles(bool enabled)
        {
            if (!enabled)
            {
                Hide();
                return;
            }

            if (ServiceManager.Instance == null) return;

            ILocalStorageService localStorageService = ServiceManager.Instance.LocalStorageService;

            List<UserRole> userRoles = await localStorageService.GetItemAsync<List<UserRole>>("roles");

            base.ActivateAll(false);

            if (userRoles == null || userRoles?.Count == 0) // anonymous
            {
                Show();
                return;
            }

            Hide();
            foreach (UserRole role in userRoles)
            {
                base.SetEnumValue(role, true, false);
            }

            Show();
        }

        public void Show() => base.ActivateBasicVisuals(true);

        public void Hide() => base.ActivateBasicVisuals(false);
    }
}
