using System;

#nullable enable 
namespace iBMSApp.Shared
{
    public partial class EqptWorkOrderOrigin
    {
        /*public int Id { get; set; }
        public string BuildingCode { get; set; } = null!;
        public string RecordSn { get; set; } = null!;
        public string? System { get; set; }
        /// <summary>
        /// 設備類型
        /// </summary>
        public string? DeviceType { get; set; }
        /// <summary>
        /// 設備編碼
        /// </summary>
        public string? DeviceCode { get; set; }
        /// <summary>
        /// 設備名稱
        /// </summary>
        public string? DeviceDescription { get; set; }*/
        /// <summary>
        /// 表單類型 R : 報修單, I : 巡檢單
        /// </summary>
        public string? OrderType { get; set; }
        /// <summary>
        /// 轉工單的報修/巡檢單號
        /// </summary>
        public string? FromAnOrder { get; set; }
        /// <summary>
        /// 報修/巡檢單內容
        /// </summary>
        public string? OrderDescription { get; set; }
        /// <summary>
        /// 轉工單的時間
        /// </summary>
        public DateTime? TransferTime { get; set; }
        /// <summary>
        /// 轉工單負責人
        /// </summary>
        public string? Staff { get; set; }
        public int? RepairFees { get; set; }
        public int? ManMinute { get; set; }
    }
}
