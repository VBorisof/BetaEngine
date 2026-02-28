using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Beta.Entities.Animations;
using Beta.Scenes;
using MonoGame.Extended;
using Beta.Verbs;
using Beta.Entities.Costumes;
using Beta.DI;
using Beta.Effects;
using Beta.SpriteBatchBuckets;

namespace Beta.Entities;

public class Entity : ISpriteBatchBucketItem
{
    // Comes from BDSM scripts.
    public string DeclName { get; set; } = "entity";
    public string Name { get; set; } = "Entity";

    public EntityData Data { get; set; } = new();

    // NB: Drawn in Scene.Draw()
    public List<Entity> Children { get; } = [];
    public bool IsShowChildren { get; private set; } = true;
    public Entity? Parent { get; set; }

    public Dictionary<Verb, EventHandler> VerbHandlers { get; }

    public Scene? Scene { get; set; }
    // Current region on the scene, if any.
    public SceneRegion? Region { get; set; }

    private Vector2 _position;
    public Vector2 Position
    {
        get
        {
            if (Parent != null)
            {
                return Parent.Position + _position;
            }
            return _position;
        }
        set => _position = value;
    }

    public Costume CurrentCostume { get; set; } = new();

    private Animation _currentAnimation;
    public Animation CurrentAnimation
    {
        get => _currentAnimation;
        set
        {
            _currentAnimation = value;
            _currentAnimation.IsDone = false;
        }
    }
    private readonly EffectManager _effectManager;
    private readonly SpriteBatchBus _spriteBatchBus;

    public float ScaleMapAdjustment { get; private set; } = 1f;
    public float NativeSceneScale { get; set; } = 1f;
    public float LayerDepth { get; set; }

    public Entity(string name)
    {
        VerbHandlers = [];
        foreach (var verb in Enum.GetValues<Verb>())
        {
            VerbHandlers[verb] = (_, __) => { };
        }

        _effectManager = DependencyContainer.Instance.Get<EffectManager>();
        _spriteBatchBus = DependencyContainer.Instance.Get<SpriteBatchBus>();

        Data = new EntityLoader().Load(name);
        CurrentCostume = Data.Costumes.Single(c => c.Name == "default");
        _currentAnimation = CurrentCostume.Animations.Single(a =>
            string.Equals(a.Name, "idle", StringComparison.OrdinalIgnoreCase)
        );
    }

    public void MoveWithVelocity(Vector2 velocity)
    {
        if (Data is not null)
        {
            Position += Data.Speed * velocity * ScaleMapAdjustment;
        }
    }

    public virtual void Update(GameTime gameTime)
    {
        CurrentAnimation?.Update(gameTime);
    }

    public void ShowChildren()
    {
        IsShowChildren = true;
        foreach (var child in Children)
        {
            child.Scene = Scene;
        }
    }

    public void HideChildren()
    {
        IsShowChildren = false;
        foreach (var child in Children)
        {
            child.Scene = null;
        }
    }

    public void SetIsShowChildren(bool value)
    {
        if (value)
        {
            ShowChildren();
        }
        else
        {
            HideChildren();
        }
    }

    public void DrawInBucket(SpriteBatch spriteBatch)
    {
        // Kinda-Safe™ because checked in Draw()
        var frame = CurrentAnimation!.GetCurrentFrame();
        var finalScale = new Vector2(NativeSceneScale * ScaleMapAdjustment, NativeSceneScale * ScaleMapAdjustment);
        spriteBatch.Draw(
            frame,
            sourceRectangle: new Rectangle(0, 0, frame.Width, frame.Height),
            position: Position,
            effects: SpriteEffects.None,
            rotation: 0,
            origin: Data is null ? Vector2.Zero : Data.Origin * new Vector2(frame.Width, frame.Height),
            color: Color.White,
            scale: finalScale,
            layerDepth: LayerDepth
        );
    }

    public virtual void Draw(SpriteBatch spriteBatch, float scaleMapAdjustment)
    {
        if (CurrentAnimation is null)
        {
            return;
        }

        ScaleMapAdjustment = scaleMapAdjustment;

        _spriteBatchBus.Push(this, _spriteBatchBus.EntityBucket);

        if (Settings.IsDebug)
        {
            spriteBatch.DrawRectangle(GetBoundingRect(), Color.Green);
        }
    }

    public void SetEffects()
    {
        return;

        /*
        if (Scene is null)
        {
            return;
        }

        Scene.DepthMap.GetPixel(Position + Data.Origin)
            .Deconstruct(out float red, out _, out _);

        _effectManager.SetEntityDepth(red);
        */
    }

    //
    // Specialized Draw function that takes width and height and draws the
    // actor at specified position. Ignores Origin.
    // Used for InventoryCellDraggingState, for example.
    public virtual void DrawSizedInPlace(SpriteBatch spriteBatch, int width, int height)
    {
        if (CurrentAnimation == null)
        {
            return;
        }

        var frame = CurrentAnimation.GetCurrentFrame();
        var padding = new Vector2(10, 10);
        var destinationRectangle = new Rectangle(
            (int)Position.X + (int)padding.X,
            (int)Position.Y + (int)padding.Y,
            width,
            height
        );
        spriteBatch.Draw(
            frame,
            sourceRectangle: new Rectangle(0, 0, frame.Width, frame.Height),
            destinationRectangle: destinationRectangle,
            color: Color.White,
            rotation: 0,
            origin: Vector2.Zero,
            layerDepth: LayerDepth,
            effects: SpriteEffects.None
        );

        if (Settings.IsDebug)
        {
            spriteBatch.DrawRectangle(GetBoundingRect(), Color.Green);
        }
    }

    public bool Contains(Vector2 pos)
    {
        return GetBoundingRect().Contains(pos);
    }

    public Rectangle GetBoundingRect()
    {
        if (CurrentAnimation == null)
        {
            return new Rectangle(0, 0, 0, 0);
        }

        var frame = CurrentAnimation.GetCurrentFrame();
        var width = (int)(frame.Width * NativeSceneScale * ScaleMapAdjustment);
        var height = (int)(frame.Height * NativeSceneScale * ScaleMapAdjustment);

        if (Data is null)
        {
            return new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                width,
                height
            );
        }
        else
        {
            return new Rectangle(
                (int)Position.X - (int)(Data.Origin.X * width),
                (int)Position.Y - (int)(Data.Origin.Y * height),
                width,
                height
            );
        }
    }

    public override string ToString()
    {
        return Name;
    }
}