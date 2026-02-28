namespace Beta.Common;

public class RingedStringBuffer(int capacity)
{
    private readonly string[] _buffer = new string[capacity];
    private int _start;
    private int _count;

    public void Add(string s)
    {
        // Either place the index at available spot,
        // or rollover to beginning of the buffer.
        var nextIndex = (_start + _count) % _buffer.Length;
        _buffer[nextIndex] = s;

        // See if we reached the end yet?
        if (_count < _buffer.Length)
        {
            // If not, just bump the counter.
            _count++;
        }
        else
        {
            // Otherwise, place the start right after the last item.
            _start = (nextIndex + 1) % _buffer.Length;
        }
    }

    public void Clear()
    {
        // Don't actually clear anything, just overwrite
        // with new data seamlessly.
        _start = 0;
        _count = 0;
    }

    public override string ToString()
    {
        // Take everything until the end
        if (_start == 0)
        {
            return string.Join('\n', _buffer[_start..(_start + _count)]);
        }

        // Otherwise, take everything till the end of the buffer,
        // plus everything from the beginning of the buffer until the 'start'.
        return string.Join('\n', _buffer[_start..])
            + '\n'
            + string.Join('\n', _buffer[.._start]);
    }
}