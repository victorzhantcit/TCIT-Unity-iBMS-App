using System.Threading.Tasks;

namespace iBMSApp.UI.Common
{
    public interface IPageDisplay
    {
        /// <summary>
        /// ��ܭ���
        /// </summary>
        public void Show();

        /// <summary>
        /// ���í���
        /// </summary>
        public void Hide();

        /// <summary>
        /// ��s����
        /// </summary>
        /// <param name="refreshParams">��s�������Ѽ�</param>
        public Task RefreshPage(PageRefreshParams refreshParams = null);
    }

    public class PageRefreshParams
    {
        public string BuildingCode { get; set; } = null;
        public string RecordSn { get; set; } = null;
        public string DeviceCode { get; set; } = null;
        public bool PageReadOnly { get; set; } = true;
    }
}
