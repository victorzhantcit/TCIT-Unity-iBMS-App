namespace iBMSApp.UI.Common
{
    public class VisualToggleSwapper : VisualGroupByEnum<ToggleState>
    {
        public void SetStatusToActivated() => base.SetEnumValue(ToggleState.Activated);

        public void SetStatusToDeactivated() => base.SetEnumValue(ToggleState.Deactivated);
    }
}

