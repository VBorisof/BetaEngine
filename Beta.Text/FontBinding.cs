using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Beta.Text;

public class FontBinding
{
    private readonly string _fontName;
    public SpriteFont? Font { get; private set; }

    private readonly string? _outlineFontName;
    private readonly int? _lineSpacing;
    public int VerticalSize { get; } // TODO: For some reason the string measurements are wrong, so need this...

    public SpriteFont? OutlineFont { get; private set; }

    public FontBinding(int verticalSize, string fontName, string? outlineFontName = default, int? lineSpacing = default)
    {
        VerticalSize = verticalSize;
        _fontName = fontName;
        _outlineFontName = outlineFontName;
        _lineSpacing = lineSpacing;
    }

    public void Load(ContentManager content)
    {
        Font = content.Load<SpriteFont>($"fonts/{_fontName}");
        if (_lineSpacing is not null)
        {
            Font.LineSpacing = _lineSpacing.Value;
        }
        if (_outlineFontName is not null)
        {
            OutlineFont = content.Load<SpriteFont>($"fonts/{_outlineFontName}");
            if (_lineSpacing is not null)
            {
                OutlineFont.LineSpacing = _lineSpacing.Value;
            }
        }
    }

    public SizeF MeasureString(string s)
    {
        if (Font is null)
        {
            return SizeF.Empty;
        }
        var size = Font.MeasureString(s);
        return new SizeF(size.X, size.Y);
    }
}