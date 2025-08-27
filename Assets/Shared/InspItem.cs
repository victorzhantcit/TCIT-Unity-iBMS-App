using System.Collections.Generic;

#nullable enable 
namespace iBMSApp.Shared
{
    public class InspItem
    {
        public int Idx { get; set; } = 0;
        public string ItemName { get; set; } ="";
        //public string Status { get; set; } = "{\"0\": \"正常\",\"1\": \"異常\"}";
        public List<Radio> Status { get; set; } = new List<Radio>();
        public string Selected { get; set; } = ""; 
        public string Method { get; set; } = "";
        public bool Running { get; set; } = false;
        public string Reference { get; set; } = "";
        public string Note { get; set; } = "";
    }
}
