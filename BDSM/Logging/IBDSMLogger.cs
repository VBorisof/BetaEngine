namespace BDSM.Logging;

public interface IBDSMLogger
{
    public void Error(int lineNumber, int? colNumber, string line, string message);
    public void Info(string message);
    public void Debug(string message);
}
