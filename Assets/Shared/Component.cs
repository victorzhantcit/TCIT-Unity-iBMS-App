#nullable enable 
namespace iBMSApp.Shared
{
    public class Component
    {
        public string ID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ComponentType { get; set; } = string.Empty;
        public string PlateNo { get; set; } = string.Empty;
        public string ProjectNo { get; set; } = string.Empty;
        public string FormType { get; set; } = string.Empty;
        public string CheTableNum { get; set; } = string.Empty;
        public string ReceiveTime { get; set; } = string.Empty;
        public bool IsDisabled { get; set; } = false;
        public string Receiver { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public bool Checked { get; set; } = false;    //從資料庫中讀回的原始狀態
        public bool IsSelected { get; set; } = false; //操作者在APP上變更過的狀態
    }
}
