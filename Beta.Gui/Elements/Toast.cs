using Beta.Gui.Styles;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;

namespace Beta.Gui.Elements;

public class Toast : GuiElement
{
    private readonly Box _box;
    private readonly Label _label;
    private readonly TextBox _textBox;

    private readonly TimeSpan? _duration;
    private TimeSpan _elapsed = TimeSpan.Zero;

    public event EventHandler Done = (_, _) => { };

    public Toast(GuiElementStyle style, string title, string text, TimeSpan? duration, string? extraInputContexts) : base(style, extraInputContexts)
    {
        _box = new Box(style with { RelativePosition = Vector2.Zero }, extraInputContexts);

        _label = new Label(
            title,
            style with
            {
                RelativePosition = new Vector2(10, 10),
                LayerDepth = style.LayerDepth + Constants.LayerDepthStep
            }
            , extraInputContexts);
        _box.AddElement(_label);

        _textBox = new TextBox(
            text,
            style with
            {
                RelativePosition = new Vector2(0, 40),
                LayerDepth = style.LayerDepth + Constants.LayerDepthStep,
                Size = new SizeF(style.Size.Width - 20, style.Size.Height - 40),
                BorderColor = Color.Transparent
            }
            , extraInputContexts);
        _box.AddElement(_textBox);

        AddElement(_box);
        _duration = duration;

        SubscribeOnLeftClick((_, _) =>
        {
            Done.Invoke(this, EventArgs.Empty);
        });
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (Style.Hidden)
        {
            return;
        }

        if (_duration is not null)
        {
            _elapsed += gameTime.ElapsedGameTime;
            if (_elapsed >= _duration)
            {
                Done.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public override string ToString()
    {
        return $"{nameof(Toast)} .{ElemClass} #{ElemId}";
    }
}