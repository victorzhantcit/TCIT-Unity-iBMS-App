using iBMSApp.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace iBMSApp.UI.Components
{
    public class PageTitlePanel : MonoBehaviour
    {

        [SerializeField] private Toggle _networkReachablilityIcon;
        [SerializeField] private TMP_Text _pageTitle;

        public string NetworkUnreachableText => _networkUnreachableText;

        private string _networkUnreachableText;

        public void SetNetworkUnreachableText(string text)
        {
            _networkUnreachableText = text;
        }

        public void SetPageTitle(string title)
        {
            if (_pageTitle != null)
            {
                _pageTitle.text = title;
            }
        }

        public void SetNetworkIcon(bool isOnline)
        {
            if (_networkReachablilityIcon != null)
            {
                _networkReachablilityIcon.isOn = isOnline;
            }
        }
    }
}
