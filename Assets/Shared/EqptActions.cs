using System;

#nullable enable 
namespace iBMSApp.Shared
{
    // File Version DateTimeStamp: 202505161100

    public class EqptRequestOrders
    {
        /// <summary>
        /// 建築編號 RG:瑞光大樓 HSB:火獅樓
        /// </summary>
        public string BuildingCode { get; set; } = null!;

        /// <summary>
        /// 必填 日期格式:2023-01-30
        /// </summary>
        public string StartDate { get; set; } = null!;

        /// <summary>
        /// 必填 日期格式:2024-09-30
        /// </summary>
        public string EndDate { get; set; } = null!;
    }

    public class EqptRequestOrder
    {
        public string BuildingCode { get; set; } = null!;
        public string RecordSn { get; set; } = null!;
    }

    /// <summary>
    /// 回傳格式 <seealso cref="EqptRespondSubmitDevice"/>
    /// </summary>
    public class EqptRequestSubmitInspDevice
    {
        /// <summary>
        /// 建築編號
        /// </summary>
        public string BuildingCode { get; set; } = null!;

        /// <summary>
        /// 工作單編號
        /// </summary>
        public string RecordSn { get; set; } = null!;

        /// <summary>
        /// 設備代號
        /// </summary>
        public string DeviceCode { get; set; } = null!;

        /// <summary>
        /// 設備數量
        /// </summary>
        public string DeviceCount { get; set; } = null!;

        /// <summary>
        /// 表單編號列表，格式: x,xx,...
        /// </summary>
        public string FormSns { get; set; } = null!;

        /// <summary>
        /// 檢查作業的檢查項目經檢查後的不合格數
        /// </summary>
        public string Summarize { get; set; } = null!;

        /// <summary>
        /// 總結
        /// </summary>
        public string Result { get; set; } = null!;

        /// <summary>
        /// 檢查項目記錄
        /// </summary>
        public string Items { get; set; } = null!;

        /// <summary>
        /// 照片，格式: [x,xx,...] 不包含字串引號
        /// </summary>
        public string Photos { get; set; } = null!;

        /// <summary>
        /// 離線操作完畢時間 (在線請將此設為null)
        /// </summary>
        public string? NowTime { get; set; }
    }

    public class EqptRespondSubmitDevice
    {
        /// <summary>
        /// 結果，正確回傳 "Done"
        /// </summary>
        public string Result { get; set; } = null!;

        /// <summary>
        /// 後端處理過後的提交時間
        /// </summary>
        public string SubmitTime { get; set; } = null!;

        /// <summary>
        /// 圖片的Sn編號，可轉型為JsonStringArray的字串格式
        /// </summary>
        public string PhotoSns { get; set; } = null!;

        /// <summary>
        /// 計算過後的總維護時間
        /// </summary>
        public int ManMinute { get; set; }
    }

    /// <summary>
    /// 回傳格式 <seealso cref="EqptRespondSubmitOrder"/>
    /// </summary>
    public class EqptRequestSubmitOrder
    {
        /// <summary>
        /// 建築編號
        /// </summary>
        public string BuildingCode { get; set; } = null!;

        /// <summary>
        /// 工作單編號
        /// </summary>
        public string RecordSn { get; set; } = null!;

        /// <summary>
        /// 完工說明
        /// </summary>
        public string CompletionReport { get; set; } = null!;

        /// <summary>
        /// 離線操作完畢時間 (在線請將此設為null)
        /// </summary>
        public string? NowTime { get; set; }
    }

    public class EqptRespondSubmitOrder
    {
        /// <summary>
        /// 結果，正確回傳 "Done"
        /// </summary>
        public string Result { get; set; } = null!;

        /// <summary>
        /// 後端處理過後的提交時間
        /// </summary>
        public string SubmitTime { get; set; } = null!;

        /// <summary>
        /// 圖片的Sn編號，可轉型為JsonStringArray的字串格式
        /// </summary>
        public string PhotoSns { get; set; } = null!;

        /// <summary>
        /// 計算過後的總維護時間
        /// </summary>
        public int ManMinute { get; set; }
    }

    /// <summary>
    /// 回傳格式 <seealso cref="string"/> 成功返回"Done"，
    /// </summary>
    public class EqptRequestSaveOrderConsumables
    {
        /// <summary>
        /// 建築編號
        /// </summary>
        public string BuildingCode { get; set; } = null!;

        /// <summary>
        /// 巡檢單直接使用RecordSn 工單使用WorkRecordsdSn
        /// </summary>
        public string RecordSn { get; set; } = null!;

        /// <summary>
        /// 設備代號
        /// </summary>
        public string DeviceCode { get; set; } = null!;

        /// <summary>
        /// 耗材更換紀錄 <seealso cref="List{T}"/>: T = <see cref="EqptOrderDevConsumable"/>
        /// </summary>
        public string Consumables { get; set; } = null!;

