using Beta.DI;
using Beta.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace Beta.SpriteBatchBuckets;

public record SpriteBatchBucket
{
    public required int DrawOrder { get; init; } // The higher, the later
    public required SpriteBatchEffectType EffectType { get; init; }

    private readonly List<ISpriteBatchBucketItem> _items = [];
    private readonly OrthographicCamera _camera;
    private readonly EffectManager _effectManager;

    public SpriteBatchBucket()
    {
        _camera = DependencyContainer.Instance.Get<OrthographicCamera>();
        _effectManager = DependencyContainer.Instance.Get<EffectManager>();
    }

    public void Clear()
    {
        _items.Clear();
    }

    public void Push(ISpriteBatchBucketItem item)
    {
        _items.Add(item);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        switch (EffectType)
        {
            case SpriteBatchEffectType.SceneLighting:
            //    DrawWithSceneLighting(spriteBatch);
            //    break;
            case SpriteBatchEffectType.EntityLighting:
            //    DrawWithEntityLighting(spriteBatch);
            //    break;
            case SpriteBatchEffectType.None:
            default:
                DrawDefault(spriteBatch);
                break;
        }
    }

    private void DrawWithSceneLighting(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(
            sortMode: SpriteSortMode.FrontToBack,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            transformMatrix: _camera.GetViewMatrix(),
            effect: _effectManager.GetSceneLightingEffect()
        );

        foreach (var item in _items)
        {
            item.DrawInBucket(spriteBatch);
        }

        spriteBatch.End();
    }

    private void DrawWithEntityLighting(SpriteBatch spriteBatch)
    {
        // TODO: rethink this. Probably not very efficient.
        // Probably required grouping or something like this.
        foreach (var item in _items)
        {
            spriteBatch.Begin(
                sortMode: SpriteSortMode.FrontToBack,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp,
                transformMatrix: _camera.GetViewMatrix(),
                effect: _effectManager.GetEntityLightingEffect()
            );

            item.SetEffects();
            item.DrawInBucket(spriteBatch);

            spriteBatch.End();
        }
    }

    private void DrawDefault(SpriteBatch spriteBatch)
    {
        var viewMatrix = _camera.GetViewMatrix();

        spriteBatch.Begin(
            sortMode: SpriteSortMode.FrontToBack,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            transformMatrix: viewMatrix
        );

        foreach (var item in _items)
        {
            item.DrawInBucket(spriteBatch);
        }

        spriteBatch.End();
    }
}