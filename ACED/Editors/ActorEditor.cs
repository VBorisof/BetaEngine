using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Microsoft.Xna.Framework.Graphics;
using Beta.DI;
using Beta.Entities.Animations;
using System.Collections.Generic;
using Beta.Logging;
using System.Text.Json;
using System.IO;
using System.Globalization;
using System.Linq;
using Beta.Input;
using System;
using aced.Models;
using aced.Exceptions;

namespace aced.Editors;

public class ActorEditor : IInputEventListener
{
    private readonly GraphicsDeviceManager _graphics;

    private ILogger _logger { get; }

    private readonly InputContextManager _inputContextManager;

    public List<Costume> Costumes { get; private set; } = [];
    public Costume CurrentCostume { get; private set; }
    public Animation CurrentAnimation { get; private set; }

    public ActorData CurrentActorData { get; private set; }
    public RectangleF Viewport { get; } = new(400, 100, 1120, 900);

    private float _spriteX;
    private float _spriteY;
    private float _spriteWidth;
    private float _spriteHeight;

    public ActorEditor()
    {
        _graphics = DependencyContainer.Instance.Get<GraphicsDeviceManager>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _inputContextManager = DependencyContainer.Instance.Get<InputContextManager>();

        CurrentActorData = new ActorData();
    }

    public void ChangeActorName(string name)
    {
        CurrentActorData.Name = name;
    }
    public void ChangeActorSpeed(float speed)
    {
        CurrentActorData.Speed = speed;
    }

    //
    // Costumes
    //
    public Costume AddCostume()
    {
        var name = Costumes.Count != 0 ? "" : "default";
        var costume = new Costume
        {
            Id = Guid.NewGuid(),
            Name = name
        };
        Costumes.Add(costume);
        CurrentActorData.Costumes.Add(new CostumeModel
        {
            Id = costume.Id,
            Name = costume.Name
        });

        return costume;
    }
    public void SetCurrentCostume(Costume costume)
    {
        if (!Costumes.Any(c => c == costume))
        {
            _logger.Error("Tried to select inexistent costume!");
            return;
        }

        CurrentCostume = costume;
    }
    public void DeleteCostume(Costume costume)
    {
        CurrentActorData.Costumes.RemoveAll(c => c.Id == costume.Id);
        if (costume.Animations.Any(a => a == CurrentAnimation))
        {
            CurrentAnimation = null;
        }
        Costumes.Remove(costume);
    }
    public void ChangeCurrentCostumeName(string name)
    {
        CurrentCostume.Name = name;

        var costumeModel = CurrentActorData.Costumes.Single(c => c.Id == CurrentCostume.Id);
        costumeModel.Name = name;
    }

    //
    // Animations
    //
    public Animation AddAnimationToCurrentCostume(string[] filePaths)
    {
        var frames = filePaths.Select(p =>
        {
            using var fs = new FileStream(p, FileMode.Open);
            return Texture2D.FromStream(_graphics.GraphicsDevice, fs);
        })
        .ToList();

        var name = CurrentCostume.Animations.Count != 0 ? "" : "idle";
        var animation = new Animation(name, speed: 0.01f, repeat: true, [.. filePaths], frames);
        CurrentCostume.Animations.Add(animation);

        return animation;
    }

    public void SetCurrentAnimation(Animation anim)
    {
        if (!CurrentCostume.Animations.Any(a => a == anim))
        {
            _logger.Error("Wrong costume animation!");
            return;
        }
        CurrentAnimation = anim;
    }

    public void DeleteAnimation(Animation anim)
    {
        var costume = Costumes.SingleOrDefault(c => c.Animations.Contains(anim));
        if (costume is null)
        {
            _logger.Error("Animation to delete not found.");
            return;
        }
        costume.Animations.Remove(anim);

        if (CurrentAnimation == anim)
        {
            CurrentAnimation = null;
        }
    }

    public void ChangeCurrentAnimationName(string name)
    {
        CurrentAnimation.Name = name;
    }
    public void ChangeCurrentAnimationSpeed(float speed)
    {
        CurrentAnimation.Speed = speed;
    }
    public void ChangeCurrentAnimationRepeat(bool repeat)
    {
        CurrentAnimation.Repeat = repeat;
    }

    public void ResetEnvironment()
    {
        DestroyEnvironment();
    }

    public void DestroyEnvironment()
    {
        Costumes.Clear();
        CurrentAnimation = null;
        CurrentActorData.Name = "";
        CurrentActorData.Speed = 1f;
        CurrentActorData.Costumes = [];
        CurrentActorData.Origin = new Coord();
    }

