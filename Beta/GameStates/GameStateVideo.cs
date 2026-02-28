using Beta.CommandManagement;
using Beta.DI;
using Beta.Input;
using Beta.Videos;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beta.GameStates;

public class GameStateVideo : GameState
{
    private readonly VideoPlayer _videoPlayer;
    private readonly CommandManager _commandManager;

    public override string Name => nameof(GameStateVideo);

    public GameStateVideo(GameStateManager manager) : base(manager)
    {
        _commandManager = DependencyContainer.Instance.Get<CommandManager>();
        _videoPlayer = DependencyContainer.Instance.Get<VideoPlayer>();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _videoPlayer.Draw(spriteBatch);
        _commandManager.Draw(spriteBatch);
    }

    public override void Update(GameTime gameTime)
    {
        _videoPlayer.Update(gameTime);
        _commandManager.Update(gameTime);
    }

    public override InputEventConsumeResult OnInputEvent(InputEventArgs args)
    {
        return new();
    }
}