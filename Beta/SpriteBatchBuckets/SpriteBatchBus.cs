using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Beta.SpriteBatchBuckets;

public class SpriteBatchBus
{
    private readonly SortedList<int, SpriteBatchBucket> _buckets = [];

    public SpriteBatchBucket SceneBucket { get; }
    public SpriteBatchBucket EntityBucket { get; }
    public SpriteBatchBucket DefaultBucket { get; }

    public SpriteBatchBus()
    {
        SceneBucket = new SpriteBatchBucket
        {
            DrawOrder = 0,
            EffectType = SpriteBatchEffectType.SceneLighting
        };

        EntityBucket = new SpriteBatchBucket
        {
            DrawOrder = 1,
            EffectType = SpriteBatchEffectType.EntityLighting
        };

        DefaultBucket = new SpriteBatchBucket
        {
            DrawOrder = 2,
            EffectType = SpriteBatchEffectType.None
        };

        _buckets.Add(SceneBucket.DrawOrder, SceneBucket);
        _buckets.Add(EntityBucket.DrawOrder, EntityBucket);
        _buckets.Add(DefaultBucket.DrawOrder, DefaultBucket);
    }

    public void Push(ISpriteBatchBucketItem item, SpriteBatchBucket bucket)
    {
        bucket.Push(item);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var bucket in _buckets)
        {
            bucket.Value.Draw(spriteBatch);
            bucket.Value.Clear();
        }
    }
}