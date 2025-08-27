using System.Collections.Generic;

#nullable enable 
namespace iBMSApp.Shared
{
    public class TreeNode
    {
        public string Text { get; set; } = "";
        public string? Photo { get; set; }
        public string Source { get; set; } = "production";//production:production_photos,eqpt:eqpt_photos
        public string Status { get; set; } = ""; 
        public string Addition { get; set; } = "";
        public bool IsExpanded { get; set; }
        public List<TreeNode>? Children { get; set; }
    }
}
