using Beta.DI;
using Beta.Input;
using Beta.InputMapping;
using Beta.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Beta.GameStates;

public abstract class GameState : IInputEventListener
{
    private readonly InputContextManager _inputContextManager;
    protected GameStateManager Manager { get; }
    protected ILogger Logger { get; }
    public InputMapper InputMapper { get; }

    private readonly InputContext _context;

    public abstract string Name { get; }

    public GameState(GameStateManager manager)
    {
        Manager = manager;
        InputMapper = DependencyContainer.Instance.Get<InputMapper>();
        Logger = DependencyContainer.Instance.Get<ILogger>();

        _inputContextManager = DependencyContainer.Instance.Get<InputContextManager>();
        _context = _inputContextManager.GetOrCreateByName(Name);
    }


    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch);

    public virtual HashSet<InputContext> GetInputContexts()
    {
        return [ _context ];
    }

    public abstract InputEventConsumeResult OnInputEvent(InputEventArgs args);
    public virtual void Reset() { }
}