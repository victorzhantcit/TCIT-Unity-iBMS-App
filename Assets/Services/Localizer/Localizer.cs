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
        /// ���o�ثe�y�t����r
        /// </summary>
        /// <param name="label">�ھ� Localization Table Key �M�䵲�G</param>
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
        /// �ͦ����w�˦�����Ȧr��
        /// </summary>
        /// <param name="localizedLabelKey">
        /// �������䪺�r�� <br></br> 
        /// �Y�n���a�Ʀr��бҥ� <see cref="LocalizedSt yle.LabelColored"/> �ýᤩ <see cref="LocalizedStyleParams.LabelColor"/>)
        /// </param>
        /// <param name="localizedValueKey">
        /// ������Ȫ��r�� <br></br>
        /// (�Y�n���a�Ʀr��бҥ� <see cref="LocalizedStyle.ValueColored"/> �ýᤩ <see cref="LocalizedStyleParams.ValueColor"/>)
        /// </param>
        /// <param name="styleType">
        /// �˦��N���A�䴩�걵�V�X (Enum | Enum | ...)
        /// </param>
        /// <param name="styleParams">
        /// �˦��ѼơA�M�λݭn�ҥι������˦��N�� <br></br>
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
        /// �ͦ������˦����r�� (�M�Φ�<seealso cref="string.Format(string, object[])"/>)
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

            string commaStr = hasComma ? "�G" : "";
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
