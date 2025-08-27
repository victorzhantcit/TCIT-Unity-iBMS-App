using System;

#nullable enable 
namespace iBMSApp.Shared
{
    public class AllOrder
    {
        public string orderName { get; set; } = "";
        public string? statusColor { get; set; }
        public string? statusBGColor { get; set; }
        /// <summary>
        /// 巡檢單號/工單號
        /// </summary>
        public string RecordSn { get; set; } = null!;
        /// <summary>
        /// 表單型態，Plan：年度計畫，Single：臨時
        /// </summary>
        public string OrderType { get; set; } = null!;
        /// <summary>
        /// 巡檢項目
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// 檢查表文字串
        /// </summary>
        public string FormName { get; set; } = "";
        public string FormSn { get; set; } = "";
        /// <summary>
        /// 預設執行人員
        /// </summary>
        public string? Executor { get; set; }
        /// <summary>
        /// 預排日期
        /// </summary>
        public DateTime ScheduledDate { get; set; }
        /// <summary>
        /// 狀態,Pending 待處理; Processing 處理中; Pause 暫結; Submitted 完工上傳; Approving 覆核中;Changed 轉工單;Reject 退件; Completed 已完成;
        /// </summary>
        public string Status { get; set; } = null!;
        /// <summary>
        /// 掃描設備條碼開始巡檢時間
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 暫結時間
        /// </summary>
        public DateTime? PauseTime { get; set; }
        /// <summary>
        /// 完工上傳時間
        /// </summary>
        public DateTime? SubmitTime { get; set; }
        /// <summary>
        /// 審查退件時間
        /// </summary>
        public DateTime? RejectTime { get; set; }
        /// <summary>
        /// 完工時間
        /// </summary>
        public DateTime? CompleteTime { get; set; }
    }
}
