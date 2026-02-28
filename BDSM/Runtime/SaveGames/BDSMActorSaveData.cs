namespace BDSM.Runtime.SaveGames;

public record BDSMActorSaveData
{
    public required FieldSaveData Fields { get; init; }
    public required string State { get; init; }
    public required string Costume { get; init; }
}