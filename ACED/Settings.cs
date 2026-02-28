using System.IO;
using System;

namespace aced;

public static class Settings
{
    public static string PROJECT_BASE_PATH { get; } = Directory.GetParent(Environment.CurrentDirectory).FullName;

    // TODO: Move this to configs...
    // Should come from a .json file or smth.
    public static string CONTENT_BASE_PATH { get; } = $"{PROJECT_BASE_PATH}/PROJECT/CONTENT";
    public static string SCRIPTS_BASE_PATH { get; } = $"{CONTENT_BASE_PATH}/scripts";
    public static string LAUNCH_SCRIPT_PATH { get; } = $"{SCRIPTS_BASE_PATH}/aced-launch.bs";
    public static string IMPORTS_SCRIPT_PATH { get; } = $"{SCRIPTS_BASE_PATH}/imports.bs";

    public static string ASSET_BASE_PATH { get; } = $"{CONTENT_BASE_PATH}";
    public static string JSON_RES_BASE_PATH { get; } = $"{CONTENT_BASE_PATH}/resources";

    public static string GAME_PWD { get; } = $"{PROJECT_BASE_PATH}/PROJECT";
    public static string AiDS_PWD { get; } = $"{PROJECT_BASE_PATH}/AiDS";

    public static string GAME_BUILD { get; } = $"{GAME_PWD}/PROJECT/BINARY";
    public static string AiDS_START { get; } = $"{PROJECT_BASE_PATH}/AiDS/run.sh";

    public static bool IsDryRun { get; set; }

    public static bool IsDrawWalkableAreas { get; set; } = true;
    public static bool IsDrawExits { get; set; } = true;
    public static bool IsDrawActors { get; set; } = true;
    public static bool IsDrawLights { get; set; } = true;
    public static bool IsDrawScaleMap { get; set; } = true;
    public static bool IsDrawRegions { get; set; } = true;
    public static bool IsDrawProps { get; set; } = true;
    public static bool IsDrawWalkbehinds { get; set; } = true;
}