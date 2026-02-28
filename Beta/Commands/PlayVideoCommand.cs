using Beta.DI;
using Beta.GameStates;
using Beta.Logging;
using Beta.Videos;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beta.Commands;

public class PlayVideoCommand : Command
{
    public string Name { get; }

    private readonly VideoPlayer _videoPlayer;
    private readonly GameStateManager _gameStateManager;
    private readonly ILogger _logger;

    public PlayVideoCommand(string name)
    {
        Name = name;

        _videoPlayer = DependencyContainer.Instance.Get<VideoPlayer>();
        _gameStateManager = DependencyContainer.Instance.Get<GameStateManager>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
    }

    public override void Startup()
    {
        // TODO: More command params.
        _videoPlayer.StartFadeVideo(
            Name,
            fadeInSpeed: 0.6f,
            fadeOutSpeed: 0.5f,
            frameDuration: 1500f);
        _gameStateManager.RequestStateVideo();
    }

    public override bool Update(GameTime gameTime)
    {
        _videoPlayer.Update(gameTime);
        IsDone = !_videoPlayer.IsPlaying;

        return IsDone;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _videoPlayer.Draw(spriteBatch);
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