    private void OnRightClick(Vector2 pos)
    {
        if (pos.X < _spriteX || pos.X > _spriteX + _spriteWidth
            || pos.Y < _spriteY || pos.Y > _spriteY + _spriteHeight)
        {
            return;
        }

        var posInSprite = pos - new Vector2(_spriteX, _spriteY);
        var relativePos = new Vector2(posInSprite.X / _spriteWidth, posInSprite.Y / _spriteHeight);

        CurrentActorData.Origin = new Coord(relativePos.X, relativePos.Y);
    }

    public void ExportActor()
    {
        var actorPathSuffix = $"actors/{CurrentActorData.Name}";
        var actorAssetPath = $"{Settings.CONTENT_BASE_PATH}/{actorPathSuffix}";
        if (Settings.IsDryRun)
        {
            _logger.Info($"[DR] Create directory {actorAssetPath}");
        }
        else if (!Directory.Exists(actorAssetPath))
        {
            Directory.CreateDirectory(actorAssetPath);
        }

        // Copy all the frames.
        foreach (var costume in Costumes)
        {
            foreach (var anim in costume.Animations)
            {
                for (var i = 0; i < anim.FramePaths.Count; ++i)
                {
                    var framePath = anim.FramePaths[i];
                    var frameName = Path.GetFileName(framePath);

                    var destFile = $"{actorAssetPath}/{frameName}";

                    if (destFile == $"{Settings.CONTENT_BASE_PATH}/{framePath}")
                    {
                        // Already copied, so skip; Kinda hacky...
                        continue;
                    }

                    if (Settings.IsDryRun)
                    {
                        _logger.Info($"[DR] Copy frame `{framePath}` into `{actorAssetPath}/{frameName}` and add to Content");
                    }
                    else
                    {
                        File.Copy($"{framePath}", $"{actorAssetPath}/{frameName}", overwrite: true);

                        // Add to content pipeline...
                        var mgcbActorDefTemplate = File.ReadAllText("Templates/template_actor_mgcb.txt");
                        mgcbActorDefTemplate = mgcbActorDefTemplate.Replace("$path", $"{actorPathSuffix}/{frameName}");

                        File.AppendAllText($"{Settings.CONTENT_BASE_PATH}/content.mgcb", mgcbActorDefTemplate);
                    }
                }
            }
        }

        // Save actor JSON
        foreach (var costumeModel in CurrentActorData.Costumes)
        {
            var costume = Costumes.Single(c => c.Id == costumeModel.Id);
            costumeModel.Animations = costume.Animations.Select(animation =>
            {
                var framePaths = animation.FramePaths.Select(fp =>
                {
                    var frameFilename = fp.Split("/").Last();
                    if (frameFilename.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        var idx = frameFilename.LastIndexOf('.');
                        frameFilename = frameFilename[..idx];
                    }
                    return $"{actorPathSuffix}/{frameFilename}";
                })
                .ToList();

                return new AnimationModel
                {
                    Name = animation.Name,
                    Speed = animation.Speed,
                    Repeat = animation.Repeat,
                    Frames = framePaths
                };
            }).ToList();
        }

        var actorJsonDir = $"{Settings.JSON_RES_BASE_PATH}/{actorPathSuffix}";
        if (!Directory.Exists(actorJsonDir))
        {
            Directory.CreateDirectory(actorJsonDir);
        }

        var actorJsonPath = $"{actorJsonDir}/{CurrentActorData.Name}.actor.json";
        var json = JsonSerializer.Serialize(CurrentActorData);

        if (Settings.IsDryRun)
        {
            _logger.Info($"[DR] Actor JSON: `{actorJsonPath}`");
            _logger.Info();
            _logger.Info(json);
            _logger.Info();
        }
        else
        {
            File.WriteAllText(actorJsonPath, json);
        }

        _logger.Debug($"Saved actor: `{actorJsonPath}`");

        // Add BDSM definition if doesn't exist.
        var bdsmPath = $"{Settings.SCRIPTS_BASE_PATH}/{actorPathSuffix}.bs";
        if (!File.Exists(bdsmPath))
        {
            var actorDefTemplate = File.ReadAllText("Templates/template_actor.txt");
            actorDefTemplate = actorDefTemplate.Replace("$declname", CurrentActorData.Name);
            var titledName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(CurrentActorData.Name);
            actorDefTemplate = actorDefTemplate.Replace("$name", titledName);
            actorDefTemplate = actorDefTemplate.Replace("$color", "#00ff00"); // TODO: Color picker

            if (Settings.IsDryRun)
            {
                _logger.Info($"[DR] Write Actor BDSM file to {bdsmPath}:");
                _logger.Info();
                _logger.Info(actorDefTemplate);
                _logger.Info();
            }
            else
            {
                File.WriteAllText(bdsmPath, actorDefTemplate);
            }
        }

        // TODO: Cache file for mgcb so that we know we already added an asset.
    }

