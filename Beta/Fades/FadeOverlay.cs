using System;
using Beta.DI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Beta.Fades;

public class FadeOverlay
{
    private readonly OrthographicCamera _camera;

    public FadeState State { get; set; } = FadeState.None;
    private float Alpha { get; set; }
    public float Speed { get; set; } = 1f;

    public EventHandler OnComplete { get; set; } = (_, _) => { };

    public FadeOverlay()
    {
        _camera = DependencyContainer.Instance.Get<OrthographicCamera>();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (Settings.IsDebug)
        {
            return;
        }

        if (Alpha > 0)
        {
            spriteBatch.FillRectangle(
                _camera.BoundingRectangle,
                new Color(0, 0, 0, Alpha),
                layerDepth: Constants.LayerDepthFade
            );
        }
    }

    public void Update(GameTime time)
    {
        switch (State)
        {
            case FadeState.None:
                return;
            case FadeState.FadeIn:
                Alpha -= Speed * time.GetElapsedSeconds();
                if (Alpha <= 0)
                {
                    Alpha = 0;
                    State = FadeState.None;
                    OnComplete(this, EventArgs.Empty);
                }
                break;
            case FadeState.FadeOut:
                Alpha += Speed * time.GetElapsedSeconds();
                if (Alpha >= 1)
                {
                    Alpha = 1;
                    State = FadeState.None;
                    OnComplete(this, EventArgs.Empty);
                }
                break;
        }
    }

    public void CompleteFade()
    {
        Alpha = 1f;
    }
    public void Remove()
    {
        Alpha = 0f;
    }
}