        /// <summary>
        /// 離線操作完畢時間 (在線請將此設為null)
        /// </summary>
        public string? NowTime { get; set; }
    }

    /// <summary>
    /// 回傳格式 <seealso cref="string"/> 成功返回"Done"，
    /// </summary>
    public class EqptRequestSaveNumericalData
    {
        /// <summary>
        /// 建築編號
        /// </summary>
        public string BuildingCode { get; set; } = null!;

        /// <summary>
        /// 工作單編號
        /// </summary>
        public string RecordSn { get; set; } = null!;

        /// <summary>
        /// 設備代號
        /// </summary>
        public string DeviceCode { get; set; } = null!;

        /// <summary>
        /// 設備讀值  <seealso cref="List{T}"/> : T = <see cref="EqptOrderDevNumericalData"/>
        /// </summary>
        public string NumericalData { get; set; } = null!;

        /// <summary>
        /// 離線操作完畢時間 (在線請將此設為null)
        /// </summary>
        public string? NowTime { get; set; }
    }

    public class EqptRequestUpdateOrderDevice
    {
        /// <summary>
        /// 建築編號
        /// </summary>
        public string BuildingCode { get; set; } = null!;

        /// <summary>
        /// 工作單編號
        /// </summary>
        public string RecordSn { get; set; } = null!;

        /// <summary>
        /// 設備代號
        /// </summary>
        public string DeviceCode { get; set; } = null!;

        /// <summary>
        /// 離線操作完畢時間 (在線請將此設為null)
        /// </summary>
        public string? NowTime { get; set; }
    }

    public class EqptRequestDeviceBasic
    {
        /// <summary>
        /// 建築編號
        /// </summary>
        public string BuildingCode { get; set; } = null!;

        /// <summary>
        /// 設備編號
        /// </summary>
        public string DeviceCode { get; set; } = null!;
    }

    public class EqptRespondDeviceBasic
    {
        /// <summary>
        /// 建築名稱
        /// </summary>
        public string BuildingName { get; set; } = null!;

        /// <summary>
        /// 建築編號
        /// </summary>
        public string BuildingCode { get; set; } = null!;

        /// <summary>
        /// 設備編碼
        /// </summary>
        public string DeviceCode { get; set; } = null!;

        /// <summary>
        /// 設備短代號
        /// </summary>
        public string Code { get; set; } = null!;

        /// <summary>
        /// 設備模型代號
        /// </summary>
        public string DeviceId { get; set; } = null!;

        /// <summary>
        /// 設備種類
        /// </summary>
        public string Type { get; set; } = null!;

        /// <summary>
        /// 產線站點代號
        /// </summary>
        public string Station { get; set; } = null!;

        /// <summary>
        /// 設備名稱
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// 繁體中文
        /// </summary>
        public string zh_TW { get; set; } = null!;

        /// <summary>
        /// 英文
        /// </summary>
        public string en_WW { get; set; } = null!;

        /// <summary>
        /// 簡體中文
        /// </summary>
        public string zh_CN { get; set; } = null!;

        /// <summary>
        /// 泰文
        /// </summary>
        public string th_TH { get; set; } = null!;

        /// <summary>
        /// 預設巡檢週期是否為"日"保養
        /// </summary>
        public bool DailyMaint { get; set; } = false;
    }

    /// <summary>
    /// 提交工單前照片資料 回傳格式 <seealso cref="EqptRespondSavePrePhoto"/>
    /// </summary>
    public class EqptRequestSavePrePhoto
    {
        /// <summary>
        /// 建築編號
        /// </summary>
        public string BuildingCode { get; set; } = null!;

        /// <summary>
        /// 工單編號
        /// </summary>
        public string RecordSn { get; set; } = null!;

        /// <summary>
        /// 設備編號
        /// </summary>
        public string DeviceCode { get; set; } = null!;

        /// <summary>
        /// 照片 base64 陣列格式: [x,xx,...] 不包含字串引號
        /// </summary>
        public string Photos { get; set; } = null!;

        /// <summary>
        /// 圖片對應的 work record Sn (可為 null)
        /// </summary>
        public int? WorkRecordsdSn { get; set; }

        /// <summary>
        /// 圖片上傳時間 (可為 null，若 null 則預設為 Now)
        /// </summary>
        public string? NowTime { get; set; }
    }

    public class EqptRespondSavePrePhoto
    {
        /// <summary>
        /// 儲存結果的狀態
        /// </summary>
        public string Result { get; set; } = null!;

        /// <summary>
        /// 圖片的 Sn 格式 x,xx,...
        /// </summary>
        public string PhotoSns { get; set; } = null!;

        /// <summary>
        /// 轉工單的巡檢或報修單號
        /// </summary>
        public int WorkRecordsdSn { get; set; }
    }

