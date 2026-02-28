using System;
using Beta.DI;
using Beta.Fades;
using Beta.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beta.Videos;

public class RollVideo : Video
{
    private readonly Texture2D _frame;
    private readonly float _speed;
    private FadeOverlay _fade;

    private readonly ILogger _logger;

    public float Speed { get; }
    public TimeSpan FadeInDuration { get; }

    // FadeIn -> Roll
    private RollVideoState _state;

    private Vector2 _position = Vector2.Zero;

    public RollVideo(
        float speed,
        TimeSpan fadeInDuration,
        Texture2D frame
    )
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();

        _speed = speed;
        _frame = frame;
        _fade = new FadeOverlay();
        _fade.CompleteFade();
        _fade.State = FadeState.FadeIn;
        FadeInDuration = fadeInDuration;
        _fade.Speed = FadeInDuration.Seconds;

        _state = RollVideoState.FadeIn;
    }

    public override void Update(GameTime gameTime)
    {
        _fade.Update(gameTime);
        _logger.Debug($"RollVideo... {_state}");

        if (_state == RollVideoState.FadeIn)
        {
            if (_fade.State == FadeState.None)
            {
                _state = RollVideoState.Roll;
            }
        }
        if (_state == RollVideoState.Roll)
        {
            _position -= new Vector2(
                0,
                (float)(_speed * gameTime.ElapsedGameTime.TotalMilliseconds)
            );

            if (_position.Y <= -_frame.Height)
            {
                IsDone = true;
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _fade.Draw(spriteBatch);
        spriteBatch.Draw(
            _frame,
            sourceRectangle: new Rectangle(0, 0, _frame.Width, _frame.Height),
            position: _position,
            effects: SpriteEffects.None,
            rotation: 0,
            origin: Vector2.Zero,
            color: Color.White,
            scale: Vector2.One,
            layerDepth: Constants.LayerDepthGui
        );
    }

    public override void Reset()
    {
        base.Reset();
        _fade = new FadeOverlay();
        _fade.CompleteFade();
        _fade.State = FadeState.FadeIn;
        _fade.Speed = FadeInDuration.Seconds;

        _state = RollVideoState.FadeIn;
        _position = Vector2.Zero;
    }
}