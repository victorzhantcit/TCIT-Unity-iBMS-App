#nullable enable 
namespace iBMSApp.Shared
{
    public partial class IbmsLineTopic
    {
        /// <summary>
        /// 主索引鍵
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 推播類型
        /// </summary>
        public string? Topic { get; set; }
        /// <summary>
        /// 推播類型註解
        /// </summary>
        public string? Note { get; set; }
        /// <summary>
        /// 推播群組
        /// </summary>
        public string? GroupIds { get; set; }
        /// <summary>
        /// 大樓代號
        /// </summary>
        public string? BuildingCode { get; set; }
        /// <summary>
        /// 語言, en_WW : 英文, zh_CN : 簡中, th_TH:泰文, zh_TW 或 Null : 繁中
        /// </summary>
        public string? Language { get; set; }
    }
}
