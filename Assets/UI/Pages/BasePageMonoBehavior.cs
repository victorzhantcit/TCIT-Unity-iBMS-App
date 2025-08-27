using iBMSApp.App;
using iBMSApp.Services;
using iBMSApp.UI.Common;
using System.Threading.Tasks;
using UnityEngine;

namespace iBMSApp.UI.Pages
{
    public abstract class BasePageMonoBehavior : MonoBehaviour, IPageDisplay
    {
        [Header("BasePage Config")]
        [SerializeField] private BasePageMonoBehavior _previousPage;
        [SerializeField] private BasePageMonoBehavior _childPage;

        public void Start()
        {
            if (ServiceManager.Instance == null)
            {
                GoToSceneIndex();
                return;
            }
        }

        public void GoToSceneIndex(bool triggered = true)
        {
            if (triggered) 
                SceneController.GoToSceneIndex();
        }

        public void GoToSceneLogin(bool triggered = true)
        {
            if (triggered)
                SceneController.GoToSceneLogin();
        }

        public void GoToSceneInspOrderList(bool triggered = true)
        {
            if (triggered)
                SceneController.GoToSceneInspOrderList();
        }

        public void GoToSceneWorkOrderList(bool triggered = true)
        {
            if (triggered)
                SceneController.GoToSceneWorkOrderList();
        }

        public void GoToSceneRepairPageOrderList(bool triggered = true)
        {
            if (triggered)
                SceneController.GoToSceneRepairPageOrderList();
        }   

        public void GoToSceneRepairPageScan(bool triggered = true)
        {
            if (triggered)
                SceneController.GoToSceneRepairPageScan();
        }

        public void GoToSceneChangePassword(bool triggered = true)
        {
            if (triggered)
                SceneController.GoToSceneChangePassword();
        } 

        /// <summary>
        /// 前往其他場景 詳細查看 Unity > [File] > [Build Profiles] > [Scene List]
        /// </summary>
        /// <param name="sceneName"></param>
        public void GoToScene(string sceneName)
        {
            SceneController.GoTo(sceneName);
        }

        public abstract void Show();

        public abstract void Hide();

        public abstract Task RefreshPage(PageRefreshParams refreshParams = null);

        public virtual async void GoToPageChild(PageRefreshParams refreshParams = null)
        {
            if (_childPage == null)
            {
                Debug.LogWarning("沒有可供導向的子頁面");
                return;
            }

            await _childPage.RefreshPage(refreshParams);
            _childPage.Show();
            Hide();
        }

        public virtual async void GoToPagePrevious()
        {
            if (_previousPage == null)
            {
                Debug.LogWarning("沒有可供導向的母頁面");
                return;
            }

            _previousPage.Show();
            Hide();
            await _previousPage.RefreshPage();
        }
    }
}