using iBMSApp.Shared;
using System.Collections.Generic;

namespace iBMSApp.DataModels
{
    public class DeviceStaticStorage
    {
        public string Sn { get; set; }
        public List<DeviceStaticInfo> Devices;
    }
}

