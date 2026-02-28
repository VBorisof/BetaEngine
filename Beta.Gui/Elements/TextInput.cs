using System;
using System.Linq;
using Beta.Gui.Events;
using Beta.Gui.Styles;
using Beta.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Beta.Gui.Elements;

[Flags]
public enum AllowedTextInput
{
    Numbers = 0,
    Letters = 1,
    Comma = 2,
}

public class TextInputSubmitEventArgs
{
    public string Text { get; private set; }

    public TextInputSubmitEventArgs(string text)
    {
        Text = text;
    }
}

public class TextInput : GuiElement
{
    public string Text { get; set; } = "";
    public string Hint { get; set; } = "";

    public bool IsActive { get; set; }
    public bool IsError { get; set; }

    public AllowedTextInput AllowedTextInputFlags { get; set; }

    private readonly Box _box;

    public TextInput(
        string value,
        string hint,
        AllowedTextInput allowedTextInputFlags,
        GuiElementStyle style,
        string? extraInputContexts
    ) : base(style, extraInputContexts)
    {
        Hint = hint;
        AllowedTextInputFlags = allowedTextInputFlags;
        _box = new Box(style with
        {
            RelativePosition = new Vector2(0, 0)
        }, extraInputContexts);

        _box.SubscribeOnLeftClick((_, __) =>
        {
            if (!IsActive)
            {
                IsActive = true;
                SubscribeOnKeyHit(OnKeyHit);
                // TODO: Must lock...
            }
        });
        AddElement(_box);
    }

    public void OnKeyHit(object? sender, Keys key)
    {
        if (key == Keys.Back)
        {
            if (!string.IsNullOrEmpty(Text))
            {
                Text = Text[..^1];
                return;
            }
        }
        if (key == Keys.Enter)
        {
            UnsubscribeOnKeyHit(OnKeyHit);
            IsActive = false;
            GuiHandlerRegistry.InvokeStringValueHandlers(GuiEventType.TextInputValueChanged, ElemId, Text);
        }
        if (AllowedTextInputFlags.HasFlag(AllowedTextInput.Letters))
        {
            if (key is >= Keys.A and <= Keys.Z)
            {
                Text += key.ToString().ToLowerInvariant();
            }
            if (key == Keys.OemMinus)
            {
                Text += '_';
            }
            if (key == Keys.Space)
            {
                Text += ' ';
            }
        }
        if (AllowedTextInputFlags.HasFlag(AllowedTextInput.Numbers))
        {
            if (key is >= Keys.D0 and <= Keys.D9)
            {
                Text += key.ToString().Last();
            }
            if (key == Keys.OemPeriod)
            {
                Text += '.';
            }
        }
        if (AllowedTextInputFlags.HasFlag(AllowedTextInput.Comma))
        {
            if (key == Keys.OemComma)
            {
                Text += ',';
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        if (Style.Hidden)
        {
            return;
        }

        if (Gui.Instance.MainFont is null)
        {
            Gui.Instance.Logger?.Error("No GUI font!");
            return;
        }

        var text = string.IsNullOrEmpty(Text) ? Hint : Text + (IsActive ? "|" : "");
        Gui.Instance.TextManager?.WriteLine(
            spriteBatch,
            text,
            new TextWriteArgs
            {
                Position = _box.GetAbsolutePosition(),
                Color = Style.TextColor,
                FontBinding = Gui.Instance.MainFont,
                LayerDepth = Style.LayerDepth + Constants.LayerDepthStep,
            }
        );
    }

    public override string ToString()
    {
        return $"{nameof(TextInput)} .{ElemClass} #{ElemId}";
    }
}