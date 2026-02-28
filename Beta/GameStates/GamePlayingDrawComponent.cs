using System;
using Beta.CommandManagement;
using Beta.Cursors;
using Beta.DI;
using Beta.Entities;
using Beta.Scenes;
using Beta.Verbs;
using Beta.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Beta.AdditionalUi;
using Beta.Common;

namespace Beta.GameStates;

public class GamePlayingDrawComponent
{
    private readonly CommandManager _commandManager;
    private readonly SceneManager _sceneManager;
    private readonly EntityManager _entityManager;
    private readonly AdditionalUiManager _additionalUiManager;
    private readonly ITextManager _textManager;
    private readonly OrthographicCamera _camera;
    private readonly Texture2D _exitArrowTexture;
    private static Color _hotspotsOverlayColor = new(20, 20, 20, 150);
    private static Color _hotspotsHighlightColor = new(100, 100, 100, 10);

    public GamePlayingDrawComponent()
    {
        _commandManager = DependencyContainer.Instance.Get<CommandManager>();
        _sceneManager = DependencyContainer.Instance.Get<SceneManager>();
        _entityManager = DependencyContainer.Instance.Get<EntityManager>();
        _additionalUiManager = DependencyContainer.Instance.Get<AdditionalUiManager>();
        _textManager = DependencyContainer.Instance.Get<ITextManager>();
        _camera = DependencyContainer.Instance.Get<OrthographicCamera>();
        _exitArrowTexture = DependencyContainer.Instance.Get<ContentCache>().Get<Texture2D>("img/cursor/exit");
    }

    public void Draw(
        SpriteBatch spriteBatch,
        Vector2 tooltipPos,
        Vector2 move,
        VerbPickerMenu verbMenu,
        bool isShowHotspots,
        CursorHoverSubject hoverSubject,
        SceneProp? prop,
        Entity? entity
    )
    {
        _commandManager.Draw(spriteBatch);
        _sceneManager.Draw(spriteBatch);
        _additionalUiManager.Draw(spriteBatch);

        if (verbMenu.IsOpen)
        {
            verbMenu.Draw(spriteBatch);
        }
        else
        {
            switch (hoverSubject)
            {
                case CursorHoverSubject.Entity:
                    if (entity is null)
                    {
                        return;
                    }
                    _textManager.WriteLine(
                        spriteBatch,
                        entity.Name,
                        new TextWriteArgs
                        {
                            FontBinding = TextManagerModule.Hint,
                            Position = tooltipPos,
                            Color = Color.White,
                            TextAlignment = TextAlignment.Center,
                        }
                    );
                    break;
                case CursorHoverSubject.Prop:
                    if (prop is null)
                    {
                        return;
                    }
                    _textManager.WriteLine(
                        spriteBatch,
                        prop.Name,
                        new TextWriteArgs
                        {
                            FontBinding = TextManagerModule.Hint,
                            Position = tooltipPos,
                            Color = Color.White,
                            TextAlignment = TextAlignment.Center,
                        }
                    );
                    break;
                case CursorHoverSubject.Exit:
                    break;
                case CursorHoverSubject.WalkableArea:
                    break;
                case CursorHoverSubject.None:
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unknown hover subject: {hoverSubject}"
                    );
            }
        }

        if (isShowHotspots)
        {
            DrawHotspots(spriteBatch);
        }

