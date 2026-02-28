using System;
using System.Numerics;
using Beta.Common.Extensions;
using Beta.Gui.Events;
using Beta.Gui.Styles;
using MonoGame.Extended;

namespace Beta.Gui.Elements;

public class OkCancelMessageBox : GuiElement
{
    private Label _label;
    private readonly Box _box;
    private readonly TextButton _okButton;
    private readonly TextButton _cancelButton;
    public EventHandler<GuiMouseEventArgs> OnOk = (_, __) => { };
    public EventHandler<GuiMouseEventArgs> OnCancel = (_, __) => { };

    public string Text { get; }

    public OkCancelMessageBox(string text, GuiElementStyle style, string? extraInputContexts) : base(style, extraInputContexts)
    {
        _box = new Box(style with
        {
            RelativePosition = Vector2.Zero
        }, extraInputContexts);
        if (Gui.Instance.TextManager is null || Gui.Instance.MainFont is null)
        {
            var message = "Text Manager is not properly loaded.";
            Gui.Instance.Logger?.Error(message);
            throw new InvalidOperationException(message);
        }

        var charWidth = Gui.Instance.TextManager.MeasureString("w", Gui.Instance.MainFont).Width;
        Text = text.SetLineWidth((int)(style.Size.Width / charWidth));
        _label = new Label(
            Text,
            style with
            {
                RelativePosition = new Vector2(10, 10),
                LayerDepth = style.LayerDepth + Constants.LayerDepthStep
            }
        , extraInputContexts);

        // TODO: Remove hardcode
        var buttonSize = new SizeF(100, 30);
        var hMargin = 10f;
        var vMargin = 10f;

        _okButton = new TextButton("OK", style with
        {
            Size = buttonSize,
            LayerDepth = style.LayerDepth + Constants.LayerDepthStep,
        }, extraInputContexts);
        _okButton.Style.RelativePosition =
            new Vector2(
                hMargin,
                _box.Style.Size.Height - _okButton.Style.Size.Height - vMargin
            );
        _okButton.SubscribeOnLeftClick((_, args) => OnOk(this, args));

        _cancelButton = new TextButton("Cancel", style with
        {
            Size = buttonSize,
            LayerDepth = style.LayerDepth + Constants.LayerDepthStep,
        }, extraInputContexts);
        _cancelButton.Style.RelativePosition =
            new Vector2(
                _box.Style.Size.Width - _cancelButton.Style.Size.Width - hMargin,
                _box.Style.Size.Height - _cancelButton.Style.Size.Height - vMargin
            );

        _cancelButton.SubscribeOnLeftClick((_, args) => OnCancel(this, args));

        _box.AddElement(_label);
        _box.AddElement(_okButton);
        _box.AddElement(_cancelButton);
        AddElement(_box);
    }

    public override string ToString()
    {
        return $"{nameof(OkCancelMessageBox)} .{ElemClass} #{ElemId}";
    }
}