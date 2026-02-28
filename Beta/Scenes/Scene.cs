using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Beta.Actors;
using Beta.DI;
using Beta.Entities;
using Beta.Common;
using Beta.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Shapes;
using System;
using Beta.Common.Extensions;
using Beta.SpriteBatchBuckets;
using Beta.Lights;
using Beta.Effects;
using Beta.Scenes.Pathfinding;
using System.Text.Json.Serialization;
using Beta.Extensions.Models;
using Beta.Services.Sounds;
using Beta.ContentTools;

namespace Beta.Scenes;

[JsonSerializable(typeof(SceneMeta))]
[JsonSerializable(typeof(ExitModel))]
[JsonSerializable(typeof(Vector2Model))]
[JsonSerializable(typeof(Vector3Model))]
[JsonSerializable(typeof(RegionModel))]
[JsonSerializable(typeof(PropModel))]
[JsonSerializable(typeof(WalkableAreaModel))]
[JsonSerializable(typeof(ScenePlacementModel))]
[JsonSerializable(typeof(SceneLightModel))]
[JsonSerializable(typeof(SceneLightType))]
internal partial class SceneMetaGenerationContext : JsonSerializerContext
{
}

public class Scene : ISpriteBatchBucketItem
{
    public string Name { get; set; } = "Scene";
    public string FriendlyName { get; set; } = "Scene";
    public string? MusicName { get; private set; }
    public Texture2D Texture { get; set; }
    public TextureMap ScaleMap { get; set; } = new();
    public TextureMap DepthMap { get; set; } = new();

    public Vector2 Position { get; set; }

    private SceneMeta? _meta;
    public List<SceneExit> Exits { get; set; } = [];
    public List<SceneRegion> Regions { get; set; } = [];
    public List<SceneProp> Props { get; set; } = [];
    public List<SceneWalkableArea> WalkableAreas { get; set; } = [];
    public List<ScenePlacement> ScenePlacements { get; set; } = [];
    public Graph WalkGraph { get; set; } = new();
    public List<PointLight> PointLights { get; } = [];
    private readonly List<Walkbehind> _walkbehinds = [];

    private readonly EntityManager _entityManager;
    private readonly MusicPlayerService _musicPlayer;
    private readonly IContentPathProvider _contentPathProvider;
    private readonly SpriteBatchBus _spriteBatchBus;
    private readonly ContentCache _contentCache;
    private readonly ILogger _logger;

    private readonly EffectManager _effectManager;

    public Scene(string name)
    {
        _effectManager = DependencyContainer.Instance.Get<EffectManager>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _contentCache = DependencyContainer.Instance.Get<ContentCache>();
        _entityManager = DependencyContainer.Instance.Get<EntityManager>();
        _musicPlayer = DependencyContainer.Instance.Get<MusicPlayerService>();
        _contentPathProvider = DependencyContainer.Instance.Get<IContentPathProvider>();

        _spriteBatchBus = DependencyContainer.Instance.Get<SpriteBatchBus>();

        Name = name;

        Texture = _contentCache.Get<Texture2D>(_contentPathProvider.ProvideSceneTexturePath(name));

        // Scalemap
        ScaleMap = new TextureMap();
        if (!ScaleMap.Load(_contentPathProvider.ProvideScaleMapPath(name)))
        {
            _logger.Warning($"No Scale Map for scene {name}");
        }

        // DepthMap
        DepthMap = new TextureMap();
        if (!DepthMap.Load(_contentPathProvider.ProvideDepthMapPath(name)))
        {
            _logger.Warning($"No Depth Map for scene {name}");
        }

        // Walkbehinds
        /*
        var dir = new DirectoryInfo($"{_contentCache.RootDirectory}/scenes/{name}");
        var walkbehinds = dir.GetFiles($"{name}.wb*");
        foreach (var walkbehind in walkbehinds)
        {
            var wbname = Path.GetFileNameWithoutExtension(walkbehind.Name);
            var baseline = int.Parse(wbname.Split('-').Last(), CultureInfo.InvariantCulture);
            var texture = _contentCache.Get<Texture2D>($"img/{name}/{wbname}");
            _walkbehinds.Add(new Walkbehind(texture, baseline));
        }
        */

        // Load metadata.
        LoadMeta();

        _logger.Info($"Load scene: `{name}`. {Texture.Bounds.Width}x{Texture.Bounds.Height}");
    }

