using System;

#nullable enable 
namespace iBMSApp.Shared
{
    public class EqptInspResult
    {
        //public int Id { get; set; }
        //public string BuildingCode { get; set; } = null!;
        //public string? RecordSn { get; set; }// = null!;
        public string DeviceCode { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string DeviceDescription { get; set; } = null!;
        //public string? Manufacturer { get; set; }
        //public string? ModelNumber { get; set; }
        //public DateTime? WarrantyDate { get; set; }
        //public int FormSn { get; set; }
        public string FormName { get; set; } = null!;
        //public string? StaffName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? PauseTime { get; set; }
        public DateTime? SubmitTime { get; set; }
        public int? ManMinute { get; set; }
        //public string? NumericalData { get; set; }// = null!;
        //public string? Consumables { get; set; }// = null!;
        //public string? Items { get; set; } // = null!;
        public string? PhotoSns { get; set; }
        //public string? Summarize { get; set; }
        public string? Result { get; set; }
        //public string? Location { get; set; }
    }
}
