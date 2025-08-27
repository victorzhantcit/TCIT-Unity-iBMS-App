using System;

#nullable enable 
namespace iBMSApp.Shared
{
    public class EqptPhoto
    {
        /// <summary>
        /// 照片索引號
        /// </summary>
        public int Sn { get; set; }

        /// <summary>
        /// 大樓/社區代號
        /// </summary>
        public string BuildingCode { get; set; } = null!;

        /// <summary>
        /// 報修號/巡檢單號/工單編號
        /// </summary>
        public string? RecordSn { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 檔案大小
        /// </summary>
        public int? Size { get; set; }

        /// <summary>
        /// 照片內容
        /// </summary>
        public string? Photo { get; set; }
    }

}
