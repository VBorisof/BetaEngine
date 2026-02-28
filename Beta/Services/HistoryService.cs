using Beta.Common;

namespace Beta.Services;

public class HistoryService
{
    private readonly RingedStringBuffer _buffer = new(8192);

    public void Append(string text)
    {
        _buffer.Add(text);
        _buffer.Add("\n");
    }

    public void Clear()
    {
        _buffer.Clear();
    }

    public string Get()
    {
        return _buffer.ToString();
    }
}