#nullable enable 
namespace iBMSApp.Shared
{
    public partial class IbmsLineTokenGroup
    {
        /// <summary>
        /// Line Token群組ID
        /// </summary>
        public long GroupId { get; set; }
        /// <summary>
        /// 群組名稱
        /// </summary>
        public string GroupName { get; set; } = null!;
        /// <summary>
        /// 群組描述
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Line Token，[{&quot;name&quot;:&quot;XXX&quot;,&quot;token&quot;:&quot;YYY&quot;}]
        /// </summary>
        public string? Tokens { get; set; }
    }
}
