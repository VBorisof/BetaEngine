using System;
using Beta.DI;
using Beta.Fades;
using Beta.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beta.Videos;

public class FadeVideo : Video
{
    private readonly Texture2D _frame;
    private readonly FadeOverlay _fade;

    private TimeSpan _timePassed = TimeSpan.Zero;
    private readonly ILogger _logger;

    public float FadeInSpeed { get; }
    public float FadeOutSpeed { get; }
    public float FrameDuration { get; }

    // FadeIn -> Show Frame -> FadeOut
    private FadeVideoState _state = FadeVideoState.FadeIn;

    public FadeVideo(
        float fadeInDuration,
        float fadeOutDuration,
        float frameDuration,
        Texture2D frame
    )
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();

        FadeInSpeed = fadeInDuration;
        FadeOutSpeed = fadeOutDuration;
        FrameDuration = frameDuration;
        _frame = frame;
        _fade = new FadeOverlay();
        _fade.CompleteFade();
        _fade.State = FadeState.FadeIn;
        _state = FadeVideoState.FadeIn;
        _fade.Speed = FadeInSpeed;
    }

    public override void Update(GameTime gameTime)
    {
        _logger.Trace($"Video State: {_state}");
        _fade.Update(gameTime);

        if (_fade.State == FadeState.None)
        {
            if (_state == FadeVideoState.EndFadeOut)
            {
                Reset();
                IsDone = true;
                return;
            }

            if (_state == FadeVideoState.FadeIn)
            {
                _state = FadeVideoState.ShowFrame;
            }

            if (_state == FadeVideoState.ShowFrame)
            {
                _timePassed += gameTime.ElapsedGameTime;
                _logger.Trace($"Showing frame...{_timePassed}/{FrameDuration}");
                if (_timePassed.TotalMilliseconds >= FrameDuration)
                {
                    _state = FadeVideoState.EndFadeOut;
                    _fade.State = FadeState.FadeOut;
                    _fade.Speed = FadeOutSpeed;
                }
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _fade.Draw(spriteBatch);
        spriteBatch.Draw(
            _frame,
            sourceRectangle: new Rectangle(0, 0, _frame.Width, _frame.Height),
            position: Vector2.Zero,
            effects: SpriteEffects.None,
            rotation: 0,
            origin: Vector2.Zero,
            color: Color.White,
            scale: Vector2.One,
            layerDepth: Constants.LayerDepthVideo
        );
    }

    public override void Reset()
    {
        base.Reset();
        _fade.CompleteFade();
        _fade.State = FadeState.FadeIn;
        _state = FadeVideoState.FadeIn;
        _fade.Speed = FadeInSpeed;
        _timePassed = TimeSpan.Zero;
    }
}