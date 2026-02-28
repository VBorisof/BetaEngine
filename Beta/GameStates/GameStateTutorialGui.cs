using Beta.DI;
using Beta.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Beta.Scenes;

namespace Beta.GameStates;

public class GameStateTutorialGui : GameState
{
    private readonly SceneManager _sceneManager;

    public GameStateTutorialGui(GameStateManager manager) : base(manager)
    {
        _sceneManager = DependencyContainer.Instance.Get<SceneManager>();
    }

    public override string Name => nameof(GameStateTutorialGui);


    public override void Update(GameTime gameTime)
    {
        Gui.Gui.Instance.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _sceneManager.Draw(spriteBatch);
        Gui.Gui.Instance.Draw(spriteBatch);
    }

    public override InputEventConsumeResult OnInputEvent(InputEventArgs args)
    {
        return new();
    }
}
