namespace Beta.ContentTools;

public interface IContentPathProvider
{
    string ProvideDialoguePath(string actorName);
    string ProvideEntityPath(string entityName);

    string ProvideSceneTexturePath(string sceneName);
    string ProvideScaleMapPath(string sceneName);
    string ProvideDepthMapPath(string sceneName);
    string ProvideSceneMetaPath(string sceneName);

    string ProvideTutorialPath();
    string ProvideScriptsPath();
    string ProvideLayoutsPath();
    string ProvideSaveGamePath();
}