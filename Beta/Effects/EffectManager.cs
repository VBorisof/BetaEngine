using Beta.DI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Beta.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Beta.Lights;
using MonoGame.Extended;
using Beta.Services;
using Beta.Logging;
using MonoGame.Extended.ViewportAdapters;

namespace Beta.Effects;

public class EffectManager
{
    private readonly ContentCache _contentCache;
    private readonly OrthographicCamera _camera;
    private readonly GraphicsService _graphics;
    private Effect? _sceneLightingEffect;
    private Effect? _entityLightingEffect;
    private readonly ILogger _logger;
    private readonly BoxingViewportAdapter _viewportAdapter;

    public EffectManager()
    {
        _viewportAdapter = DependencyContainer.Instance.Get<BoxingViewportAdapter>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _contentCache = DependencyContainer.Instance.Get<ContentCache>();
        _camera = DependencyContainer.Instance.Get<OrthographicCamera>();
        _graphics = DependencyContainer.Instance.Get<GraphicsService>();
    }

    public void Load()
    {
        // _sceneLightingEffect = _contentCache.Get<Effect>("fx/scene-lighting");
        // _entityLightingEffect = _contentCache.Get<Effect>("fx/entity-lighting");
    }

    public void SetSceneDepthMap(Texture2D? depthMap)
    {
        if (_sceneLightingEffect is null)
        {
            throw new InvalidOperationException("Scene Lighting Effect was not loaded.");
        }

        _sceneLightingEffect.Parameters["DepthMap"].SetValue(depthMap);
    }

    public void SetEntityDepth(float depth)
    {
        if (_entityLightingEffect is null)
        {
            throw new InvalidOperationException("Entity Lighting Effect was not loaded.");
        }

        _entityLightingEffect.Parameters["EntityDepth"].SetValue(depth);
    }

    public void SetLightPositions(IEnumerable<PointLight> lights)
    {
        if (_sceneLightingEffect is null)
        {
            throw new InvalidOperationException("Lighting Effect was not loaded.");
        }

        // TODO: Finish this.
        return;

#pragma warning disable CS0162
        // Can't really make this work.
        // Something missing in the transformations of the light positions,
        // we probably want to transform them into proper world positions,
        // as well as the pixel positions in the shader itself.
        // Probably also missing some stuff about how the spritebatch + camera works.
        var viewMatrix = _camera.GetViewMatrix();
        var vp = _viewportAdapter.Viewport;
        var projectionMatrix = Matrix.CreateOrthographic(vp.Width, vp.Height, 0, 1);
        var worldViewProjection = viewMatrix;

        _sceneLightingEffect.Parameters["WorldViewProjection"]?.SetValue(worldViewProjection);
        _sceneLightingEffect.Parameters["InverseViewMatrix"]?.SetValue(_camera.GetInverseViewMatrix());
        _sceneLightingEffect.Parameters["ViewportSize"]?.SetValue(new Vector2(vp.Width, vp.Height));
        _sceneLightingEffect.Parameters["ViewportPos"]?.SetValue(new Vector2(vp.X, vp.Y));
        _sceneLightingEffect.Parameters["PointLightPositions"]?.SetValue(
            lights.Select(l =>
            {
                var worldPos = _camera.WorldToScreen(l.Position.X, l.Position.Y);
                return new Vector3(worldPos.X, worldPos.Y, l.Position.Z);
            }).ToArray()
        );

        /*
        _sceneLightingEffect.Parameters["PointLightPositions"].SetValue(
            lights.Select(l =>
            {
                return l.Position;
            }).ToArray()
        );
        */

        if (_entityLightingEffect is null)
        {
            throw new InvalidOperationException("Lighting Effect was not loaded.");
        }

        _entityLightingEffect.Parameters["LightPositions"].SetValue(
            lights.Select(l =>
            {
                var worldPos = _camera.ScreenToWorld(l.Position.X, l.Position.Y);
                return new Vector3(worldPos.X, worldPos.Y, l.Position.Z);
            }).ToArray()
        );
#pragma warning restore CS0162
    }

    public void SetPointLights(IEnumerable<PointLight> lights)
    {        
        // TODO: Finish this.
        return;

#pragma warning disable CS0162
        if (_sceneLightingEffect is null)
        {
            throw new InvalidOperationException("Lighting Effect was not loaded.");
        }

        _sceneLightingEffect.Parameters["PointLightsNum"]?.SetValue(
            lights.Count()
        );

        SetLightPositions(lights);

        _sceneLightingEffect.Parameters["PointLightColors"]?.SetValue(
            lights.Select(l => l.Color).ToArray()
        );
        _sceneLightingEffect.Parameters["PointLightIntensities"]?.SetValue(
            lights.Select(l => l.Color).ToArray()
        );
        // TODO: Move to scene and load from resource.
        _sceneLightingEffect.Parameters["AmbientColor"]?.SetValue(
            new Vector3(0.4f, 0.4f, 0.4f)
        );

        if (_entityLightingEffect is null)
        {
            throw new InvalidOperationException("Lighting Effect was not loaded.");
        }

        _entityLightingEffect.Parameters["LightsNum"]?.SetValue(
            lights.Count()
        );
        _entityLightingEffect.Parameters["LightColors"]?.SetValue(
            lights.Select(l => l.Color).ToArray()
        );
#pragma warning restore CS0162
    }

    public Effect? GetSceneLightingEffect()
    {
        return _sceneLightingEffect;
    }

    public Effect? GetEntityLightingEffect()
    {
        return _entityLightingEffect;
    }
}