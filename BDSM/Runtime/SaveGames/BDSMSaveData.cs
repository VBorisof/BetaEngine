using System.Collections.Generic;

namespace BDSM.Runtime.SaveGames;

public record BDSMSaveData
{
    public Dictionary<string, BDSMActorSaveData> Actors { get; set; } = [];
    public Dictionary<string, BDSMSceneSaveData> Scenes { get; set; } = [];
    public required FieldSaveData Fields { get; init; } = new();
}