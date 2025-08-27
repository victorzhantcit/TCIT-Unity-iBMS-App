#nullable enable 
namespace iBMSApp.Shared
{
    public class InspConsumables
    {
        public string Name { get; set; } = "";
        public string AvailableNum { get; set; } = "";
        public string AvailableUnit { get; set; } = "";
        public string ReplaceDate { get; set; } = "";
        public string ExpiryDate { get; set; } = "";
        public bool isChecked { get; set; } = false;
    }
}
