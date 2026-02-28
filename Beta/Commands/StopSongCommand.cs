using Beta.DI;
using Beta.Logging;
using Beta.Services.Sounds;
using Microsoft.Xna.Framework;

namespace Beta.Commands;

public class StopSongCommand : Command
{
    private readonly MusicPlayerService _musicPlayer;
    private readonly ILogger _logger;

    public StopSongCommand()
    {
        _musicPlayer = DependencyContainer.Instance.Get<MusicPlayerService>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
    }

    public override void Startup()
    {
        _musicPlayer.StopWithFade();
        IsDone = true;
    }

    public override bool Update(GameTime gameTime)
    {
        return IsDone;
    }

    public override void OnComplete()
    {
        base.OnComplete();
        _logger.Debug("");
    }

    public override void OnInterrupt()
    {
        base.OnInterrupt();
        _logger.Debug("");
    }
}