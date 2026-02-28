using BDSM.Functions;
using BDSM.Runtime;
using BDSM.Runtime.SaveGames;
using BDSM.Tokens;
using System.Collections.Generic;

#nullable disable

namespace BDSM.Instances;

public class Instance
{
    public Dictionary<string, object> Fields { get; } = [];
    protected Dictionary<string, ICallable> Methods { get; } = [];

    public Instance()
    {
    }

    public object Get(Token name)
    {
        if (Fields.TryGetValue(name.Lexeme, out var field))
        {
            return field;
        }
        if (Methods.TryGetValue(name.Lexeme, out var method))
        {
            return method;
        }
        throw new RuntimeError(name, $"Undefined field `{name.Lexeme}`.");
    }
    public object Get(string name)
    {
        if (Fields.TryGetValue(name, out var field))
        {
            return field;
        }
        if (Methods.TryGetValue(name, out var method))
        {
            return method;
        }
        throw new RuntimeError(null, $"Undefined field `{name}`.");
    }

    public void AddMethod(Token name, ICallable val)
    {
        Methods[name.Lexeme] = val;
    }
    public void AddMethod(string name, ICallable val)
    {
        Methods[name] = val;
    }

    public bool TryGetMethod(string name, out ICallable method)
    {
        if (!Methods.TryGetValue(name, out var value))
        {
            method = null;
            return false;
        }
        method = value;
        return true;
    }

    public void AddField(Token name, object val)
    {
        Fields[name.Lexeme] = val;
    }

    public void AddField(string name, object val)
    {
        Fields[name] = val;
    }

    public void SetField(Token name, object val)
    {
        Fields[name.Lexeme] = val;
    }
    public void SetField(string name, object val)
    {
        Fields[name] = val;
    }

    public void SetFieldsFromSaveData(
        FieldSaveData fieldData
    )
    {
        foreach (var b in fieldData.BoolFields)
        {
            Fields[b.Key] = b.Value;
        }
        foreach (var s in fieldData.StringFields)
        {
            Fields[s.Key] = s.Value;
        }
        foreach (var d in fieldData.DoubleFields)
        {
            Fields[d.Key] = d.Value;
        }
    }
}