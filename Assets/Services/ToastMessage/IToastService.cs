using System;

namespace iBMSApp.Services
{
    public interface IToastService
    {
        event Action<string, ToastLevel> OnShow;
        event Action OnHide;
        void ShowToast(string message, ToastLevel level);
        void Dispose();
    }

    public enum ToastLevel
    {
        Info,
        Remind,
        Success,
        Warning,
        Error
    }
}
