using System.Threading.Tasks;

namespace iBMSApp.UI.Common
{
    public interface IPageDisplay
    {
        /// <summary>
        /// 顯示頁面
        /// </summary>
        public void Show();

        /// <summary>
        /// 隱藏頁面
        /// </summary>
        public void Hide();

        /// <summary>
        /// 更新頁面
        /// </summary>
        /// <param name="refreshParams">更新頁面的參數</param>
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
