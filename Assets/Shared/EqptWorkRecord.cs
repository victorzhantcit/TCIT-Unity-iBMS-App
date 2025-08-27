using System;
using System.Collections.Generic;

#nullable enable 
namespace iBMSApp.Shared
{
    public partial class EqptWorkRecord
    {
        /// <summary>
        /// 主索引鍵
        /// </summary>
        public int Sn { get; set; }
        /// <summary>
        /// 建築編號
        /// </summary>
        public string? BuildingCode { get; set; }
        /// <summary>
        /// 工單號
        /// </summary>
        public string? RecordSn { get; set; }
        /// <summary>
        /// 設備編號
        /// </summary>
        public string? DeviceCode { get; set; }

        public string? DeviceDescription { get; set; }
        public string Manufacturer { get; set; } = "";
        public string ModelNumber { get; set; } = "";
        public DateTime? WarrantyTime { get; set; }
        /// <summary>
        /// 回應或審查者
        /// </summary>
        public string Staff { get; set; } = null!;
        /// <summary>
        /// 工單處理前照片SN
        /// </summary>
        public string? PrePhotoSns { get; set; }
        public List<EqptWorkOrderOrigin> OrderOrigin { get; set; } = new List<EqptWorkOrderOrigin>();
        /// <summary>
        /// 耗材
        /// </summary>
        public List<InspConsumables> Consumables { get; set; } = new List<InspConsumables>();
        /// <summary>
        /// 回應或審查內容
        /// </summary>
        public string Respond { get; set; } = null!;
        /// <summary>
        /// 回應或審查時間
        /// </summary>
        public DateTime RespondTime { get; set; }
        /// <summary>
        /// 工單處理後照片SN
        /// </summary>
        public string? AfPhotoSns { get; set; }
        /// <summary>
        /// 掃碼開始工作時間
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
        /// 花費工時(分鐘)
        /// </summary>
        public int? ManMinute { get; set; }
        /// <summary>
        /// 定位資訊
        /// </summary>
        public string? Location { get; set; }
        public string Status { get; set; } = null!;
    }
}
