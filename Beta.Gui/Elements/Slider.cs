using Beta.Gui.Events;
using Beta.Gui.Styles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Globalization;

namespace Beta.Gui.Elements;

public class Slider : GuiElement
{
    private readonly Label _startLabel;
    private readonly Label _endLabel;

    private int _value;
    public int Value
    {
        get => _value;
        set
        {
            _value = value;
            _endLabel.Text = Value.ToString(CultureInfo.InvariantCulture);
        }
    }

    private readonly int _min;
    private readonly int _max;
    private readonly Label _textLabel;
    private Vector2 _pivotPos;
    private Vector2 _lineEnd;
    private Vector2 _lineStart;

    public Slider(string text, int min, int max, int initValue, GuiElementStyle style, string? extraInputContexts) : base(style, extraInputContexts)
    {
        _min = min;
        _max = max;

        _textLabel = new Label(text, style with
        {
            RelativePosition = new Vector2(0, 0)
        }, extraInputContexts);

        float padding = 10f;
        _startLabel = new Label(min.ToString(CultureInfo.InvariantCulture), style with
        {
            RelativePosition = new Vector2(0, style.Size.Height - _textLabel.Style.Size.Height - padding)
        }, extraInputContexts);

        _endLabel = new Label(min.ToString(CultureInfo.InvariantCulture), style with
        {
            RelativePosition = new Vector2(style.Size.Width, style.Size.Height - _textLabel.Style.Size.Height - padding)
        }, extraInputContexts);
        Value = initValue;
        _endLabel.Text = Value.ToString(CultureInfo.InvariantCulture);

        SubscribeOnLeftPress((_, args) =>
        {
            SetPivot(args.Position.X);
        });
        SubscribeOnLeftClick((_, args) =>
        {
            SetPivot(args.Position.X);
            Gui.Instance.PlaySound(GuiSoundType.Slide);
        });
        SubscribeOnDrag((_, args) =>
        {
            SetPivot(args.Position.X);
        });
        SubscribeOnScroll((_, args) =>
        {
            const int step = 5;
            if (args.ScrollWheelDiff > 0)
            {
                if (Value - step < _min)
                {
                    return;
                }
                Value -= step;
            }
            else
            {
                if (Value + step > _max)
                {
                    return;
                }
                Value += step;
            }
            GuiHandlerRegistry.InvokeIntValueHandlers(GuiEventType.SliderValueChanged, ElemId, Value);
        });

        Update(new GameTime());

        AddElement(_textLabel);
        AddElement(_startLabel);
        AddElement(_endLabel);
    }

    public void SetPivot(float x)
    {
        var pos = new Vector2(x, _lineStart.Y);
        if (pos.X >= _lineStart.X || pos.X <= _lineEnd.X)
        {
            _pivotPos = pos;
            var t = (_pivotPos.X - _lineStart.X) / (_lineEnd.X - _lineStart.X);
            t = t > 1 ? 1 : t;
            t = t < 0 ? 0 : t;

            Value = (int)(t * (_max - _min));
            Value -= Value % 5;

            GuiHandlerRegistry.InvokeIntValueHandlers(GuiEventType.SliderValueChanged, ElemId, Value);
            _endLabel.Text = Value.ToString(CultureInfo.InvariantCulture);
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        if (Style.Hidden)
        {
            return;
        }

        const int margin = 20;
        _lineStart = _startLabel.GetAbsolutePosition() + new Vector2(_startLabel.Style.Size.Width + margin, _startLabel.Style.Size.Height / 2);
        _lineEnd = _endLabel.GetAbsolutePosition() + new Vector2(-margin, _startLabel.Style.Size.Height / 2);

        var t = (float)Value / (_max - _min);
        t = t > 1 ? 1 : t;
        t = t < 0 ? 0 : t;
        _pivotPos = _lineStart + t * (_lineEnd - _lineStart);

        spriteBatch.DrawLine(_lineStart, _lineEnd, Style.Color, 3, Style.LayerDepth);
        var fivePercent = (_lineStart - _lineEnd).Length() / 100f * 5f;
        for (var i = 0; i <= 20; ++i)
        {
            spriteBatch.DrawLine(
                _lineStart + new Vector2(i * fivePercent, -4),
                _lineStart + new Vector2(i * fivePercent, 4),
                Style.Color,
                2,
                Style.LayerDepth
            );
        }

        spriteBatch.DrawCircle(new CircleF(_pivotPos, 10), 6, Style.Color, 10, Style.LayerDepth);
    }

    public override string ToString()
    {
        return $"{nameof(Slider)} .{ElemClass} #{ElemId}";
    }
}