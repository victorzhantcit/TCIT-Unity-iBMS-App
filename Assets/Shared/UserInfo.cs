using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable enable 
namespace iBMSApp.Shared
{
    public class UserInfo
    {
        [Required(ErrorMessage = "請輸入ID")]
        public string Id { get; set; } = null!;

        [Required(ErrorMessage = "請輸入密碼")]
        public string Password { get; set; } = null!; 
        public string Name { get; set; } = "";
        public string Department { get; set; } = "";
        public string Tel { get; set; } = "";
        public string Email { get; set; } = "";

        public List<string> Roles = new List<string>();

        public List<string> GroupSids = new List<string>();
    }

    public enum UserRole
    {
        Staff,
        QC,
        Insp,
        Maint,
        DeviceMaint,
        Security,
        Undefined
    }
}
