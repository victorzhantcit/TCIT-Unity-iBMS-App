using System.Collections.Generic;

#nullable enable 
namespace iBMSApp.Shared
{
    public class TreeViewItem
    {
        public string Self { get; set; } = "";
        public List<TreeViewItem>? Children { get; set; }
    }
}
