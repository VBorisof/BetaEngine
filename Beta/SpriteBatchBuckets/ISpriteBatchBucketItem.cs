using Microsoft.Xna.Framework.Graphics;

namespace Beta.SpriteBatchBuckets;

public interface ISpriteBatchBucketItem
{
    public void DrawInBucket(SpriteBatch spriteBatch);
    public void SetEffects();
}