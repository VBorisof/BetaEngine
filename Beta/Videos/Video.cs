using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beta.Videos;

public class Video
{
    public bool IsDone = false;

    public virtual void Update(GameTime gameTime) { }
    public virtual void Draw(SpriteBatch spriteBatch) { }
    public virtual void Reset()
    {
        IsDone = false;
    }
}