    public class EqptRequestSubmitWorkDevice
    {
        /// <summary>
        /// 建築編號
        /// </summary>
        public string BuildingCode { get; set; } = null!;

        /// <summary>
        /// 工單編號
        /// </summary>
        public string RecordSn { get; set; } = null!;

        /// <summary>
        /// 設備編碼
        /// </summary>
        public string DeviceCode { get; set; } = null!;

        /// <summary>
        /// 0:紀錄，1:暫結，2:完工
        /// </summary>
        public string RecordType { get; set; } = null!; // "0" | "1" | "2"

        /// <summary>
        /// 總結
        /// </summary>
        public string Respond { get; set; } = null!;

        /// <summary>
        /// 處理後的圖片 格式: [x,xx,...]
        /// </summary>
        public string Photos { get; set; } = null!;    // Still a string (JSON array)

        /// <summary>
        /// 該單設備的總數量
        /// </summary>
        public int DeviceCount { get; set; }

        /// <summary>
        /// 圖片上傳時間 (可為 null，若 null 則預設為 Now)
        /// </summary>
        public string? NowTime { get; set; } = null!;
    }

    public class EqptRespondSubmitWorkDevice
    {
        /// <summary>
        /// 儲存結果的狀態
        /// </summary>
        public string Result { get; set; } = "Done";

        /// <summary>
        /// 處理後的 PhotoSns 格式 x,xx,...
        /// </summary>
        public string AfPhotoSns { get; set; } = "";
    }

    public class EqptRequestSetRepairOrder
    {
        /// <summary>
        /// 建築編號
        /// </summary>
        public string BuildingCode { get; set; } = null!;

        /// <summary>
        /// 報修編號
        /// </summary>
        public string RepairNo { get; set; } = null!;

        /// <summary>
        /// 設備種類
        /// </summary>
        public string DeviceType { get; set; } = null!;

        /// <summary>
        /// 設備編號
        /// </summary>
        public string DeviceCode { get; set; } = null!;

        /// <summary>
        /// 設備簡碼
        /// </summary>
        public string Code { get; set; } = null!;

        /// <summary>
        /// 設備描述
        /// </summary>
        public string DeviceDescription { get; set; } = null!;

        /// <summary>
        /// 報修描述
        /// </summary>
        public string Issue { get; set; } = null!;

        /// <summary>
        /// 照片
        /// </summary>
        public string Photo { get; set; } = null!;
        public string? NowTime { get; set; }
    }

    public class EqptRespondRepairOrder
    {
        /// <summary>
        /// 建築編號
        /// </summary>
        public string BuildingCode { get; set; } = null!;

        /// <summary>
        /// 建築名稱
        /// </summary>
        public string BuildingName { get; set; } = null!;

        /// <summary>
        /// 報修單號
        /// </summary>
        public string RecordSn { get; set; } = null!;

        /// <summary>
        /// 設備編號
        /// </summary>
        public string DeviceCode { get; set; } = null!;

        /// <summary>
        /// 設備類型
        /// </summary>
        public string DeviceType { get; set; } = null!;

        /// <summary>
        /// 設備簡碼
        /// </summary>
        public string Code { get; set; } = null!;

        /// <summary>
        /// 設備描述
        /// </summary>
        public string DeviceDescription { get; set; } = null!;

        /// <summary>
        /// 報修人ID
        /// </summary>
        public string Issuer { get; set; } = null!;

        /// <summary>
        /// 報修人名稱
        /// </summary>
        public string IssuerName { get; set; } = null!;

        /// <summary>
        /// 報修單位，可不填
        /// </summary>
        public string? Department { get; set; }

        /// <summary>
        /// 報修人連絡電話，可不填
        /// </summary>
        public string? Tel { get; set; }

        /// <summary>
        /// 報修人聯絡信箱，可不填
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 報修單狀態
        /// </summary>
        public string Status { get; set; } = null!;

        /// <summary>
        /// 圖片編號
        /// </summary>
        public string PhotoSns { get; set; } = null!;

        /// <summary>
        /// 異常描述
        /// </summary>
        public string Issue { get; set; } = null!;

        /// <summary>
        /// 預計修繕日期
        /// </summary>
        public DateTime? ScheduledDate { get; set; }

        /// <summary>
        /// 創單時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 已處理的時間
        /// </summary>
        public DateTime? ProcessTime { get; set; }

        /// <summary>
        /// 設備完成修繕時間
        /// </summary>
        public DateTime? CompleteTime { get; set; }

        /// <summary>
        /// 關聯的工單編號
        /// </summary>
        public string? WorkSn { get; set; }

        /// <summary>
        /// 回覆內容
        /// </summary>
        public string? Reply { get; set; } = null!;

        /// <summary>
        /// 回覆人員名稱
        /// </summary>
        public string StaffName { get; set; } = null!;
    }

}
