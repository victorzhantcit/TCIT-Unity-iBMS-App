using System.Collections.Generic;

#nullable enable 
namespace iBMSApp.Shared
{
    public class MsgInfo
    {
        /// <summary>
        /// 訊息標題
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// 訊息內容
        /// </summary>
        public string Msg { get; set; } = "";

        /// <summary>
        /// 照片id array
        /// </summary>
        //public ImageFile Img { get; set; } = new();
        public List<string> ImgList { get; set; } = new();

        /// <summary>
        /// 照片idx
        /// </summary>
        public string Imgid { get; set; } = "";

        /// <summary>
        /// 照片
        /// </summary>
        public ImageFile Img { get; set; } = new ();

        /// <summary>
        /// 是否顯示訊息
        /// </summary>
        public bool showMsg { get; set; } = false;

        public string NowTime { get; set; } = "";
        /// <summary>
        /// 看看是呼叫哪一個 GetPhoto
        /// </summary>
        public string Url { get; set; } = "eqpt";
    }
}
