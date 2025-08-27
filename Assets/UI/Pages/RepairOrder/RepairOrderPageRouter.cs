using iBMSApp.App;
using iBMSApp.UI.Layout;
using UnityEngine;
using UnityEngine.UI;

namespace iBMSApp.UI.Pages
{
    public class RepairOrderPageRouter : MonoBehaviour
    {
        [SerializeField] private MainLayout _orientationDetector;
        [SerializeField] private Toggle _navMenuButton;
        [SerializeField] private Toggle _scanToggle;
        [SerializeField] private Toggle _listToggle;
        [SerializeField] private BasePageMonoBehavior _scanPage;
        [SerializeField] private BasePageMonoBehavior _listPage;

        void Start()
        {
            UnregisterToggleEvents();

            switch (SceneController.BroadcastMessage)
            {
                case SceneBroadcastMessage.CallRepairPageScan:
                    ShowScanView();
                    break;
                case SceneBroadcastMessage.CallRepairPageOrderList:
                    ShowListView();
                    break;
                default: // �w�]�H Repair Order List ���ɦV
                    ShowListView();
                    break;
            }

            SceneController.NotifyBroadcastReceived();

            RegisterToggleEvents();
        }

        public async void ShowScanView(bool isToggled = true)
        {
            if (!isToggled)
                return;
            Debug.Log("ShowScanView called");
            if (!_scanToggle.isOn)
                _scanToggle.isOn = true;
            if (_listToggle.isOn)
                _listToggle.isOn = false;
            _scanPage.Show();
            _listPage.Hide();
            await _scanPage.RefreshPage();
            HideNavMenuWhenLandscapeMode();
        }

        public async void ShowListView(bool isToggled = true)
        {
            if (!isToggled)
                return;

            Debug.Log("ShowListView called");
            if (_scanToggle.isOn)
                _scanToggle.isOn = false;
            if (!_listToggle.isOn)
                _listToggle.isOn = true;
            _listPage.Show();
            _scanPage.Hide();
            await _listPage.RefreshPage();
            HideNavMenuWhenLandscapeMode();
        }

        private void UnregisterToggleEvents()
        {
            _scanToggle.onValueChanged.RemoveAllListeners();
            _listToggle.onValueChanged.RemoveAllListeners();
        }

        private void RegisterToggleEvents()
        {
            _scanToggle.onValueChanged.AddListener(ShowScanView);
            _listToggle.onValueChanged.AddListener(ShowListView);
        }

        /// <summary>
        /// ���禡�ηN�� <seealso cref="RepairScanView"/> ����� <see cref="RepairOrderView"/> ��<br></br>
        /// �����T���������� Highlight�A�åB���B�~Ĳ�o�w�]�� Toggle �ƥ�C
        /// </summary>
        public void SilenceToggleListPageTag()
        {
            UnregisterToggleEvents();
            _listToggle.isOn = true;
            _scanToggle.isOn = false;
            RegisterToggleEvents();
        }

        private void HideNavMenuWhenLandscapeMode()
        {
            if (_orientationDetector.IsPortraitMode && _navMenuButton.isOn)
            {
                _navMenuButton.isOn = false;
            }
        }
    }
}
