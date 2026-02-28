namespace Beta.Services;

public interface IToastService
{
    public bool ShowTip(int durationSeconds, string id, string text);
    public void Notify(int durationSeconds, string text);
}