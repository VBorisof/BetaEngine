namespace BDSM.Runtime.SaveGames;

public record BDSMSceneSaveData
{
    public required FieldSaveData Fields { get; init; }
}