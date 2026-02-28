using Beta.Common;
using Beta.DI;
using Beta.Input;
using Beta.InputMapping;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beta.GameStates;

public class GameStateOverlay : GameState
{
    private readonly ContentCache _contentCache;
    private readonly Texture2D _texture;

    public override string Name => nameof(GameStateOverlay);

    public GameStateOverlay(GameStateManager manager, string name) : base(manager)
    {
        _contentCache = DependencyContainer.Instance.Get<ContentCache>();
        _texture = _contentCache.Get<Texture2D>($"img/closeups/{name}");
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _texture,
            Vector2.Zero,
            sourceRectangle: _texture.Bounds,
            color: Color.White,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: Vector2.One,
            effects: SpriteEffects.None,
            layerDepth: Constants.LayerDepthCloseup
        );
    }

    public override void Update(GameTime gameTime)
    {

    }

    private void OnCancel(InputEventArgs e)
    {
        Manager.RequestStatePlaying();
    }

    public override InputEventConsumeResult OnInputEvent(InputEventArgs args)
    {
        if (InputMapper.IsMatch(args, GameInputType.OverlayCancel))
        {
            OnCancel(args);
        }

        return new();
    }
}
