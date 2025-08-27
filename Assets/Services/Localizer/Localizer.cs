using System;
using UnityEngine;
using UnityEngine.Localization;

namespace iBMSApp.Services
{
    public class Localizer : MonoBehaviour
    {
        public static Localizer Instance { get; private set; }

        [SerializeField] private LocalizedStringTable localizedTableRef;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 取得目前語系的單字
        /// </summary>
        /// <param name="label">根據 Localization Table Key 尋找結果</param>
        /// <returns></returns>
        public string GetLocalizedString(string label)
        {
            var localizedString = new LocalizedString(localizedTableRef.TableReference, label);
            string localizedResult = localizedString.GetLocalizedString();

            if (localizedResult == null || localizedResult.StartsWith("No translation found for"))
                return label;

            return localizedResult;
        }

        /// <summary>
        /// 生成指定樣式的鍵值字串
        /// </summary>
        /// <param name="localizedLabelKey">
        /// 資料欄位鍵的字串 <br></br> 
        /// 若要本地化字串請啟用 <see cref="LocalizedSt yle.LabelColored"/> 並賦予 <see cref="LocalizedStyleParams.LabelColor"/>)
        /// </param>
        /// <param name="localizedValueKey">
        /// 資料欄位值的字串 <br></br>
        /// (若要本地化字串請啟用 <see cref="LocalizedStyle.ValueColored"/> 並賦予 <see cref="LocalizedStyleParams.ValueColor"/>)
        /// </param>
        /// <param name="styleType">
        /// 樣式代號，支援串接混合 (Enum | Enum | ...)
        /// </param>
        /// <param name="styleParams">
        /// 樣式參數，套用需要啟用對應的樣式代號 <br></br>
        /// (ex: <see cref="LocalizedStyle.Indent"/> => <see cref="LocalizedStyleParams.Indent"/>)
        /// </param>
        /// <returns></returns>
        public static string GenerateLocalizedRichText(string localizedLabelKey, string localizedValueKey
            , LocalizedStyle styleType, LocalizedStyleParams styleParams = null)
        {
            return GenerateFormattedString(new LocalizedLabelValue
            {
                Label = localizedLabelKey,
                Value = localizedValueKey,
                IsLabelLocalized = styleType.HasFlag(LocalizedStyle.LabelLocalized),
                IsValueLocalized = styleType.HasFlag(LocalizedStyle.ValueLocalized),
                RichTextFormat = GenerateFormat(styleType, styleParams)
            });
        }

        private static string GenerateFormattedString(LocalizedLabelValue localizerParams)
        {
            localizerParams ??= new LocalizedLabelValue();

            string fieldLabel = (localizerParams.IsLabelLocalized)
                ? Instance.GetLocalizedString(localizerParams.Label)
                : localizerParams.Label;
            string fieldValue = (localizerParams.IsValueLocalized)
                ? Instance.GetLocalizedString(localizerParams.Value)
                : localizerParams.Value;

            return string.Format(localizerParams.RichTextFormat, fieldLabel, fieldValue);
        }

        /// <summary>
        /// 生成對應樣式的字串 (套用至<seealso cref="string.Format(string, object[])"/>)
        /// </summary>
        /// <param name="styleType"></param>
        /// <param name="styleParams"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        private static string GenerateFormat(LocalizedStyle styleType, LocalizedStyleParams styleParams)
        {
            styleParams ??= new LocalizedStyleParams();

            bool hasComma = styleType.HasFlag(LocalizedStyle.Comma);
            bool hasIndent = styleType.HasFlag(LocalizedStyle.Indent);
            bool isDualAligned = styleType.HasFlag(LocalizedStyle.DualAligned);
            bool isLabelColored = styleType.HasFlag(LocalizedStyle.LabelColored);
            bool isValueColored = styleType.HasFlag(LocalizedStyle.ValueColored);

            if (isLabelColored && string.IsNullOrWhiteSpace(styleParams.LabelColor))
                throw new FormatException($"{nameof(LocalizedStyle.LabelColored)} requires a valid {nameof(styleParams.LabelColor)}.");

            if (isValueColored && string.IsNullOrWhiteSpace(styleParams.ValueColor))
                throw new FormatException($"{nameof(LocalizedStyle.ValueColored)} requires a valid {nameof(styleParams.ValueColor)}.");

            if (hasIndent && styleParams.Indent < 0)
                throw new FormatException($"{nameof(LocalizedStyle.Indent)} requires a non-negative {nameof(styleParams.Indent)}.");

            string commaStr = hasComma ? "：" : "";
            string labelWrap = isLabelColored ? $"<color={styleParams.LabelColor}>{{0}}</color>" : "{0}";
            string valueWrap = isValueColored ? $"<color={styleParams.ValueColor}>{{1}}</color>" : "{1}";

            if (isDualAligned)
            {
                return $"<line-height=\"0%\">{labelWrap}{commaStr}\n<line-height=\"0%\"><align=\"right\">{valueWrap}</align></line-height>";
            }
            else if (hasIndent)
            {
                return $"{labelWrap}{commaStr}<indent={styleParams.Indent}>{valueWrap}</indent>";
            }
            else
            {
                return $"{labelWrap}{commaStr}{valueWrap}";
            }
        }
    }
}
