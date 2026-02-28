using Beta.Gui.Styles;
using Beta.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Beta.Gui.Elements;

public class Label : GuiElement
{
    private string _text = "";
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            if (Gui.Instance.TextManager is null || Gui.Instance.MainFont is null)
            {
                return;
            }
            Style.Size = Gui.Instance.TextManager.MeasureString(_text, Gui.Instance.MainFont);
        }
    }

    public Label(string text, GuiElementStyle style, string? extraInputContexts) : base(style, extraInputContexts)
    {
        Text = text;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        if (Style.Hidden)
        {
            return;
        }

        if (Gui.Instance.TextManager is null || Gui.Instance.MainFont is null)
        {
            return;
        }

        Gui.Instance.TextManager.WriteLine(
            spriteBatch,
            Text,
            new TextWriteArgs
            {
                FontBinding = Gui.Instance.MainFont,
                Position = GetAbsolutePosition(),
                Color = Style.TextColor,
                // Fix: OutlineColor = Style.OutlineColor,
                LayerDepth = Style.LayerDepth,
            }
        );
    }

    public override string ToString()
    {
        return $"{nameof(Label)} .{ElemClass} #{ElemId}";
    }
}