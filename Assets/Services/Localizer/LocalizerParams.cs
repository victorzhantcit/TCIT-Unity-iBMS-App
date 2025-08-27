using System;

namespace iBMSApp.Services
{
    public class LocalizedLabelValue
    {
        /// <summary>
        /// 資料欄位名稱
        /// </summary>
        public string Label { get; set; } = "Key";

        /// <summary>
        /// 資料欄位值
        /// </summary>
        public string Value { get; set; } = "Value";

        /// <summary>
        /// 資料名稱是否為本地化鍵值
        /// </summary>
        public bool IsLabelLocalized { get; set; } = false;

        /// <summary>
        /// 資料欄位值是否為本地化鍵值
        /// </summary>
        public bool IsValueLocalized { get; set; } = false;

        /// <summary>
        /// 格式化的樣式
        /// </summary>
        public string RichTextFormat { get; set; } = "{0}：{1}";
    }

    /// <summary>
    /// 樣式代號，支援串接混合 (Enum | Enum | ...)
    /// </summary>
    [Flags]
    public enum LocalizedStyle
    {
        LabelLocalized = 1 << 0,
        ValueLocalized = 1 << 1,
        Comma = 1 << 2,
        Indent = 1 << 3,
        LabelColored = 1 << 4,
        ValueColored = 1 << 5,
        DualAligned = 1 << 6,
    }

    public class LocalizedStyleParams
    {
        /// <summary>
        /// 資料欄位名稱的顏色 ex: #FFFFFF、red
        /// </summary>
        public string LabelColor { get; set; } = null;

        /// <summary>
        /// 資料欄位值的顏色 ex: #FFFFFF、red
        /// </summary>
        public string ValueColor { get; set; } = null;

        /// <summary>
        /// 鍵與值之間的縮排
        /// </summary>
        public int Indent { get; set; } = -1;
    }
}
