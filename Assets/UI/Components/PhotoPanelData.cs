using iBMSApp.Shared;
using System;

namespace iBMSApp.UI.Components
{
    public class PhotoPanelData
    {
        public ImageFile ImageFileRef { get; set; }
        public Action RefreshUIEvent { get; set; }
    }
}

