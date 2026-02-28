using System.Collections.Generic;
using BDSM.Language;
using BDSM.StandardLibrary;

#nullable disable

namespace BDSM.Instances;

public class BDSMActor : GameInstance
{
    public string State { get; set; } = "idle";
    public string Costume { get; set; } = "default";

    public BDSMActor(string declname, List<VerbStatement> verbs)
        : base(declname, verbs)
    {
        AddMethod("say", new ActorSayMethod(this));
        AddMethod("pickup", new ItemPickupMethod(this));
        AddMethod("removeitem", new PlayerItemRemoveMethod(this));
        AddMethod("additem", new PlayerItemAddMethod(this));
        AddMethod("talk", new ActorTalkToMethod(this));
        AddMethod("move", new ActorMoveMethod(this));
        AddMethod("put", new ActorPutMethod(this));
        AddMethod("interrupt", new ActorInterruptMethod(this));
        AddMethod("setstate", new SetStateMethod(this));
        AddMethod("getstate", new GetStateMethod(this));
        AddMethod("setcostume", new SetCostumeMethod(this));
        AddMethod("clearinventory", new ClearInventoryMethod(this));
        AddMethod("setIsShowChildren", new ActorSetIsShowChildrenMethod(this));
    }

    public override string ToString()
    {
        var str = $"actor {DeclName} : \n";
        str += "\n  fields:\n";
        foreach (var field in Fields)
        {
            str += $"    {field.Key}={field.Value}\n";
        }

        return str;
    }
}