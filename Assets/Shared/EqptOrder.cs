using System;
using System.Collections.Generic;

#nullable enable 
namespace iBMSApp.Shared
{
    public class EqptOrder
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
        /// 巡檢單，表單型態，Plan：年度計畫，Single：臨時
        /// </summary>
        public string OrderType { get; set; } = null!;
        /// <summary>
        /// 巡檢單，年計畫編號
        /// </summary>
        public int? InspSn { get; set; }
        /// <summary>
        /// 工單編號
        /// </summary>
        public string RecordSn { get; set; } = null!;
        /// <summary>
        /// 巡檢單，使用之檢查表串，用逗號分隔
        /// </summary>
        public string Forms { get; set; } = null!;
        /// <summary>
        /// 工單描述
        /// </summary>
        public string Description { get; set; } = null!;
        /// <summary>
        /// 預設處理人員
        /// </summary>
        public string ExecutorName { get; set; } = "";
        /// <summary>
        /// 最後一個處理巡檢單的人員
        /// </summary>
        public string StaffName { get; set; } = "";
        /// <summary>
        /// 工作時間
        /// </summary>
        public string ScheduledDate { get; set; } = "";
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
        public string? StartTime { get; set; }
        /// <summary>
        /// 暫結時間
        /// </summary>
        //public string? PauseTime { get; set; }
        /// <summary>
        /// 完工上傳時間
        /// </summary>
        public string? SubmitTime { get; set; }
        /// <summary>
        /// 退件時間
        /// </summary>
        public string? RejectTime { get; set; }
        /// <summary>
        /// 退件審查意見
        /// </summary>
        public string? Comment { get; set; }
        /// <summary>
        /// 完成時間
        /// </summary>
        public string? CompleteTime { get; set; }
        /// <summary>
        /// 花費工時(分鐘)
        /// </summary>
        public int ManMinute { get; set; }
        ///// <summary>
        ///// 工單處理前照片
        ///// </summary>
        //public string? PrePhotoSns { get; set; }
        ///// <summary>
        ///// 工單處理後照片
        ///// </summary>
        //public string? AfPhotoSns { get; set; }
        /// <summary>
        /// 工單中需要處理的設備
        /// </summary>
        public List<EqptOrderDevice> OrderDevices { get; set; } = new List<EqptOrderDevice>();
        /// <summary>
        /// 對應的照片代號串，以逗號分隔
        /// </summary>
        public string PhotoSns { get; set; } = "";
    }
    public class EqptOrderDevice
    {
        /// <summary>
        /// 建築編號
        /// </summary>
        //public string BuildingName { get; set; } = null!;
        public string BuildingCode { get; set; } = null!;
        /// <summary>
        /// 系統別
        /// </summary>
        public string System { get; set; } = null!;
        /// <summary>
        /// 設備類別
        /// </summary>
        public string Type { get; set; } = null!;
        /// <summary>
        /// 設備代號
        /// </summary>
        public string DeviceCode { get; set; } = null!;
        /// <summary>
        /// 設備簡碼
        /// </summary>
        public string Code { get; set; } = null!;
        /// <summary>
        /// 設備名稱
        /// </summary>
        public string DeviceDescription { get; set; } = "";
        /// <summary>
        /// 廠牌
        /// </summary>
        public string Manufacturer { get; set; } = "";
        /// <summary>
        /// 型號
        /// </summary>
        public string ModelNumber { get; set; } = "";
        /// <summary>
        /// 保固期限，string
        /// </summary>
        public string WarrantyEndDate { get; set; } = "";
        /// <summary>
        /// 保固期限，DateTime
        /// </summary>
        public DateTime? WarrantyTime { get; set; }
        /// <summary>
        /// 執行者
        /// </summary>
        public string Executor { get; set; } = "";
        /// <summary>
        /// 設備處理狀態，Pending 待處理; Processing 處理中; Pause 暫結; Submitted 完工上傳; Approving 覆核中;Redistribute 另開工單; Completed 已完成;
        /// </summary>
        public string Status { get; set; } = null!;
        /// <summary>
        /// 現場掃碼開始作業時間
        /// </summary>
        public string? StartTime { get; set; }
        /// <summary>
        /// 暫結時間
        /// </summary>
        public string? PauseTime { get; set; }
        /// <summary>
        /// 完工上傳時間
        /// </summary>
        public string? SubmitTime { get; set; }
        /// <summary>
        /// 完成時間
        /// </summary>
        public string? CompleteTime { get; set; }
        /// <summary>
        /// 預估工時(分鐘)
        /// </summary>
        public int EstManMinute { get; set; } = 0;
        /// <summary>
        /// 花費工時(分鐘)
        /// </summary>
        public int ManMinute { get; set; } = -1;
        /// <summary>
        /// 工單處理前照片
        /// </summary>
        public string? PrePhotoSns { get; set; }
        /// <summary>
        /// 工單處理後照片
        /// </summary>
        public string AfPhotoSns { get; set; } = "";// string.Empty!;
        /// <summary>
        /// 說明是報修轉單來的，還是巡檢轉單過來的
        /// </summary>
        public List<EqptOrderFrom> Froms { get; set; } = new List<EqptOrderFrom>();
        /// <summary>
        /// 檢查表代號串，用逗號分隔
        /// </summary>
        public List<int> FormSns { get; set; } = new List<int>();
        /// <summary>
        /// 檢查表名稱串，用逗號分隔
        /// </summary>
        public List<string> FormNames { get; set; } = new List<string>();
        /// <summary>
        /// 檢查作業的檢查項目
        /// </summary>
        public List<EqptOrderItem> Items { get; set; } = new List<EqptOrderItem>();
        /// <summary>
        /// 檢查作業的檢查項目經檢查後的不合格數
        /// </summary>
        public int Summarize { get; set; } = 0;
        /// <summary>
        /// 設備讀值
        /// </summary>
        public List<EqptOrderDevNumericalData> NumericalData { get; set; } = new List<EqptOrderDevNumericalData>();
        /// <summary>
        /// 設備讀值時間
        /// </summary>
        public string? DataTime { get; set; }
        /// <summary>
        /// 耗材更換紀錄
        /// </summary>
        public List<EqptOrderDevConsumable> Consumables { get; set; } = new List<EqptOrderDevConsumable>();
        /// <summary>
        /// 報修轉單來的單號，或巡檢轉單過來的單號
        /// </summary>
        public int WorkRecordsdSn { get; set; } = 0;
        /// <summary>
        /// 處理內容說明或回應內容
        /// </summary>
        public string Respond { get; set; } = "";
        /// <summary>
        /// 回應紀錄
        /// </summary>
        public List<EqptOrderRecord> Records { get; set; } = new List<EqptOrderRecord>();
        /// <summary>
        /// 是否唯讀
        /// </summary>
        public bool IsDisabled { get; set; } = false;
    }
    public class EqptOrderDevNumericalData
    {
        /// <summary>
        /// 點位
        /// </summary>
        public string TagName { get; set; } = null!;
        /// <summary>
        /// 點位數值的單位
        /// </summary>
        public string TagUnit { get; set; } = null!;
        /// <summary>
        /// 點位名稱
        /// </summary>
        public string TagDescription { get; set; } = null!;
        /// <summary>
        /// 點位數值, string 方便顯示空值
        /// </summary>
        public string Value { get; set; } = "";
    }
    public class EqptOrderDevConsumable
    {
        /// <summary>
        /// 更換勾選
        /// </summary>
        public bool isChecked { get; set; } = false;
        /// <summary>
        /// 耗材名稱
        /// </summary>
        public string Name { get; set; } = null!;
        /// <summary>
        /// 更換日期
        /// </summary>
        public string ReplaceDate { get; set; } = "";
        /// <summary>
        /// 耗材更換數量
        /// </summary>
        public int AvailableNum { get; set; } = 1;
        /// <summary>
        /// 耗材單位
        /// </summary>
        public string AvailableUnit { get; set; } = "";
    }
    public class EqptOrderFrom
    {
        /// <summary>
        /// 開單原因 R:報修單,I:巡檢單
        /// </summary>
        public string OrderType { get; set; } = null!;
        /// <summary>
        /// 報修單或巡檢單的單號
        /// </summary>
        public string FromAnOrder { get; set; } = null!;
        /// <summary>
        /// 報修單或巡檢單的內容
        /// </summary>
        public string OrderDescription { get; set; } = null!;
    }
    public class EqptOrderItem
    {
        /// <summary>
        /// 巡檢項目
        /// </summary>
        public string ItemName { get; set; } = null!;
        /// <summary>
        /// 巡檢的狀態選項
        /// </summary>
        public Dictionary<string, string> Status { get; set; } = new Dictionary<string, string>();
        //public JObject Status { get; set; } = null!;
        /// <summary>
        /// 檢查方式
        /// </summary>
        public string Method { get; set; } = null!;
        /// <summary>
        /// 巡檢時設備運作狀態
        /// </summary>
        //public bool Running { get; set; } = false;
        public string Running { get; set; } = "";
        /// <summary>
        /// 巡檢指引
        /// </summary>
        public string Reference { get; set; } = "";
        /// <summary>
        /// 設備巡檢狀態選擇
        /// </summary>
        public string Selected { get; set; } = "";
        /// <summary>
        /// 檢查說明
        /// </summary>
        public string Note { get; set; } = "";
    }
    public class EqptOrderRecord
    {
        /// <summary>
        /// 回覆時間
        /// </summary>
        public string RespondTime { get; set; } = null!;
        /// <summary>
        /// 回覆人
        /// </summary>
        public string StaffName { get; set; } = null!;
        /// <summary>
        /// 回覆內容
        /// </summary>
        public string Respond { get; set; } = null!;
        /// <summary>
        /// 處理紀錄時設備狀態，Pending 待處理; Processing 處理中; Pause 暫結; Submitted 完工上傳;
        /// </summary>
        public string Status { get; set; } = null!;
        /// <summary>
        /// 工時(分鐘)
        /// </summary>
        public int ManMinute { get; set; } = 0;
    }
}
