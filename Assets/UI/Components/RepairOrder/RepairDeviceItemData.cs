using iBMSApp.Shared;
using System.Collections.Generic;

namespace iBMSApp.UI.Components
{
    public class RepairDeviceItemData
    {
        public EqptOrderDevice Device { get; set; }
        public List<ImageFile> PrePhotoList { get; set; }
        public List<ImageFile> AfPhotoList { get; set; }
    }
}
