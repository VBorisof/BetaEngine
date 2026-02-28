using System.Collections.Generic;
using System.Linq;
using Beta.Text.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Beta.Text;

public class TextManager : ITextManager
{
    private const float LAYER_DEPTH_MICROSTEP = 0.000001f;
    private readonly float _defaultLayerDepth;

    private readonly List<FontBinding> _fontBindings = [];

    public TextManager(List<FontBinding> fontBindings, float defaultLayerDepth)
    {
        _fontBindings = fontBindings;
        _defaultLayerDepth = defaultLayerDepth;
    }

    public void Load(ContentManager content)
    {
        foreach (var fontBinding in _fontBindings)
        {
            fontBinding.Load(content);
        }
    }

    public SizeF MeasureString(string s, FontBinding fontBinding)
    {
        if (fontBinding.Font is null)
        {
            return SizeF.Empty;
        }
        var size = fontBinding.Font.MeasureString(s);
        return new SizeF(size.X, fontBinding.VerticalSize);
    }

    public SizeF GetLinesSize(string[] lines, FontBinding fontBinding)
    {
        if (fontBinding.Font is null)
        {
            return SizeF.Empty;
        }

        float totalHeight = lines.Length * fontBinding.Font.LineSpacing;
        float maxWidth = lines.Max(l => MeasureString(l, fontBinding).Width);

        return new SizeF(maxWidth, totalHeight);
    }

    public void WriteLines(SpriteBatch spriteBatch, string[] lines, TextWriteArgs args)
    {
        if (args.FontBinding.Font is null)
        {
            return;
        }

        float lineSpacing = args.FontBinding.Font.LineSpacing;
        float totalHeight = lines.Length * lineSpacing;

        float dy = 0;
        foreach (var line in lines)
        {
            var textSize = args.FontBinding.Font.MeasureString(line);
            var linePos = args.Position - new Vector2(0, dy);

            var lineArgs = args.Copy();
            lineArgs.Position = linePos;

            WriteLine(spriteBatch, line, lineArgs);

            dy -= lineSpacing;
        }
    }

    public void WriteLine(SpriteBatch spriteBatch, string line, TextWriteArgs args)
    {
        if (args.FontBinding.Font is null)
        {
            return;
        }

        var position = args.Position;
        if (args.TextAlignment == TextAlignment.Center)
        {
            var lineSize = args.FontBinding.MeasureString(line);
            position -= new Vector2(lineSize.Width / 2, 0);
        }

        float layerDepth = args.LayerDepth ?? _defaultLayerDepth;

        if (args.FontBinding.OutlineFont is not null)
        {
            Vector2 offset = new Vector2(2, 0);
            spriteBatch.DrawString(
                args.FontBinding.OutlineFont,
                line,
                position - offset,
                args.OutlineColor,
                layerDepth - LAYER_DEPTH_MICROSTEP
            );
        }

        spriteBatch.DrawString(
            args.FontBinding.Font,
            line,
            position,
            args.Color,
            layerDepth
        );
    }
}