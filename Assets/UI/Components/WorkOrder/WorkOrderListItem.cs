using iBMSApp.Services;
using iBMSApp.Shared;
using iBMSApp.UI.Common;
using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace iBMSApp.UI.Components
{
    public class WorkOrderListItem : MonoBehaviour
    {
        private static readonly int ValueIndent = 110;

        private static readonly LocalizedStyle DefaultStyle =
            LocalizedStyle.LabelLocalized | LocalizedStyle.LabelColored | LocalizedStyle.ValueColored | LocalizedStyle.Indent;

        private static readonly LocalizedStyle PriorityStyle =
            LocalizedStyle.LabelLocalized | LocalizedStyle.ValueLocalized 
            | LocalizedStyle.LabelColored | LocalizedStyle.ValueColored | LocalizedStyle.Indent;

        private static readonly LocalizedStyleParams DefaultStyleParams = new LocalizedStyleParams
        {
            LabelColor = "#A2A2A2",
            ValueColor = "#707070",
            Indent = ValueIndent
        };

        private static readonly LocalizedStyleParams PriorityStyleParams = new LocalizedStyleParams
        {
            LabelColor = "#A2A2A2",
            ValueColor = "red",
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

        private static string MakePriorityString(string localizedLabelKey, string localizedValueKey)
        {
            return Localizer.GenerateLocalizedRichText(
                localizedLabelKey,
                localizedValueKey,
                PriorityStyle,
                PriorityStyleParams
            );
        }

        public void UpdateListItem(EqptOrder order)
        {
            Color backgroundColor = ColorMapper.Instance.GetColor(order.StatusBGColor);
            StringBuilder sb = new StringBuilder();

            _recordSnText.text = order.RecordSn;
            _recordSnText.color = backgroundColor;
            _statusText.text = Localizer.Instance.GetLocalizedString($"S_{order.Status}");
            _statusBackground.color = backgroundColor;

            sb.AppendLine(MakeLocalizedInfoRichText("T_Building", order.BuildingName));
            sb.AppendLine(MakeLocalizedInfoRichText("T_Description", order.Description));

            if (!string.IsNullOrEmpty(order.StaffName))
                sb.AppendLine(MakeLocalizedInfoRichText("T_WorkBy", order.StaffName));
            else if (!string.IsNullOrEmpty(order.ExecutorName))
                sb.AppendLine(MakeLocalizedInfoRichText("預設處理人員", order.ExecutorName));

            if (order.Priority == 2)
                sb.AppendLine(MakePriorityString("T_Level", "C_Urgent"));
            else if (@order.Priority == 1)
                sb.AppendLine(MakePriorityString("T_Level", "C_Critical"));

            sb.AppendLine(MakeLocalizedInfoRichText("T_ExecutionTime", order.ScheduledDate));

            if (order.CompleteTime != null)
                sb.AppendLine(MakeLocalizedInfoRichText("T_CompleteTime", order.CompleteTime));
            else if (order.RejectTime != null)
                sb.AppendLine(MakeLocalizedInfoRichText("T_RejectTime", order.RejectTime));
            else if (order.SubmitTime != null)
                sb.AppendLine(MakeLocalizedInfoRichText("BTN012", order.SubmitTime));
            else if (order.StartTime != null)
                sb.AppendLine(MakeLocalizedInfoRichText("T_StartingTime", order.StartTime));

            _overviewText.text = sb.ToString();
            sb.Clear();

            bool isCompleted = order.CompleteTime != null || order.SubmitTime != null;
            string buttonActionKey = isCompleted ? "BTN006" : "BTN007";

            _enterOrderButton.UpdateText(Localizer.Instance.GetLocalizedString(buttonActionKey));
            _enterOrderButton.UpdateBackgroundColor(backgroundColor);
        }

        public void BindEnterOrderEvent(Action action) => _enterOrderButton.BindOnClickedEvent(action);

        public void Show() => this.gameObject.SetActive(true);

        public void Hide() => this.gameObject.SetActive(false);
    }
}
