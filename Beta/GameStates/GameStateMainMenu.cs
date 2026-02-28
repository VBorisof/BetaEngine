using Beta.DI;
using Beta.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Beta.Scenes;
using System;
using System.Collections.Generic;

namespace Beta.GameStates;

public class GameStateMainMenu : GameState
{
    private readonly SceneManager _sceneManager;
    private readonly InputContextManager _inputContextsManager;

    public override string Name => nameof(GameStateMainMenu);

    public event EventHandler MainMenuToggle = (_, _) => { };

    public GameStateMainMenu(GameStateManager manager) : base(manager)
    {
        _sceneManager = DependencyContainer.Instance.Get<SceneManager>();
        _inputContextsManager = DependencyContainer.Instance.Get<InputContextManager>();
    }

    public override void Update(GameTime gameTime)
    {
        Gui.Gui.Instance.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _sceneManager.Draw(spriteBatch);

        Gui.Gui.Instance.Draw(spriteBatch);
    }

    private void OnMainMenuToggle()
    {
        MainMenuToggle.Invoke(this, EventArgs.Empty);
    }

    private void OnMainMenuConfirm()
    {
        Logger.Debug("");
    }

    public override HashSet<InputContext> GetInputContexts()
    {
        var contexts = base.GetInputContexts();
        contexts.Add(_inputContextsManager.GetOrCreateByName(nameof(Gui.Gui)));

        return contexts;
    }

    public override InputEventConsumeResult OnInputEvent(InputEventArgs args)
    {
        if (InputMapper.IsMatch(args, InputMapping.GameInputType.MainMenuConfirm))
        {
            OnMainMenuConfirm();
        }
        if (InputMapper.IsMatch(args, InputMapping.GameInputType.ToggleMainMenu))
        {
            OnMainMenuToggle();
        }

        return new();
    }
}