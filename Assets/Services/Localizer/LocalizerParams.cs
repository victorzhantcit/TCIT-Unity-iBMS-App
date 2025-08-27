using System;

namespace iBMSApp.Services
{
    public class LocalizedLabelValue
    {
        /// <summary>
        /// ������W��
        /// </summary>
        public string Label { get; set; } = "Key";

        /// <summary>
        /// �������
        /// </summary>
        public string Value { get; set; } = "Value";

        /// <summary>
        /// ��ƦW�٬O�_�����a�����
        /// </summary>
        public bool IsLabelLocalized { get; set; } = false;

        /// <summary>
        /// ������ȬO�_�����a�����
        /// </summary>
        public bool IsValueLocalized { get; set; } = false;

        /// <summary>
        /// �榡�ƪ��˦�
        /// </summary>
        public string RichTextFormat { get; set; } = "{0}�G{1}";
    }

    /// <summary>
    /// �˦��N���A�䴩�걵�V�X (Enum | Enum | ...)
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
        /// ������W�٪��C�� ex: #FFFFFF�Bred
        /// </summary>
        public string LabelColor { get; set; } = null;

        /// <summary>
        /// ������Ȫ��C�� ex: #FFFFFF�Bred
        /// </summary>
        public string ValueColor { get; set; } = null;

        /// <summary>
        /// ��P�Ȥ������Y��
        /// </summary>
        public int Indent { get; set; } = -1;
    }
}
