using System;

#nullable enable 
namespace iBMSApp.Shared
{
    public class IbmsLog
    {
        /// <summary>
        /// 主索引鍵
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 使用者名稱
        /// </summary>
        public string? UserName { get; set; }
        /// <summary>
        /// 登入IP
        /// </summary>
        public string? IpAddress { get; set; }
        /// <summary>
        /// 類型(登入、下載、匯入...)
        /// </summary>
        public string? LogType { get; set; }
        /// <summary>
        /// Log等級：(Trace = 0、Debug = 1、Information = 2、Warning = 3、Error = 4、Critical = 5)
        /// </summary>
        public int? LogLevel { get; set; }
        /// <summary>
        /// 功能(呼叫的API名稱)
        /// </summary>
        public string? Function { get; set; }
        /// <summary>
        /// 詳細內容
        /// </summary>
        public string? LogInformation { get; set; }
        /// <summary>
        /// 紀錄時間
        /// </summary>
        public DateTime LogTime { get; set; }
    }
}
