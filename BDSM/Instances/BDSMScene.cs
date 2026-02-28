using System.Collections.Generic;
using BDSM.Language;
using BDSM.StandardLibrary;

#nullable disable

namespace BDSM.Instances;

public class BDSMScene : GameInstance
{
    public List<BDSMRegion> Regions { get; }
    public List<PropStatement> Props { get; }

    public BDSMScene(string declName, List<BDSMRegion> regions, List<PropStatement> props)
        : base(declName, [])
    {
        AddField("timesEntered", 0.0);
        AddMethod("add", new SceneAddMethod(this));
        AddMethod("remove", new SceneRemoveMethod(this));
        Regions = regions;
        foreach (var region in Regions)
        {
            AddField(region.DeclName, region);
        }

        Props = props;
        foreach (var prop in Props)
        {
            AddField(prop.declName.Lexeme, prop);
        }
    }

    public override string ToString()
    {
        var str = $"scene {DeclName} : \n";
        str += "  regions:\n";
        foreach (var region in Regions)
        {
            str += $"    {region.DeclName}={region}\n";
        }
        str += "  fields:\n";
        foreach (var field in Fields)
        {
            str += $"    {field.Key}={field.Value}\n";
        }

        return str;
    }
}