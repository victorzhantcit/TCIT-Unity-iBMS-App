using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable 
namespace iBMSApp.Shared
{
    public class EqptRepairOrder
    {
        public string? statusColor { get; set; }
        public string? statusBGColor { get; set; }
        public int Sn { get; set; }
        /// <summary>
        /// 建築編號
        /// </summary>
        public string BuildingCode { get; set; } = "";
        public string BuildingName { get; set; } = "";
        /// <summary>
        /// 報修單號
        /// </summary>
        public string RecordSn { get; set; } = null!;
        /// <summary>
        /// 設備類型，非設備則為Other
        /// </summary>
        public string DeviceType { get; set; } = null!;
        /// <summary>
        /// 設備代號，可無
        /// </summary>
        public string DeviceCode { get; set; } = "";
        /// <summary>
        /// 設備短代號
        /// </summary>
        public string Code { get; set; } = "";
        /// <summary>
        /// 設備名稱或自填
        /// </summary>
        public string? DeviceDescription { get; set; }
        /// <summary>
        /// 報修人
        /// </summary>
        public string Issuer { get; set; } = "";
        /// <summary>
        /// 報修人姓名
        /// </summary>
        public string? IssuerName { get; set; }
        /// <summary>
        /// 報修單位
        /// </summary>
        public string? Department { get; set; }
        /// <summary>
        /// 報修人連絡電話
        /// </summary>
        public string? Tel { get; set; }
        /// <summary>
        /// 報修人Email
        /// </summary>
        public string? Email { get; set; }
        /// <summary>
        /// 報修時間
        /// </summary>
        public DateTime? CreateTime { get; set; }
        /// <summary>
        /// 報修隨附照片SN
        /// </summary>
        public string? PhotoSns { get; set; }
        /// <summary>
        /// 異常描述
        /// </summary>
        public string? Issue { get; set; }
        /// <summary>
        /// 報修單狀態
        /// </summary>
        public string Status { get; set; } = null!;
        /// <summary>
        /// 預計修繕日期
        /// </summary>
        public DateTime? ScheduledDate { get; set; }
        /// <summary>
        /// 報修處理人
        /// </summary>
        public string? Staff { get; set; }
        /// <summary>
        /// 處理人員的處理方式描述
        /// </summary>
        public string? Reply { get; set; }
        /// <summary>
        /// 處理人員回覆時間
        /// </summary>
        public DateTime? ReplyTime { get; set; }
        /// <summary>
        /// 轉工單號
        /// </summary>
        public string? WorkSn { get; set; }
        /// <summary>
        /// 回覆照片SN
        /// </summary>
        public string? RphotoSns { get; set; }
        /// <summary>
        /// 簽名圖檔
        /// </summary>
        public string? SignPhoto { get; set; }
        /// <summary>
        /// 責任單位
        /// </summary>
        public string? DutyUnit { get; set; }
        /// <summary>
        /// 付款單位
        /// </summary>
        public string? PayUnit { get; set; }
        /// <summary>
        /// 修繕金額(元)
        /// </summary>
        public int? PayAmount { get; set; }
        /// <summary>
        /// 已處理的時間
        /// </summary>
        public DateTime? ProcessTime { get; set; }
        /// <summary>
        /// 設備完成修繕時間
        /// </summary>
        public DateTime? CompleteTime { get; set; }
    }
}
