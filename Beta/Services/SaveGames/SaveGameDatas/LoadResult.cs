using Beta.Scenes;
using Beta.Actors;

namespace Beta.Services.SaveGames.SaveGameDatas;

public record LoadResult
{
    public required Actor Player { get; init; }
    public required Scene CurrentScene { get; init; }
}