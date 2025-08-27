using TMPro;
using UnityEngine;

namespace iBMSApp.UI.Common
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizeStringEventHelper : MonoBehaviour
    {
        [SerializeField] private TMP_Text _tmpText;
        [SerializeField] private string _prefix;
        [SerializeField] private string _suffix;

        public void UpdateLocalizedText(string localizedResult)
        {
            _tmpText.text = _prefix + localizedResult + _suffix;
        }
    }
}