    public ActorData ReadActor(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new ActorLoadException($"File doesn't exist: {filePath}");
        }

        ResetEnvironment();

        var json = File.ReadAllText(filePath);

        CurrentActorData = JsonSerializer.Deserialize<ActorData>(json);

        var costumes = new List<Costume>();
        foreach (var costumeModel in CurrentActorData.Costumes)
        {
            var animations = new List<Animation>();
            foreach (var anim in costumeModel.Animations)
            {
                var frames = anim.Frames.Select(frameRelativePath =>
                    {
                        var framePath = $"{Settings.CONTENT_BASE_PATH}/{frameRelativePath}.png";
                        using var fs = new FileStream(framePath, FileMode.Open);
                        return Texture2D.FromStream(_graphics.GraphicsDevice, fs);
                    }
                );
                animations.Add(
                    new Animation(
                        anim.Name,
                        anim.Speed,
                        anim.Repeat,
                        anim.Frames,
                        frames.ToList()
                    )
                );
            }
            var costume = new Costume
            {
                Id = Guid.NewGuid(),
                Name = costumeModel.Name,
                Animations = animations
            };
            costumeModel.Id = costume.Id;
            costumes.Add(costume);
        }
        Costumes = costumes;
        CurrentAnimation = costumes.FirstOrDefault()?.Animations.FirstOrDefault();

        return CurrentActorData;
    }

    public void Update(GameTime gameTime)
    {
        CurrentAnimation?.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Draw viewport
        spriteBatch.DrawRectangle(
            Viewport,
            Color.Black,
            thickness: Constants.LayerDepthViewport
        );

        if (CurrentAnimation != null)
        {
            var frame = CurrentAnimation.GetCurrentFrame();
            _spriteX = Viewport.X;
            _spriteY = Viewport.Y;
            _spriteWidth = frame.Width;
            _spriteHeight = frame.Height;
            if (frame.Height > frame.Width)
            {
                _spriteWidth = frame.Width * Viewport.Height / frame.Height;
                _spriteHeight = Viewport.Height;
                _spriteX = Viewport.X + (Viewport.Width / 2) - (_spriteWidth / 2);
            }
            else
            {
                _spriteHeight = Viewport.Width * frame.Height / frame.Width;
                _spriteWidth = Viewport.Width;
                _spriteY = Viewport.Y + (Viewport.Height / 2) - (_spriteHeight / 2);
            }

            var destinationRectangle = new RectangleF(_spriteX, _spriteY, _spriteWidth, _spriteHeight);

            // Draw character
            spriteBatch.Draw(
                frame,
                sourceRectangle: new Rectangle(0, 0, frame.Width, frame.Height),
                destinationRectangle: destinationRectangle.ToRectangle(),
                effects: SpriteEffects.None,
                rotation: 0,
                origin: Vector2.Zero,
                color: Color.White,
                layerDepth: Constants.LayerDepthActor
            );

            // Draw wireframe
            spriteBatch.DrawRectangle(
                destinationRectangle,
                Color.Green,
                thickness: 1f,
                layerDepth: Constants.LayerDepthWireframe
            );

            // Draw origin
            spriteBatch.DrawCircle(
                _spriteX + (_spriteWidth * CurrentActorData.Origin.X),
                _spriteY + (_spriteHeight * CurrentActorData.Origin.Y),
                radius: 5f,
                sides: 10,
                color: Color.White,
                thickness: 1f,
                layerDepth: Constants.LayerDepthNode);
        }
    }

    public HashSet<InputContext> GetInputContexts()
    {
        return [_inputContextManager.GetOrCreateByName(nameof(ActorEditor))];
    }

    public InputEventConsumeResult OnInputEvent(InputEventArgs args)
    {
        if (args.EventType == InputEventType.RMBClicked)
        {
            OnRightClick(args.GetCursorPosition());
        }
        return new();
    }
}