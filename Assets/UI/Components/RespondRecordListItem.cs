using iBMSApp.Utility;
using TMPro;
using UnityEngine;

namespace iBMSApp.UI.Components
{
    public class RespondRecordListItem : MonoBehaviour, IObjectPoolItem<RespondRecordDto>
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _content;

        /// <inheritdoc/>
        public void Bind(RespondRecordDto display, bool isDisabled)
        {
            _title.text = display.Title;
            _content.text = display.Content;
        }

        /// <inheritdoc/>
        public void Show()
        {
            this.gameObject.SetActive(true);
        }

        /// <inheritdoc/>
        public void Hide()
        {
            this.gameObject.SetActive(false);
            _title.text = string.Empty;
            _content.text = string.Empty;
        }
    }

    public class RespondRecordDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }
}

