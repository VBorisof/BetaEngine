using Beta.Gui.Elements;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;

namespace Beta.Gui.Behaviors;

public class GuiElementSlideBehavior : GuiElementBehavior
{
    private readonly Vector2 _target;
    private readonly Vector2 _direction;

    private readonly float _speed = 1000f;

    public GuiElementSlideBehavior(GuiElement guiElement, Vector2 target) : base(guiElement)
    {
        _target = target;
        _direction = target - guiElement.GetAbsolutePosition();
        _direction.Normalize();
        if (_direction.IsNaN())
        {
            IsDone = true;
            OnDone();
        }
    }

    public override void Update(GameTime gameTime)
    {
        var tolerance = 25;
        var dist = GuiElement.GetAbsolutePosition() - _target;
        var distLenSquared = dist.LengthSquared();

        if (dist.LengthSquared() <= tolerance * tolerance)
        {
            IsDone = true;
            OnDone();
            return;
        }

        var dynamicSpeed =
            Math.Min(
                _speed,
                distLenSquared / (_speed * (float)gameTime.ElapsedGameTime.TotalSeconds));

        GuiElement.Style.RelativePosition +=
            _direction * dynamicSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}