    public void SetMusic(string musicName)
    {
        MusicName = musicName;
        _musicPlayer.AddSong(musicName, $"sounds/scenes/{musicName}");
    }

    private static readonly JsonSerializerOptions _metaOptions = new()
    {
        TypeInfoResolver = SceneMetaGenerationContext.Default
    };

    private void LoadMeta()
    {
        var json = FileLoader.ReadAllFromFile(_contentPathProvider.ProvideSceneMetaPath(Name));
#pragma warning disable IL3050, IL2026
        _meta = JsonSerializer.Deserialize<SceneMeta>(json, _metaOptions);
#pragma warning restore IL3050, IL2026

        if (_meta is null)
        {
            _logger.Error("Failed to load scene meta.");
            throw new InvalidOperationException("Failed to load scene meta.");
        }

        // Exits
        foreach (var exit in _meta.Exits)
        {
            Exits.Add(new SceneExit
            {
                Destination = exit.Destination,
                ExitPoint = exit.ExitPoint.ToVector2(),
                Nodes = exit.Nodes.Select(n => n.ToVector2()).ToList(),
                StartIndex = exit.StartIndex,
                TargetIndex = exit.TargetIndex,
                Polygon = new Polygon(exit.Nodes.Select(n => n.ToVector2()).ToList())
            });
        }
        // Regions
        foreach (var region in _meta.Regions)
        {
            Regions.Add(new SceneRegion
            {
                Name = region.Name,
                Nodes = region.Nodes.Select(n => n.ToVector2()).ToList(),
                Polygon = new Polygon(region.Nodes.Select(n => n.ToVector2()).ToList()),
                TimesActive = region.TimesActive
            });
        }
        // Props
        foreach (var prop in _meta.Props)
        {
            Props.Add(new SceneProp
            {
                DeclName = prop.DeclName,
                Name = prop.DeclName,
                Nodes = prop.Nodes.Select(n => n.ToVector2()).ToList(),
                Polygon = new Polygon(prop.Nodes.Select(n => n.ToVector2()).ToList()),
            });
        }
        // Lights
        foreach (var light in _meta.Lights)
        {
            switch (light.LightType)
            {
                case SceneLightType.Point:
                    PointLights.Add(new PointLight
                    {
                        Position = light.LightPosition.ToVector3(),
                        Color = ColorEx.FromHexString(light.LightColor).ToVector3(),
                        Intensity = light.LightIntensity,
                    });
                    break;
                default:
                    break;
            }
        }

        // Walkable Areas
        foreach (var area in _meta.WalkableAreas)
        {
            WalkableAreas.Add(new SceneWalkableArea
            {
                Index = area.Index,
                Nodes = area.Nodes.Select(n => n.ToVector2()).ToList(),
                Polygon = new Polygon(area.Nodes.Select(n => n.ToVector2()).ToList()),
            });
        }
        WalkGraph = _meta.CreateGraph(WalkableAreas);

        // Actors
        ScenePlacements = _meta.Actors
            .Select(ScenePlacement.FromScenePlacementModel)
            .ToList();
        foreach (var scenePlacement in ScenePlacements)
        {
            // HACK
            //if (metaActor.Name == "charlie")
            //{
            //    continue;
            //}

            if (!_entityManager.Contains(scenePlacement.Name))
            {
                _logger.Error($"Failed to get entity {scenePlacement.Name}");
                continue;
            }

            var entity = _entityManager.Get<Entity>(scenePlacement.Name);
            entity.Scene = this;
            entity.Position = new Vector2(scenePlacement.Position.X, scenePlacement.Position.Y);
            entity.NativeSceneScale = scenePlacement.Scale;
            if (!string.IsNullOrWhiteSpace(scenePlacement.State))
            {
                // TODO: Reconsider this. Why is this not actor in the first place?
                ((Actor)entity).ForceState(new ActorState(scenePlacement.State, isManuallyManaged: true));
            }

            // Children
            foreach (var metaChild in scenePlacement.Children)
            {
                var child = _entityManager.Get<Entity>(metaChild.Name);
                child.Scene = this;
                child.Position = new Vector2(metaChild.Position.X, metaChild.Position.Y);
                child.NativeSceneScale = metaChild.Scale;
                child.Parent = entity;
                child.SetIsShowChildren(metaChild.IsShowChildren);
                if (!string.IsNullOrWhiteSpace(metaChild.State))
                {
                    // TODO: Reconsider this. Why is this not actor in the first place?
                    ((Actor)child).ForceState(new ActorState(metaChild.State, isManuallyManaged: true));
                }

                entity.Children.Add(child);
            }

            entity.SetIsShowChildren(scenePlacement.IsShowChildren);
        }
    }

