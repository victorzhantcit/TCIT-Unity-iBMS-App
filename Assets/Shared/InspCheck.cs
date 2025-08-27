using System;
using System.Collections.Generic;

#nullable enable 
namespace iBMSApp.Shared
{
    public class InspCheck
    {
        public string DeviceDescription { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string ModelNumber { get; set; } = "";
        public DateTime? WarrantyDate { get; set; }
        public string StaffName { get; set; } = "";
        public string FormSns { get; set; } = "";
        public string FormNames { get; set; } = "";
        public string Cycle { get; set; } = "";
        public int EstManMinute { get; set; }
        public DateTime? DataTime { get; set; }
        public List<InspItem> Items { get; set; } = new List<InspItem>();
        public List<InspNumericalData> NumericalData { get;set; }= new List<InspNumericalData>();
        public List<InspConsumables> Consumables { get; set; }= new List<InspConsumables>();
        public string Summarize { get; set; } = "";
        public string Result { get; set; } = "";
        public string PhotoSns { get; set; } = "";
    }
}
