namespace CommunityLink.App.Services;

public enum ToastLevel { Info, Success, Warning, Error }

public sealed record ToastMessage(string Title, string Message, ToastLevel Level);

public sealed class ToastService
{
    public event Action<ToastMessage>? OnToast;

    public void Show(string message, ToastLevel level = ToastLevel.Info, string title = "")
    {
        OnToast?.Invoke(new ToastMessage(title, message, level));
    }

    public void Success(string message, string title = "Success") => Show(message, ToastLevel.Success, title);
    public void Error(string message, string title = "Notice") => Show(message, ToastLevel.Error, string.IsNullOrWhiteSpace(title) || title == "Error" ? "Notice" : title);
    public void Warning(string message, string title = "Notice") => Show(message, ToastLevel.Warning, string.IsNullOrWhiteSpace(title) || title == "Warning" ? "Notice" : title);
    public void Info(string message, string title = "Info") => Show(message, ToastLevel.Info, title);
}