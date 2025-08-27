using iBMSApp.Services;
using iBMSApp.Shared;
using iBMSApp.UI.Common;
using iBMSApp.Utility;
using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace iBMSApp.UI.Components
{
    public class RepairOrderListItem : MonoBehaviour, IObjectPoolItem<EqptRepairOrder>
    {
        private static readonly int ValueIndent = 140;

        private static readonly LocalizedStyle DefaultStyle =
            LocalizedStyle.LabelLocalized | LocalizedStyle.LabelColored | LocalizedStyle.ValueColored | LocalizedStyle.Indent;

        private static readonly LocalizedStyleParams DefaultStyleParams = new LocalizedStyleParams
        {
            LabelColor = "#A2A2A2",
            ValueColor = "#707070",
            Indent = ValueIndent
        };

        [Header("UI")]
        [SerializeField] private TMP_Text _recordSnText;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private Image _statusBackground;
        [SerializeField] private TMP_Text _overviewText;
        [SerializeField] private VisualButtonHelper _enterOrderButton;

        private static string MakeLocalizedInfoRichText(string localizedLabelKey, string value)
        {
            return Localizer.GenerateLocalizedRichText(
                localizedLabelKey,
                value,
                DefaultStyle,
                DefaultStyleParams
            );
        }

        public void Bind(EqptRepairOrder order, bool _ = false)
        {
            Color backgroundColor = ColorMapper.Instance.GetColor(order.statusBGColor);
            StringBuilder sb = new StringBuilder();
            bool isNotDevice = order.DeviceType.Equals("Other") || order.DeviceType.Equals("");
            string descriptionLabelLocalizedKey = isNotDevice ? "T_Location" : "T_Device";

            _recordSnText.text = order.RecordSn;
            _recordSnText.color = backgroundColor;
            _statusText.text = Localizer.Instance.GetLocalizedString($"S_{order.Status}");
            _statusBackground.color = backgroundColor;

            sb.AppendLine(MakeLocalizedInfoRichText("T_Building", order.BuildingName));
            sb.AppendLine(MakeLocalizedInfoRichText(descriptionLabelLocalizedKey, order.DeviceDescription));
            sb.AppendLine(MakeLocalizedInfoRichText("³ø­×¤H", order.IssuerName));
            sb.AppendLine(MakeLocalizedInfoRichText("T_Content", order.Issue));
            if (order.CreateTime != null)
                sb.AppendLine(MakeLocalizedInfoRichText("T_IssueTime", order.CreateTime.Value.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture)));
            if (order.CompleteTime != null)
                sb.AppendLine(MakeLocalizedInfoRichText("T_ProcessingTime", order.CompleteTime.Value.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture)));
            else if (order.ScheduledDate != null)
                sb.AppendLine(MakeLocalizedInfoRichText("T_ScheduledTime", order.ScheduledDate.Value.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture)));
            _overviewText.text = sb.ToString();
            sb.Clear();

            _enterOrderButton.UpdateBackgroundColor(backgroundColor);
        }

        public void BindEnterOrderEvent(bool isIssuerAndNotStart, Action editAction, Action viewAction)
        {
            if (isIssuerAndNotStart)
            {
                _enterOrderButton.UpdateText(Localizer.Instance.GetLocalizedString("BTN023"));
                _enterOrderButton.BindOnClickedEvent(editAction); 
            }
            else
            {
                _enterOrderButton.UpdateText(Localizer.Instance.GetLocalizedString("BTN006"));
                _enterOrderButton.BindOnClickedEvent(viewAction);
            }
        }

        public void Show() => this.gameObject.SetActive(true);

        public void Hide() => this.gameObject.SetActive(false);
    }
}
