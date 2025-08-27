using System;
using System.Text.Json;

#nullable enable 
namespace iBMSApp.Shared
{
    public class EqptDailyMaintenance
    {
        /// <summary>
        /// 主索引鍵
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 大樓代號
        /// </summary>
        public string BuildingCode { get; set; } = null!;
        /// <summary>
        /// 設備編碼
        /// </summary>
        public string DeviceCode { get; set; } = null!;
        /// <summary>
        /// 年
        /// </summary>
        public short Year { get; set; }
        /// <summary>
        /// 月
        /// </summary>
        public short Month { get; set; }
        /// <summary>
        /// 日
        /// </summary>
        public short Day { get; set; }
        /// <summary>
        /// 檢查表
        /// </summary>
        public int FormSn { get; set; }
        public string FormName { get; set; } = null!;
        /// <summary>
        /// 執行人員
        /// </summary>
        public string Staff { get; set; } = null!;
        public string StaffName { get; set; } = null!;
        /// <summary>
        /// 開始時間
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 完工上傳時間
        /// </summary>
        public DateTime? SubmitTime { get; set; }
        /// <summary>
        /// 人時(分鐘)
        /// </summary>
        public int? ManMinute { get; set; }
        /// <summary>
        /// 設備數據
        /// </summary>
        public string? NumericalData { get; set; }
        /// <summary>
        /// 耗材更換記錄
        /// </summary>
        public string? Consumables { get; set; }
        /// <summary>
        /// 檢查項目記錄
        /// </summary>
        public JsonElement Items { get; set; }
        /// <summary>
        /// 日保養照片
        /// </summary>
        public string? PhotoSns { get; set; }
        /// <summary>
        /// 日保養結果，0:OK
        /// </summary>
        public string? Summarize { get; set; }
        /// <summary>
        /// 總結
        /// </summary>
        public string? Result { get; set; }
        /// <summary>
        /// 定位位置
        /// </summary>
        public string? Location { get; set; }
    }
}
