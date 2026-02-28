namespace Beta.Logging;

public interface ILogger
{
    public void Error(string message="");
    public void Warning(string message="");
    public void Info(string message="");
    public void Debug(string message="");
    public void Trace(string message="");
    public void LogMore();
    public void LogLess();
}

