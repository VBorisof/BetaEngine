using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace aced;

public static class BuildTools
{
    public static void WriteStartupScript(string sceneName)
    {
        // Generate imports.
        List<string> imports = [];
        imports.AddRange(MakeImports("actors"));
        imports.AddRange(MakeImports("scenes"));
        imports.AddRange(MakeImports("cinematics"));
        imports.Add("import \"posthook.bs\"");
        imports.Add("import \"default-handlers.bs\"");

        var importStrings = string.Join("\n", imports);

        File.WriteAllText(Settings.IMPORTS_SCRIPT_PATH, importStrings);

        // Generate the launch script.
        var launchScriptTemplate = File.ReadAllText("Templates/template_launch_script.txt");
        launchScriptTemplate = launchScriptTemplate.Replace("$scene", sceneName);

        File.WriteAllText(Settings.LAUNCH_SCRIPT_PATH, launchScriptTemplate);
    }

    private static IEnumerable<string> MakeImports(string directory)
    {
        var fileNames = Directory.GetFiles($"{Settings.SCRIPTS_BASE_PATH}/{directory}/");
        var imports = fileNames.Select(f => $"import \"{directory}/{f.Split('/').Last()}\"");

        return imports;
    }
}