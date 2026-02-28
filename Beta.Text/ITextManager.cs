using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Beta.Text;

public interface ITextManager
{
    public void Load(ContentManager content);
    public SizeF MeasureString(string s, FontBinding fontBinding);
    public SizeF GetLinesSize(string[] lines, FontBinding fontBinding);
    public void WriteLines(SpriteBatch spriteBatch, string[] lines, TextWriteArgs args);
    public void WriteLine(SpriteBatch spriteBatch, string line, TextWriteArgs args);
}
