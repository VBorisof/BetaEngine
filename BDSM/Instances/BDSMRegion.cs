#nullable disable

namespace BDSM.Instances;

public class BDSMRegion : Instance
{
    public string DeclName { get; }
    public static string IsEnteredFieldName { get; } = "isEntered";

    public BDSMRegion(string declName)
    {
        DeclName = declName;

        AddField(IsEnteredFieldName, false);
    }

    public override string ToString()
    {
        var str = $"region {DeclName} : \n";
        str += "\n  fields:\n";
        foreach (var field in Fields)
        {
            str += $"    {field.Key}={field.Value}\n";
        }

        return str;
    }
}