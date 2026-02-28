using Beta.CommandManagement;
using Beta.DI;
using Beta.Entities;
using Beta.Input;
using Beta.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Beta.GameStates;

public class GameStateCinematic : GameState
{
    private RectangleF _top;
    private RectangleF _bottom;

    private float _dist;
    private readonly float _maxDist = 65f;
    private readonly float _speed = 50f;
    private readonly OrthographicCamera _camera;
    private readonly CommandManager _commandManager;
    private readonly EntityManager _entityManager;
    private readonly SceneManager _sceneManager;

    public override string Name => nameof(GameStateCinematic);

    public GameStateCinematic(GameStateManager manager) : base(manager)
    {
        _camera = DependencyContainer.Instance.Get<OrthographicCamera>();

        _commandManager = DependencyContainer.Instance.Get<CommandManager>();
        _entityManager = DependencyContainer.Instance.Get<EntityManager>();
        _sceneManager = DependencyContainer.Instance.Get<SceneManager>();

        _top.Width = _bottom.Width = _camera.BoundingRectangle.Width;
        _top.Height = _bottom.Height = _maxDist;
    }

    public override void Update(GameTime gameTime)
    {
        _dist += _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_dist > _maxDist)
        {
            _dist = _maxDist;
        }

        _top.Y = -_maxDist + _dist;
        _bottom.Y = _camera.BoundingRectangle.Bottom - _dist;

        _top.Y += _camera.Position.Y;
        _bottom.Y += _camera.Position.Y;
        _top.X += _camera.Position.X;
        _bottom.X += _camera.Position.X;

        _sceneManager.Update(gameTime);
        _commandManager.Update(gameTime);
        _entityManager.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(_top, Color.Black, Constants.LayerDepthGui);
        spriteBatch.FillRectangle(_bottom, Color.Black, Constants.LayerDepthGui);

        _sceneManager.Draw(spriteBatch);
        _commandManager.Draw(spriteBatch);
    }

    private void Skip()
    {
        _commandManager.SkipFirst();
    }

    public override InputEventConsumeResult OnInputEvent(InputEventArgs args)
    {
        if (InputMapper.IsMatch(args, InputMapping.GameInputType.CursorMainAction))
        {
            Skip();
        }
        if (InputMapper.IsMatch(args, InputMapping.GameInputType.CursorMainActionAtPosition))
        {
            Skip();
        }

        return new();
    }
}