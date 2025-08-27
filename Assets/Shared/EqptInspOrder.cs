using System;
using System.Collections.Generic;

#nullable enable 
namespace iBMSApp.Shared
{
    public class EqptInspOrder
    {
        /// <summary>
        /// UI 清單中單項文字顏色，對應CSS名稱，區分待處理、處理中、已處理
        /// </summary>
        public string? StatusColor { get; set; }
        /// <summary>
        /// UI 清單中單項背景、按鍵底色，對應CSS名稱，區分待處理、處理中、已處理
        /// </summary>
        public string? StatusBGColor { get; set; }
        /// <summary>
        /// 表單狀態，0:待處理，1:處理中，2:已處理
        /// </summary>
        public int StatusType { get; set; }
        /// <summary>
        /// 經過狀態篩選，是否在 UI 清單中呈現
        /// </summary>
        public bool Show { get; set; } = false;
        /// <summary>
        /// 主索引鍵
        /// </summary>
        public string Sn { get; set; } = null!;
        /// <summary>
        /// 建築/廠區/路段名稱
        /// </summary>
        public string? BuildingName { get; set; }
        /// <summary>
        /// 建築/廠區/路段編號
        /// </summary>
        public string BuildingCode { get; set; } = null!;
        /// <summary>
        /// 巡檢單號
        /// </summary>
        public string RecordSn { get; set; } = null!;
        /// <summary>
        /// 專案編號
        /// </summary>
        //public int? Project { get; set; }
        /// <summary>
        /// 表單型態，Plan：年度計畫，Single：臨時
        /// </summary>
        public string OrderType { get; set; } = null!;
        /// <summary>
        /// 巡檢項目
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// 檢查表文字串，逗號分隔
        /// </summary>
        public string? Forms { get; set; }
        /// <summary>
        /// 設備描述文字串，逗號分隔
        /// </summary>
        public string? Devices { get; set; }
        /// <summary>
        /// 預設執行人員
        /// </summary>
        public string? Executor { get; set; }
        /// <summary>
        /// 預排日期
        /// </summary>
        public DateTime ScheduledDate { get; set; }
        /// <summary>
        /// 執行人員
        /// </summary>
        public string? Staff { get; set; }
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
        /// <summary>
        /// 轉工單號
        /// </summary>
        public string? WorkSn { get; set; }
        /// <summary>
        /// 工時(分鐘)
        /// </summary>
        public int ManMinute { get; set; }
        /// <summary>
        /// 工單中需要處理的設備
        /// </summary>
        public List<EqptOrderDevice> OrderDevices { get; set; } = new List<EqptOrderDevice>();
    }
}
