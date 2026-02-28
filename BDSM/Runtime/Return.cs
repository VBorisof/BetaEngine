namespace BDSM.Runtime;

#nullable disable

public class Return : RuntimeError
{
    public object Value { get; }

    public Return(object value) : base(null, null)
    {
        Value = value;
    }
}