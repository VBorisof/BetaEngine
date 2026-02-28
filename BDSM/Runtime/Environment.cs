using BDSM.Instances;
using BDSM.Runtime.SaveGames;
using BDSM.Tokens;
using BDSM.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BDSM.Runtime;

#nullable disable

public class Environment
{
    private readonly Environment _parentEnv;
    private readonly Dictionary<string, object> _values = [];
    private readonly HashSet<string> _importedFiles = [];

    public Environment()
    {
        _parentEnv = null;
    }
    public Environment(Environment parentEnv)
    {
        _parentEnv = parentEnv;
    }

    public void Define(string name, object @value)
    {
        _values[name] = @value;
    }

    public object Get(Token name)
    {
        if (_values.TryGetValue(name.Lexeme, out var value))
        {
            return value;
        }
        if (_parentEnv != null)
        {
            return _parentEnv.Get(name);
        }

        throw new RuntimeError(name, $"Undefined variable `{name.Lexeme}`");
    }

    public object Get(string name)
    {
        if (_values.TryGetValue(name, out var value))
        {
            return value;
        }
        if (_parentEnv != null)
        {
            return _parentEnv.Get(name);
        }

        throw new RuntimeError(null, $"Undefined variable `{name}`");
    }

    public List<T> Get<T>()
    {
        return _values.Where(v => v.Value is T).Select(v => (T)v.Value).ToList();
    }

    public Dictionary<string, object> GetVars()
    {
        return _values
            .Where(v => v.Value is double or string or bool)
            .ToDictionary(v => v.Key, v => v.Value);
    }

    public bool IsDefined(Token name)
    {
        return _values.ContainsKey(name.Lexeme);
    }

    public void Assign(Token name, object val)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = val;
            return;
        }
        if (_parentEnv != null)
        {
            _parentEnv.Assign(name, val);
            return;
        }

        throw new RuntimeError(name, $"Undefined variable `{name.Lexeme}`");
    }

    public void Assign(string name, object val)
    {
        if (_values.ContainsKey(name))
        {
            _values[name] = val;
            return;
        }
        if (_parentEnv != null)
        {
            _parentEnv.Assign(name, val);
            return;
        }
    }

    public void SetFileImported(string path)
    {
        _importedFiles.Add(path);
    }
    public bool IsFileImported(string path)
    {
        return _importedFiles.Contains(path);
    }

    public void Dump()
    {
        Console.WriteLine($"TOTAL SIZE: {SizeCalculator.GetSizeOf(_values)} bytes");
        foreach (var v in _values)
        {
            Console.WriteLine($"{v.Value.GetType()} | {v.Key} : {v.Value}");
        }
    }

    public void Clear()
    {
        var current = this;
        do
        {
            current._values.Clear();
            current._importedFiles.Clear();
            current = current._parentEnv;
        } while (current != null);
    }


    public BDSMSaveData GetBDSMSaveData()
    {
        // Get variables
        var saveData = new BDSMSaveData()
        {
            Fields = GetFieldSaveData(_values),
        };

        foreach (var val in _values)
        {
            // Get actor save data
            if (val.Value is BDSMActor actor)
            {
                saveData.Actors[actor.DeclName] = new BDSMActorSaveData
                {
                    State = actor.State,
                    Costume = actor.Costume,
                    Fields = GetFieldSaveData(actor.Fields)
                };
            }
            // Get scene save data
            else if (val.Value is BDSMScene scene)
            {
                saveData.Scenes[scene.DeclName] = new BDSMSceneSaveData
                {
                    Fields = GetFieldSaveData(scene.Fields)
                };
            }
        }

        return saveData;
    }

    public void SetBDSMSaveData(BDSMSaveData data)
    {
        SetFieldsFromSaveData(data.Fields);
        foreach (var actorSaveData in data.Actors)
        {
            var actor = (BDSMActor)Get(actorSaveData.Key);
            actor.SetFieldsFromSaveData(actorSaveData.Value.Fields);
            actor.State = actorSaveData.Value.State;
            actor.Costume = actorSaveData.Value.Costume;
        }
        foreach (var sceneSaveData in data.Scenes)
        {
            var scene = (BDSMScene)Get(sceneSaveData.Key);
            scene.SetFieldsFromSaveData(sceneSaveData.Value.Fields);
        }
    }

    private static FieldSaveData GetFieldSaveData(Dictionary<string, object> fields)
    {
        var saveData = new FieldSaveData();
        foreach (var val in fields)
        {
            if (val.Value is bool b)
            {
                saveData.BoolFields[val.Key] = b;
            }
            else if (val.Value is string s)
            {
                saveData.StringFields[val.Key] = s;
            }
            else if (val.Value is double d)
            {
                saveData.DoubleFields[val.Key] = d;
            }
        }

        return saveData;
    }

    private void SetFieldsFromSaveData(
        FieldSaveData fieldData
    )
    {
        foreach (var b in fieldData.BoolFields)
        {
            _values[b.Key] = b.Value;
        }
        foreach (var s in fieldData.StringFields)
        {
            _values[s.Key] = s.Value;
        }
        foreach (var d in fieldData.DoubleFields)
        {
            _values[d.Key] = d.Value;
        }
    }
}