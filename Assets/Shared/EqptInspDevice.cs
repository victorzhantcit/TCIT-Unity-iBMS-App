#nullable enable 
namespace iBMSApp.Shared
{
    public class EqptInspDevice
    {
        public int Id { get; set; }
        public int FormSn { get; set; }
        public string BuildingCode { get; set; } = null!;
        public string System { get; set; } = null!;
        public string DeviceType { get; set; } = null!;
        public string DeviceCode { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
