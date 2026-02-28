using Beta.AdditionalUi;
using Beta.BDSM;
using Beta.CommandManagement;
using Beta.DI;
using Beta.Entities;
using Beta.Input;
using Beta.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beta.GameStates;

public class GameStateStartMenu : GameState
{
    private readonly CommandManager _commandManager;
    private readonly EntityManager _entityManager;
    private readonly SceneManager _sceneManager;
    private readonly BDSMAdapter _bdsmAdapter;
    private readonly AdditionalUiManager _additionalUiManager;
    public override string Name => nameof(GameStateStartMenu);

    public GameStateStartMenu(GameStateManager manager) : base(manager)
    {
        _additionalUiManager = DependencyContainer.Instance.Get<AdditionalUiManager>();
        _bdsmAdapter = DependencyContainer.Instance.Get<BDSMAdapter>();
        _sceneManager = DependencyContainer.Instance.Get<SceneManager>();
        _entityManager = DependencyContainer.Instance.Get<EntityManager>();
        _commandManager = DependencyContainer.Instance.Get<CommandManager>();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Gui.Gui.Instance.Draw(spriteBatch);
        _commandManager.Draw(spriteBatch);
        _sceneManager.Draw(spriteBatch);
        _additionalUiManager.Draw(spriteBatch);
    }

    public override void Update(GameTime gameTime)
    {
        Gui.Gui.Instance.Update(gameTime);

        _additionalUiManager.Update(gameTime);
        _commandManager.Update(gameTime);
        _entityManager.Update(gameTime);
        _sceneManager.Update(gameTime);
        _bdsmAdapter.Update(gameTime);
    }

    public override InputEventConsumeResult OnInputEvent(InputEventArgs args)
    {
        return new();
    }
}