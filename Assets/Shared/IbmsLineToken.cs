#nullable enable 
namespace iBMSApp.Shared
{
    public partial class IbmsLineToken
    {
        /// <summary>
        /// 主索引鍵
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Line群組Token
        /// </summary>
        public string? Token { get; set; }
        /// <summary>
        /// 案場編號
        /// </summary>
        public string? CodeName { get; set; }
        /// <summary>
        /// 群組編號
        /// </summary>
        public string? GroupId { get; set; }
        /// <summary>
        /// 群組名稱
        /// </summary>
        public string? GroupName { get; set; }
        /// <summary>
        /// 群組備註
        /// </summary>
        public string? Description { get; set; }
    }
}
