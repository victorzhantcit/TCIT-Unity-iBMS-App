using System;

#nullable enable 
namespace iBMSApp.Shared
{
    public class EqptWorkOrder
    {
        public string? statusColor { get; set; }
        public string? statusBGColor { get; set; }
        /// <summary>
        /// 主索引鍵
        /// </summary>
        public int Sn { get; set; }
        /// <summary>
        /// 建築編號
        /// </summary>
        public string BuildingName { get; set; } = null!;
        public string BuildingCode { get; set; } = null!;
        /// <summary>
        /// 工單編號
        /// </summary>
        public string RecordSn { get; set; } = null!;
        /// <summary>
        /// 工單描述
        /// </summary>
        public string Description { get; set; } = null!;
        /// <summary>
        /// 處理人員類別,S:員工,F:廠商,Null:自填
        /// </summary>
        public string? ExecutorType { get; set; }
        /// <summary>
        /// 處理人員
        /// </summary>
        public string? Executor { get; set; }
        public string? ExecutorName { get; set; }
        /// <summary>
        /// 開立工單者
        /// </summary>
        public string Staff { get; set; } = "";
        /// <summary>
        /// 工單開單時間
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 工作起始期限
        /// </summary>
        public DateTime ScheduledStartDate { get; set; }
        /// <summary>
        /// 工作結束期限
        /// </summary>
        public DateTime ScheduledEndDate { get; set; }
        /// <summary>
        /// 管報系統編號
        /// </summary>
        public string? SklSn { get; set; }
        /// <summary>
        /// 優先等級,0:一般,1:優先,2:緊急
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// 狀態,Pending 待處理; Processing 處理中; Pause 暫結; Submitted 完工上傳; Approving 覆核中;Redistribute 另開工單; Completed 已完成;
        /// </summary>
        public string Status { get; set; } = null!;
        /// <summary>
        /// 現場掃碼開始作業時間
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
        /// 退件時間
        /// </summary>
        public DateTime? RejectTime { get; set; }
        /// <summary>
        /// 完成時間
        /// </summary>
        public DateTime? CompleteTime { get; set; }
        /// <summary>
        /// 花費工時(分鐘)
        /// </summary>
        public int ManMinute { get; set; }
        /// <summary>
        /// 工單處理前照片
        /// </summary>
        public string? PrePhotoSns { get; set; }
        /// <summary>
        /// 工單處理後照片
        /// </summary>
        public string? AfPhotoSns { get; set; }
    }
}
