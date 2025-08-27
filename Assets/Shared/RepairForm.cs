#nullable enable 
namespace iBMSApp.Shared
{
    public class RepairForm
    {
        public string? statusColor1 { get; set; }
        //public string? appstatus { get; set; }
        public string? RecordSn { get; set; }
        public string? Status { get; set; }
       // public string? Name { get; set; }
        //public string? categoryChtName { get; set; }
        public string? Issue { get; set; }
        public string PhotoSns { get; set; } = "";
        public string? CreateTime { get; set; }
        public string? Issuer { get; set; }
        //public string? deviceNo { get; set; }
        //public string? deviceSN { get; set; }
        public string? DeviceCode { get; set; } = "";
        //public string completePhoto { get; set; } = "";
        public string CompleteTime { get; set; } = "";
        public string? Reply { get; set; }
        //public string repairType { get; set; } = "";
        public string DeviceDescription { get; set; } = "";
        //public string location { get; set; } = "";
    }
}