        if (Settings.IsDebug)
        {
            DrawDebugInfo(spriteBatch, hoverSubject, move);
        }
    }

    private void DrawHotspots(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(
            new RectangleF(0, 0, _camera.BoundingRectangle.Width, _camera.BoundingRectangle.Height),
            color: _hotspotsOverlayColor,
            layerDepth: Constants.LayerDepthGui);

        if (_sceneManager.CurrentScene is null)
        {
            throw new InvalidOperationException("No scene defined.");
        }

        const float radius = 36f;
        const float thickness = 16f;
        const int sides = 32;
        foreach (var entity in _entityManager.GetOnScene(_sceneManager.CurrentScene))
        {
            var rect = entity.GetBoundingRect();
            spriteBatch.DrawCircle(
                center: rect.Center.ToVector2(),
                radius: radius,
                thickness: thickness,
                sides: sides,
                color: _hotspotsHighlightColor,
                layerDepth: Constants.LayerDepthText
            );
        }
        foreach (var prop in _sceneManager.CurrentScene.Props)
        {
            var rect = prop.Polygon.BoundingRectangle;
            spriteBatch.DrawCircle(
                center: rect.Center,
                radius: radius,
                thickness: thickness,
                sides: sides,
                color: _hotspotsHighlightColor,
                layerDepth: Constants.LayerDepthText
            );
        }

        const int exitArrowWidth = 64;
        var exitArrowHeight = exitArrowWidth * _exitArrowTexture.Height / _exitArrowTexture.Width;
        var sceneCenter = _camera.ScreenToWorld(_camera.Center);
        foreach (var exit in _sceneManager.CurrentScene.Exits)
        {
            var pos = exit.Polygon.BoundingRectangle.Center;
            spriteBatch.Draw(
                _exitArrowTexture,
                sourceRectangle: new Rectangle(
                    0, 0, _exitArrowTexture.Width, _exitArrowTexture.Height
                ),
                destinationRectangle: new Rectangle(
                    (int)pos.X,
                    (int)pos.Y,
                    exitArrowWidth,
                    exitArrowHeight
                ),
                color: Color.White * 0.6f,
                rotation: Cursor.GetExitArrowRotation(_camera.ScreenToWorld(pos), sceneCenter),
                origin: Vector2.Zero,
                effects: SpriteEffects.None,
                layerDepth: Constants.LayerDepthCursor
            );
        }
    }

    private void DrawDebugInfo(
        SpriteBatch spriteBatch,
        CursorHoverSubject hoverSubject,
        Vector2 move)
    {
        switch (hoverSubject)
        {
            case CursorHoverSubject.Entity:
                break;
            case CursorHoverSubject.Exit:
                {
                    break;
                    // Disabled, often crashes, probably due to some state inconsistency.

                    /*
                    Vector2 exitPoint =
                        _exit.Polygon.BoundingRectangle.Center;
                    exitPoint.Y = _exit.Polygon.Bottom;

                    Vector2 closestPoint = 
                        _sceneManager.CurrentScene.Meta.WalkableAreas.First().Polygon.GetClosestEdgePoint(
                            exitPoint
                        );

                    var path = _sceneManager.CurrentScene.Meta.MakePath(
                        _entityManager.Player.Position,
                        closestPoint
                    );

                    spriteBatch.DrawPoint(exitPoint, Color.CadetBlue, 20);
                    spriteBatch.DrawPoint(closestPoint, Color.CadetBlue, 20);
                    for (var i = 0; i < path.Count; ++i)
                    {
                        spriteBatch.DrawPoint(path[i], Color.Red, 5);
                        if (i < path.Count-1)
                        {
                            spriteBatch.DrawLine(path[i], path[i+1], Color.White, 3);
                        }
                    }
                    break;
                    */
                }
            case CursorHoverSubject.WalkableArea:
                {
                    if (_sceneManager.CurrentScene is null || _entityManager.Player is null)
                    {
                        throw new InvalidOperationException("No scene or player defined.");
                    }
                    var path = _sceneManager.CurrentScene.MakePath(_entityManager.Player.Position, move);

                    foreach (var point in path)
                    {
                        for (var i = 0; i < path.Count; ++i)
                        {
                            spriteBatch.DrawPoint(path[i], Color.Red, 5);
                            if (i < path.Count - 1)
                            {
                                spriteBatch.DrawLine(
                                    path[i],
                                    path[i + 1],
                                    Color.White,
                                    3,
                                    layerDepth: Constants.LayerDepthScene + Constants.LayerDepthStep * 3
                                );
                            }
                        }
                    }

                    break;
                }

            case CursorHoverSubject.Prop:
                break;
            case CursorHoverSubject.None:
                break;
            default:
                throw new InvalidOperationException(
                    $"Unknown hover subject: {hoverSubject}"
                );
        }
    }
}