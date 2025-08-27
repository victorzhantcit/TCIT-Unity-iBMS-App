using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace iBMSApp.UI.Components
{
    public class RadioToggleButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text _toggleName;

        [SerializeField] private Toggle _toggle;

        public Toggle Toggle => _toggle;

        public void UpdateToggleName(string toggleName)
        {
            _toggleName.text = toggleName;
        }
    }

}