    public void DrawInBucket(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            Texture,
            Position,
            sourceRectangle: Texture!.Bounds,
            color: Color.White,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: Vector2.One,
            effects: SpriteEffects.None,
            layerDepth: Constants.LayerDepthScene
        );
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        //
        // Draw Scene
        // 
        _spriteBatchBus.Push(this, _spriteBatchBus.SceneBucket);

        var sceneEntities = _entityManager.GetOnScene(this);

        //
        // Init Entity Layer Depth
        //
        var entityLayerDepth = Constants.LayerDepthWbBase - Constants.LayerDepthStep;
        foreach (var entity in sceneEntities.OrderBy(e => e.Position.Y))
        {
            entity.LayerDepth = entityLayerDepth;
            entityLayerDepth += Constants.LayerDepthStep;

            foreach (var child in entity.Children)
            {
                child.LayerDepth = entity.LayerDepth + Constants.LayerDepthMicroStep;
            }
        }

        //
        // Draw Walkbehinds
        //
        var wbLayerDepth = Constants.LayerDepthWbBase * 2;
        foreach (var wb in _walkbehinds.OrderBy(wb => wb.Baseline))
        {
            wb.LayerDepth = wbLayerDepth;

            entityLayerDepth = wb.LayerDepth + Constants.LayerDepthStep;
            foreach (var entity in sceneEntities.Where(e => e.Position.Y > wb.Baseline).OrderBy(e => e.Position.Y))
            {
                entity.LayerDepth = entityLayerDepth;
                entityLayerDepth += Constants.LayerDepthStep;

                foreach (var child in entity.Children)
                {
                    child.LayerDepth = entity.LayerDepth + Constants.LayerDepthStep;
                }
            }

            spriteBatch.Draw(
                wb.Texture,
                Vector2.Zero,
                sourceRectangle: Texture.Bounds,
                color: Color.White,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: Vector2.One,
                effects: SpriteEffects.None,
                layerDepth: wb.LayerDepth
            );

            wbLayerDepth += Constants.LayerDepthWbBase;
        }

        //
        // Draw Entities
        //
        foreach (var entity in sceneEntities)
        {
            ScaleMap.GetPixel(entity.Position)
                    .Deconstruct(out _, out _, out _, out float scale);

            entity.SetEffects();
            entity.Draw(spriteBatch, scale);

            /* Shader code, disabled for now.
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                entity.Draw(spriteBatch, scale);
            }
            */
        }
    }

    public virtual void Update(GameTime gameTime)
    {
    }

    public void SetEffects()
    {
        return;
        // _effectManager.SetSceneDepthMap(DepthMap.Texture);
        // _effectManager.SetPointLights(PointLights);
    }

    public void UpdateEffects()
    {
        return;
        // _effectManager.SetLightPositions(PointLights);
    }

    public List<Vector2> MakePath(Vector2 from, Vector2 to)
    {
        if (_meta is null)
        {
            throw new InvalidOperationException("Scene not loaded properly.");
        }
        return _meta.MakePath(from, to);
    }
}
