using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json;
using System.Linq;
using Beta.Common;
using Beta.DI;
using Microsoft.Xna.Framework;
using Beta.Entities.Animations;
using Beta.Logging;
using Beta.Entities.Costumes;
using System;
using System.Text.Json.Serialization;
using Beta.Extensions.Models;
using Beta.ContentTools;

namespace Beta.Entities;

[JsonSerializable(typeof(EntityDataModel))]
[JsonSerializable(typeof(Vector2Model))]
[JsonSerializable(typeof(CostumeModel))]
[JsonSerializable(typeof(AnimationModel))]
internal partial class EntityDataModelGenerationContext : JsonSerializerContext
{
}

public class EntityLoader
{
    private readonly ILogger _logger;
    private readonly ContentCache _contentCache;
    private readonly IContentPathProvider _contentPathProvider;

    public EntityLoader()
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _contentCache = DependencyContainer.Instance.Get<ContentCache>();
        _contentPathProvider = DependencyContainer.Instance.Get<IContentPathProvider>();
    }

    private static readonly JsonSerializerOptions _options = new()
    {
        TypeInfoResolver = EntityDataModelGenerationContext.Default
    };

    public EntityData Load(string entityName)
    {
        var json = FileLoader.ReadAllFromFile(_contentPathProvider.ProvideEntityPath(entityName));
#pragma warning disable IL3050, IL2026
        var entityDataModel = JsonSerializer.Deserialize<EntityDataModel>(json, _options);
#pragma warning restore IL3050, IL2026
        if (entityDataModel is null)
        {
            throw new ArgumentException($"Entity data not found at {entityName}");
        }

        var costumes = new List<Costume>();
        foreach (var costumeModel in entityDataModel.Costumes)
        {
            var animations = new List<Animation>();
            foreach (var anim in costumeModel.Animations)
            {
                var frames = anim.Frames.Select(_contentCache.Get<Texture2D>).ToList();
                _logger.Debug($"Load animations: \n    {string.Join("\n    ", frames)}");
                animations.Add(
                    new Animation(
                        anim.Name,
                        anim.Speed,
                        anim.Repeat,
                        anim.Frames,
                        frames
                    )
                );
            }

            var costume = new Costume
            {
                Name = costumeModel.Name,
                Animations = animations
            };
            costumes.Add(costume);
        }

        var entityData = new EntityData
        {
            Costumes = costumes,
            Speed = entityDataModel.Speed,
            Origin = new Vector2(entityDataModel.Origin.X, entityDataModel.Origin.Y)
        };

        return entityData;
    }
}