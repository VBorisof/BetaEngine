using Beta.CommandManagement;
using Beta.DI;
using Beta.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beta.GameStates;

public class GameStateLoading : GameState
{
    private readonly CommandManager _commandManager;

    public override string Name => nameof(GameStateLoading);

    public GameStateLoading(GameStateManager manager) : base(manager)
    {
        _commandManager = DependencyContainer.Instance.Get<CommandManager>();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _commandManager.Draw(spriteBatch);
    }

    public override void Update(GameTime gameTime)
    {
        _commandManager.Update(gameTime);
    }

    public override InputEventConsumeResult OnInputEvent(InputEventArgs args)
    {
        return new();
    }
}