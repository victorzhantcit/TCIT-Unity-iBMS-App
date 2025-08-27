using UnityEngine.SceneManagement;
using iBMSApp.Shared;
using iBMSApp.UI.Pages;

namespace iBMSApp.App
{
    public static class SceneController
    {
        public static SceneBroadcastMessage BroadcastMessage { get; private set; } = SceneBroadcastMessage.None;

        public static UserRole ValidatedRole { get; set; } = UserRole.Undefined;

        private static string IndexSceneName = "Index";

        private static string LoginSceneName = "Login";

        private static string InspOrderListSceneName = "InspOrderList";

        private static string WorkOrderListSceneName = "WorkOrderList";

        private static string RepairOrderListSceneName = "RepairOrderList";

        private static string ChangePasswordSceneName = "ChangePassword";

        public static void GoTo(string sceneName, SceneBroadcastMessage broadcastMessage = SceneBroadcastMessage.None)
        {
            BroadcastMessage = broadcastMessage;
            if (LoadingPage.Instance != null)
            {
                // 直接由 Singleton 啟動 Coroutine（不再產生 Runner）
                LoadingPage.Instance.StartSceneLoading(sceneName);
            }
            else
            {
                // 如果 Singleton 還沒準備好，fallback 為立即切場景
                SceneManager.LoadScene(sceneName);
            }
        }

        public static void GoToSceneIndex() => GoTo(IndexSceneName);

        public static void GoToSceneLogin() => GoTo(LoginSceneName);

        public static void GoToSceneInspOrderList() => GoTo(InspOrderListSceneName);

        public static void GoToSceneWorkOrderList() => GoTo(WorkOrderListSceneName);

        public static void GoToSceneRepairPageOrderList() => GoTo(RepairOrderListSceneName, SceneBroadcastMessage.CallRepairPageOrderList);

        public static void GoToSceneRepairPageScan() => GoTo(RepairOrderListSceneName, SceneBroadcastMessage.CallRepairPageScan);

        public static void GoToSceneChangePassword() => GoTo(ChangePasswordSceneName);

        public static void NotifyBroadcastReceived() => BroadcastMessage = SceneBroadcastMessage.None;
    }

    public enum SceneBroadcastMessage
    {
        None,
        CallRepairPageOrderList,
        CallRepairPageScan
    }
}
