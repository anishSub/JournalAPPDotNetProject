using System;

namespace JournalApp.Services
{
    public enum ToastType
    {
        Success,
        Info,
        Warning,
        Error
    }

    public class ToastService
    {
        public event Action<string, ToastType>? OnShow;

        public void ShowToast(string message, ToastType type)
        {
            OnShow?.Invoke(message, type);
        }

        public void ShowSuccess(string message) => ShowToast(message, ToastType.Success);
        public void ShowInfo(string message) => ShowToast(message, ToastType.Info);
        public void ShowWarning(string message) => ShowToast(message, ToastType.Warning);
        public void ShowError(string message) => ShowToast(message, ToastType.Error);
    }
}
