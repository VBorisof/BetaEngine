using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Beta.DI;
using System.IO;
using MonoGame.Extended;
using Beta.Input;
using System.Collections.Generic;
using Beta.Logging;
using System.Linq;
using System;
using System.Text.Json;
using Beta.Entities.Animations;
using System.Globalization;
using aced.Models;
using Beta.Common.Extensions;
using aced.Exceptions;
using aced.Input;

namespace aced.Editors;

public class SceneEditor : IInputEventListener
{
    private SceneEditorState _state = SceneEditorState.None;
    private readonly ILogger _logger;
    private readonly GraphicsDeviceManager _graphics;
    private readonly InputService _input;
    private readonly InputContextManager _inputContextManager;
    public RectangleF Viewport { get; } = new(400, 100, 1120, 900);

    private const float CamSpeed = 10f;
    private const float CamZoomSpeed = 0.3f;
    private Vector2 _camPos = new(0, 0);
    private float _camZoom = 1f;

    public SceneData CurrentScene { get; private set; }

    private readonly KeyboardMap _keyboardMap;
    private Texture2D _sceneTexture;

    private const float NodeSelectionRadius = 20f;
    private SceneNode _selectedNode;
    private SceneNode _hoveredNode;
    private SceneActor _hoveredActor;
    private SceneLight _hoveredLight;

    public SceneExit SelectedExit { get; private set; }
    public SceneRegion SelectedRegion { get; private set; }
    public SceneProp SelectedProp { get; private set; }
    public SceneWalkbehind SelectedWalkbehind { get; private set; }
    public SceneActor SelectedActor { get; private set; }
    public SceneLight SelectedLight { get; private set; }

    public event EventHandler<Vector2> SceneActorPositionChanged = (_, _) => { };
    public event EventHandler<float> SceneActorScaleChanged = (_, _) => { };
    public event EventHandler<SceneActor> SceneActorSelectedOnScene = (_, _) => { };

    public event EventHandler<SceneLight> SceneLightSelectedOnScene = (_, _) => { };
    public event EventHandler<Vector3Model> SceneLightPositionChanged = (_, _) => { };

    public SceneEditor()
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _graphics = DependencyContainer.Instance.Get<GraphicsDeviceManager>();
        CurrentScene = new SceneData();

        _keyboardMap = DependencyContainer.Instance.Get<KeyboardMap>();

        _input = DependencyContainer.Instance.Get<InputService>();
        _inputContextManager = DependencyContainer.Instance.Get<InputContextManager>();
    }

    public void ChangeSceneName(string name)
    {
        CurrentScene.Name = name;
    }

    public void SetSceneTexture(string filename)
    {
        using (var fs = new FileStream(filename, FileMode.Open))
        {
            _sceneTexture = Texture2D.FromStream(_graphics.GraphicsDevice, fs);
        }
        CurrentScene.TexturePath = filename;
    }

    public void EditWalkableAreas()
    {
        ResetState();
        _state = SceneEditorState.EditWalkableArea;
        if (CurrentScene.WalkableAreas.Count != 0)
        {
            CurrentScene.WalkableAreas.Add(new WalkableArea());
        }
    }

    public void EditScaleMap()
    {
        ResetState();
        _state = SceneEditorState.EditScaleMap;
    }
    public void SetScaleMapMinScale(float value)
    {
        CurrentScene.ScaleMap.MinScale = value;
        CurrentScene.ScaleMap.ClearMap();
        CurrentScene.ScaleMap.BuildMap();
    }

    //
    // Exits
    //
    public SceneExit AddExit()
    {
        var exit = new SceneExit();
        CurrentScene.Exits.Add(exit);

        return exit;
    }
    public void SetCurrentExit(SceneExit exit)
    {
        SelectedExit = exit;
        _state = SceneEditorState.EditExit;
    }
    public void DeleteExit(SceneExit exit)
    {
        if (!CurrentScene.Exits.Any(e => e == exit))
        {
            _logger.Error("Exit to delete is not found.");
            return;
        }

        CurrentScene.Exits.Remove(exit);

        if (SelectedExit == exit)
        {
            SelectedExit = null;
            if (_state is SceneEditorState.EditExit or SceneEditorState.EditExitNode)
            {
                _state = SceneEditorState.None;
            }
        }
    }
    public bool IsEditingGivenExit(SceneExit exit)
    {
        return exit == SelectedExit
            && (_state is SceneEditorState.EditExit or SceneEditorState.EditExitNode);
    }


    //
    // Actor
    //
    public SceneActor AddActorToScene(string filePath)
    {
        var sceneActor = ReadActor(filePath);
        CurrentScene.Actors.Add(sceneActor);
        return sceneActor;
    }
    public void SetCurrentActor(SceneActor actor)
    {
        _state = SceneEditorState.EditActor;
        SelectedActor = actor;
    }
    public void DeleteActor(SceneActor actor)
    {
        if (!CurrentScene.Actors.Any(a => a == actor))
        {
            _logger.Error("Actor to delete is not found.");
            return;
        }

        CurrentScene.Actors.Remove(actor);

        if (SelectedActor == actor)
        {
            SelectedActor = null;
            if (_state is SceneEditorState.EditActor or SceneEditorState.ScaleActor)
            {
                _state = SceneEditorState.None;
            }
        }
    }
    public SceneActor AddChildActorToSelectedActor(string filePath)
    {
        var sceneActor = ReadActor(filePath);

        SelectedActor.Children.Add(sceneActor);
        sceneActor.Parent = SelectedActor;

        return sceneActor;
    }
    public void DeleteChildActor(SceneActor actor)
    {
        actor.Parent.Children.Remove(actor);

        if (IsEditingGivenActor(actor))
        {
            SelectedActor = actor.Parent;
        }
        actor.Parent = null;
    }

    private void OnActorScale()
    {
        if (_state != SceneEditorState.EditActor || SelectedActor == null)
        {
            return;
        }

        _state = SceneEditorState.ScaleActor;
    }
    public bool IsEditingGivenActor(SceneActor actor)
    {
        return actor == SelectedActor
            && _state == SceneEditorState.EditActor;
    }
    public void SetSelectedActorState(string state)
    {
        SelectedActor.State = state;
        SetAnimationBasedOnState(state);
    }

    public void SetSelectedActorCostume(string costumeName)
    {
        var costume = SelectedActor.Costumes.SingleOrDefault(c =>
                string.Equals(c.Name, costumeName, StringComparison.OrdinalIgnoreCase));

        if (costume is null)
        {
            _logger.Warning($"Invalid costume for {SelectedActor.Name}: {costumeName}");
            return;
        }
        else
        {
            SelectedActor.CurrentCostume = costume;
        }

        SetAnimationBasedOnState(SelectedActor.State);
    }

    private void SetAnimationBasedOnState(string state)
    {
        var newAnim = SelectedActor.CurrentCostume.Animations
            .SingleOrDefault(a =>
                string.Equals(a.Name, state, StringComparison.OrdinalIgnoreCase));

        if (newAnim is null)
        {
            _logger.Warning($"Invalid state for {SelectedActor.Name}: {state}");
        }
        else
        {
            SelectedActor.CurrentAnimation = newAnim;
        }
    }


    //
    // Props
    //
    public SceneProp AddProp()
    {
        var prop = new SceneProp();
        CurrentScene.Props.Add(prop);

        return prop;
    }
    public void SetCurrentProp(SceneProp prop)
    {
        SelectedProp = prop;
        _state = SceneEditorState.EditProp;
    }
    public void DeleteProp(SceneProp prop)
    {
        if (!CurrentScene.Props.Any(p => p == prop))
        {
            _logger.Error("Prop to delete is not found.");
            return;
        }

        CurrentScene.Props.Remove(prop);

        if (SelectedProp == prop)
        {
            SelectedProp = null;
            if (_state is SceneEditorState.EditProp or SceneEditorState.EditPropNode)
            {
                _state = SceneEditorState.None;
            }
        }
    }
    public bool IsEditingGivenProp(SceneProp prop)
    {
        return prop == SelectedProp
            && (_state == SceneEditorState.EditProp
                || _state == SceneEditorState.EditPropNode);
    }

    //
    // Walkbehinds
    //
    public SceneWalkbehind AddWalkbehind()
    {
        var walkbehind = new SceneWalkbehind();
        CurrentScene.Walkbehinds.Add(walkbehind);

        return walkbehind;
    }
    public void SetCurrentWalkbehind(SceneWalkbehind walkbehind)
    {
        SelectedWalkbehind = walkbehind;
        _state = SceneEditorState.EditWalkbehind;
    }
    public void DeleteWalkbehind(SceneWalkbehind walkbehind)
    {
        if (!CurrentScene.Walkbehinds.Any(p => p == walkbehind))
        {
            _logger.Error("Walkbehind to delete is not found.");
            return;
        }

        CurrentScene.Walkbehinds.Remove(walkbehind);

        if (SelectedWalkbehind == walkbehind)
        {
            SelectedWalkbehind = null;
            if (_state is SceneEditorState.EditWalkbehind)
            {
                _state = SceneEditorState.None;
            }
        }
    }
    public bool IsEditingGivenWalkbehind(SceneWalkbehind walkbehind)
    {
        return walkbehind == SelectedWalkbehind
            && (_state == SceneEditorState.EditWalkbehind);
    }


    //
    // Regions
    //
    public SceneRegion AddRegion()
    {
        var region = new SceneRegion();
        CurrentScene.Regions.Add(region);

        return region;
    }
    public void SetCurrentRegion(SceneRegion region)
    {
        SelectedRegion = region;
        _state = SceneEditorState.EditRegion;
    }
    public void DeleteRegion(SceneRegion region)
    {
        if (!CurrentScene.Regions.Any(r => r == region))
        {
            _logger.Error("Region to delete is not found.");
            return;
        }

        CurrentScene.Regions.Remove(region);

        if (SelectedRegion == region)
        {
            SelectedRegion = null;
            if (_state is SceneEditorState.EditRegion or SceneEditorState.EditRegionNode)
            {
                _state = SceneEditorState.None;
            }
        }
    }
    public bool IsEditingGivenRegion(SceneRegion region)
    {
        return region == SelectedRegion
            && (_state == SceneEditorState.EditRegion
                || _state == SceneEditorState.EditRegionNode);
    }

    //
    // Lights
    //
    public SceneLight AddLight()
    {
        var light = new SceneLight();
        CurrentScene.Lights.Add(light);

        return light;
    }
    public void SetCurrentLight(SceneLight light)
    {
        SelectedLight = light;
        _state = SceneEditorState.EditLight;
    }
    public void DeleteLight(SceneLight light)
    {
        if (!CurrentScene.Lights.Any(r => r == light))
        {
            _logger.Error("Light to delete is not found.");
            return;
        }

        CurrentScene.Lights.Remove(light);

        if (SelectedLight == light)
        {
            SelectedLight = null;
            if (_state is SceneEditorState.EditLight)
            {
                _state = SceneEditorState.None;
            }
        }
    }
    public bool IsEditingGivenLight(SceneLight light)
    {
        return light == SelectedLight;
    }


    public void ResetEnvironment()
    {
        DestroyEnvironment();
    }

    public void DestroyEnvironment()
    {
        CurrentScene.Name = "";
        CurrentScene.TexturePath = "";
        CurrentScene.WalkableAreas.Clear();
        CurrentScene.WalkableAreas.Add(new WalkableArea());
        CurrentScene.Exits.Clear();
        CurrentScene.Regions.Clear();
        CurrentScene.Lights.Clear();
        CurrentScene.Walkbehinds.Clear();
        CurrentScene.Props.Clear();
        CurrentScene.ScaleMap = new SceneScaleMap();
        CurrentScene.Actors.Clear();

        ResetState();

        _camPos = Vector2.Zero;
        _camZoom = 1f;
        _sceneTexture = null;
    }

    public void SoftResetState()
    {
        _state = SceneEditorState.None;
    }
    public void ResetState()
    {
        _state = SceneEditorState.None;
        _selectedNode = _hoveredNode = null;
        SelectedActor = _hoveredActor = null;
        SelectedLight = _hoveredLight = null;
        SelectedExit = null;
        SelectedRegion = null;
        SelectedProp = null;
        SelectedWalkbehind = null;
    }

    private void OnLeftClick(Vector2 clickPos)
    {
        var isClickOnScene = ScreenToScene(clickPos, out var scenePos);

        if (!isClickOnScene)
        {
            return;
        }

        // TODO: Refactor this switch... can definitely extract a function.

        switch (_state)
        {
            case SceneEditorState.None:
                {
                    // Try to find some child first.
                    SelectedActor = CurrentScene.Actors.SelectMany(a => a.Children).FirstOrDefault(c =>
                        c.GetBoundingRect(
                            c.Parent.Position.ToVector2() + c.Position.ToVector2(),
                            _camZoom,
                            CurrentScene.ScaleMap.GetScale(c.Parent.Position.ToVector2() + c.Position.ToVector2())
                        ).Contains(scenePos)
                    );

                    // If found, break.
                    if (SelectedActor != null)
                    {
                        _state = SceneEditorState.EditActor;
                        return;
                    }

                    // Otherwise, try to find in actors.
                    SelectedActor = CurrentScene.Actors.FirstOrDefault(c =>
                        c.GetBoundingRect(
                            c.Position.ToVector2(),
                            _camZoom,
                            CurrentScene.ScaleMap.GetScale(c.Position.ToVector2())
                        ).Contains(scenePos)
                    );
                    if (SelectedActor != null)
                    {
                        _state = SceneEditorState.EditActor;
                        SceneActorSelectedOnScene.Invoke(this, SelectedActor);
                        return;
                    }

                    // Otherwise, try to find in lights.
                    SelectedLight = CurrentScene.Lights.FirstOrDefault(l =>
                        new CircleF(new Vector2(l.LightPosition.X, l.LightPosition.Y), 50)
                            .Contains(scenePos)
                        );
                    if (SelectedLight is not null)
                    {
                        _state = SceneEditorState.EditLight;
                        SceneLightSelectedOnScene.Invoke(this, SelectedLight);
                        return;
                    }
                    break;
                }
            case SceneEditorState.EditWalkableArea:
                {
                    SelectOrCreateNode(
                        CurrentScene.WalkableAreas[0],
                        SceneEditorState.EditWalkableAreaNode,
                        scenePos);
                    break;
                }
            case SceneEditorState.EditScaleMap:
                {
                    SelectOrCreateNode(
                        CurrentScene.ScaleMap,
                        SceneEditorState.EditScaleMapNode,
                        scenePos);
                    break;
                }
            case SceneEditorState.EditExit:
                {
                    SelectOrCreateNode(
                        SelectedExit,
                        SceneEditorState.EditExitNode,
                        scenePos);
                    break;
                }
            case SceneEditorState.EditRegion:
                {
                    SelectOrCreateNode(
                        SelectedRegion,
                        SceneEditorState.EditRegionNode,
                        scenePos);
                    break;
                }
            case SceneEditorState.EditProp:
                {
                    SelectOrCreateNode(
                        SelectedProp,
                        SceneEditorState.EditPropNode,
                        scenePos);
                    break;
                }
            case SceneEditorState.EditWalkbehind:
                {
                    SelectOrCreateNode(
                        SelectedWalkbehind,
                        SceneEditorState.EditWalkbehindNode,
                        scenePos);
                    break;
                }
            case SceneEditorState.EditWalkableAreaNode:
                {
                    _selectedNode = null;
                    _state = SceneEditorState.EditWalkableArea;
                    break;
                }
            case SceneEditorState.EditScaleMapNode:
                {
                    _selectedNode = null;
                    _state = SceneEditorState.EditScaleMap;
                    break;
                }
            case SceneEditorState.EditExitNode:
                {
                    _selectedNode = null;
                    _state = SceneEditorState.EditExit;
                    break;
                }
            case SceneEditorState.EditRegionNode:
                {
                    _selectedNode = null;
                    _state = SceneEditorState.EditRegion;
                    break;
                }
            case SceneEditorState.EditPropNode:
                {
                    _selectedNode = null;
                    _state = SceneEditorState.EditProp;
                    break;
                }
            case SceneEditorState.EditWalkbehindNode:
                {
                    _selectedNode = null;
                    _state = SceneEditorState.EditWalkbehind;
                    break;
                }
            case SceneEditorState.EditActor:
                {
                    SelectedActor = null;
                    _state = SceneEditorState.None;
                    break;
                }
            case SceneEditorState.EditLight:
                {
                    _state = SceneEditorState.None;
                    break;
                }
            case SceneEditorState.ScaleActor:
                {
                    SelectedActor = null;
                    _state = SceneEditorState.None;
                    break;
                }
            default:
                break;
        }
    }

    private void OnRightClick(Vector2 clickPos)
    {
        var isClickOnScene = ScreenToScene(clickPos, out var scenePos);

        if (!isClickOnScene)
        {
            return;
        }

        switch (_state)
        {
            case SceneEditorState.EditScaleMap:
                // Define MAX pivot
                if (CurrentScene.ScaleMap.MaxPivot == null)
                {
                    CurrentScene.ScaleMap.MaxPivot = new SceneNode
                    {
                        Id = 0,
                        X = (int)scenePos.X,
                        Y = (int)scenePos.Y
                    };
                }
                // Define MIN pivot
                else if (CurrentScene.ScaleMap.MinPivot == null)
                {
                    CurrentScene.ScaleMap.MinPivot = new SceneNode
                    {
                        Id = 1,
                        X = (int)scenePos.X,
                        Y = (int)scenePos.Y
                    };

                    CurrentScene.ScaleMap.BuildMap();
                }
                else
                {
                    CurrentScene.ScaleMap.MaxPivot = null;
                    CurrentScene.ScaleMap.MinPivot = null;

                    CurrentScene.ScaleMap.ClearMap();
                }
                break;

            case SceneEditorState.EditExit:
                SelectedExit.ExitPoint = new Coord(scenePos.X, scenePos.Y);
                break;
            case SceneEditorState.None:
            case SceneEditorState.EditWalkableArea:
            case SceneEditorState.EditWalkableAreaNode:
            case SceneEditorState.EditScaleMapNode:
            case SceneEditorState.EditExitNode:
            case SceneEditorState.EditRegion:
            case SceneEditorState.EditRegionNode:
            case SceneEditorState.EditProp:
            case SceneEditorState.EditPropNode:
            case SceneEditorState.EditWalkbehind:
            case SceneEditorState.EditWalkbehindNode:
            case SceneEditorState.EditActor:
            case SceneEditorState.EditLight:
            case SceneEditorState.ScaleActor:
            default:
                break;
        }
    }

    private void OnMouseMoved(Vector2 screenPos)
    {
        _hoveredNode = null;
        _hoveredActor = null;
        var isMoveOnScene = ScreenToScene(screenPos, out var scenePos);

        if (!isMoveOnScene)
        {
            return;
        }

        switch (_state)
        {
            case SceneEditorState.EditExit:
                _hoveredNode = SelectedExit.Nodes.FirstOrDefault(n =>
                    (scenePos - new Vector2(n.X, n.Y)).Length() < NodeSelectionRadius
                );
                break;
            case SceneEditorState.EditRegion:
                _hoveredNode = SelectedRegion.Nodes.FirstOrDefault(n =>
                    (scenePos - new Vector2(n.X, n.Y)).Length() < NodeSelectionRadius
                );
                break;
            case SceneEditorState.EditProp:
                _hoveredNode = SelectedProp.Nodes.FirstOrDefault(n =>
                    (scenePos - new Vector2(n.X, n.Y)).Length() < NodeSelectionRadius
                );
                break;
            case SceneEditorState.EditWalkbehind:
                _hoveredNode = SelectedWalkbehind.Nodes.FirstOrDefault(n =>
                    (scenePos - new Vector2(n.X, n.Y)).Length() < NodeSelectionRadius
                );
                break;

            case SceneEditorState.EditWalkableArea:
                _hoveredNode = CurrentScene.WalkableAreas[0].Nodes.FirstOrDefault(n =>
                    (scenePos - new Vector2(n.X, n.Y)).Length() < NodeSelectionRadius
                );
                break;

            case SceneEditorState.EditScaleMap:
                _hoveredNode = CurrentScene.ScaleMap.Nodes.FirstOrDefault(n =>
                    (scenePos - new Vector2(n.X, n.Y)).Length() < NodeSelectionRadius
                );
                break;

            case SceneEditorState.EditExitNode:
            case SceneEditorState.EditRegionNode:
            case SceneEditorState.EditPropNode:
            case SceneEditorState.EditWalkableAreaNode:
            case SceneEditorState.EditScaleMapNode:
            case SceneEditorState.EditWalkbehindNode:
                _selectedNode.X = (int)scenePos.X;
                _selectedNode.Y = (int)scenePos.Y;
                break;

            case SceneEditorState.EditActor:
                if (SelectedActor.Parent == null)
                {
                    SelectedActor.Position = Coord.FromVector2(scenePos);
                    SceneActorPositionChanged.Invoke(this, scenePos);
                }
                else
                {
                    var relPos = scenePos - SelectedActor.Parent.Position.ToVector2();
                    SelectedActor.Position = Coord.FromVector2(relPos);
                }
                break;

            case SceneEditorState.EditLight:
                var orthPos = Coord.FromVector2(scenePos);
                SelectedLight.LightPosition.X = orthPos.X;
                SelectedLight.LightPosition.Y = orthPos.Y;
                SceneLightPositionChanged.Invoke(this, SelectedLight.LightPosition);
                break;

            case SceneEditorState.ScaleActor:
                if (SelectedActor.Parent == null)
                {
                    SelectedActor.Scale = (scenePos - SelectedActor.Position.ToVector2()).Length() / 10f;
                    SceneActorScaleChanged.Invoke(this, SelectedActor.Scale);
                }
                else
                {
                    SelectedActor.Scale = (scenePos - (SelectedActor.Parent.Position.ToVector2() + SelectedActor.Position.ToVector2())).Length() / 10f;
                }
                break;

            case SceneEditorState.None:
                // Try to find some child first.
                _hoveredActor = CurrentScene.Actors.SelectMany(a => a.Children).FirstOrDefault(c =>
                    c.GetBoundingRect(
                        c.Parent.Position.ToVector2() + c.Position.ToVector2(),
                        _camZoom,
                        CurrentScene.ScaleMap.GetScale(c.Parent.Position.ToVector2() + c.Position.ToVector2())
                    ).Contains(scenePos)
                );

                // If found, break.
                if (_hoveredActor is not null)
                {
                    break;
                }

                // Otherwise, continue searching through actors.
                _hoveredActor = CurrentScene.Actors.FirstOrDefault(c =>
                    c.GetBoundingRect(
                        c.Position.ToVector2(),
                        _camZoom,
                        CurrentScene.ScaleMap.GetScale(c.Position.ToVector2())
                    ).Contains(scenePos)
                );

                if (_hoveredActor is not null)
                {
                    break;
                }

                // Otherwise, search through lights.
                _hoveredLight = CurrentScene.Lights.FirstOrDefault(l =>
                    new CircleF(new Vector2(l.LightPosition.X, l.LightPosition.Y), 10)
                        .Contains(scenePos)
                );

                if (_hoveredLight is not null)
                {
                    break;
                }
                break;

            default:
                break;
        }
    }

    ///
    /// Convert screen position to a position on the scene.
    /// Returns false if the position is outside the viewport and true otherwise.
    private bool ScreenToScene(Vector2 screenPos, out Vector2 scenePos)
    {
        // If we are out of the viewport, return false.
        if (screenPos.X < Viewport.X || screenPos.X > Viewport.Right
            || screenPos.Y < Viewport.Y || screenPos.Y > Viewport.Bottom)
        {
            scenePos = Vector2.Zero;
            return false;
        }

        scenePos = screenPos - Viewport.TopLeft;
        scenePos *= _camZoom;
        scenePos += _camPos;

        return true;
    }

    private void OnDelete()
    {
        switch (_state)
        {
            case SceneEditorState.EditWalkableArea:
                if (_hoveredNode == null)
                {
                    return;
                }
                CurrentScene.WalkableAreas[0].Nodes.Remove(_hoveredNode);
                break;
            case SceneEditorState.EditScaleMap:
                if (_hoveredNode == null)
                {
                    return;
                }
                CurrentScene.ScaleMap.Nodes.Remove(_hoveredNode);
                break;
            case SceneEditorState.EditExit:
                if (_hoveredNode == null)
                {
                    return;
                }
                SelectedExit.Nodes.Remove(_hoveredNode);
                break;
            case SceneEditorState.EditRegion:
                if (_hoveredNode == null)
                {
                    return;
                }
                SelectedRegion.Nodes.Remove(_hoveredNode);
                break;
            case SceneEditorState.EditProp:
                if (_hoveredNode == null)
                {
                    return;
                }
                SelectedProp.Nodes.Remove(_hoveredNode);
                break;
            case SceneEditorState.EditWalkbehind:
                if (_hoveredNode == null)
                {
                    return;
                }
                SelectedWalkbehind.Nodes.Remove(_hoveredNode);
                break;
            case SceneEditorState.EditActor:
                if (SelectedActor.Parent == null)
                {
                    CurrentScene.Actors.Remove(SelectedActor);
                }
                else
                {
                    SelectedActor.Parent.Children.Remove(SelectedActor);
                    SelectedActor.Parent = null;
                }
                ResetState();
                break;
            case SceneEditorState.EditLight:
                CurrentScene.Lights.Remove(SelectedLight);
                ResetState();
                break;
            case SceneEditorState.None:
            case SceneEditorState.EditWalkableAreaNode:
            case SceneEditorState.EditScaleMapNode:
            case SceneEditorState.EditExitNode:
            case SceneEditorState.EditRegionNode:
            case SceneEditorState.EditPropNode:
            case SceneEditorState.ScaleActor:
            default:
                break;
        }
    }
    public SceneActor ReadActor(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new ActorLoadException($"File doesn't exist: {filePath}");
        }

        var json = File.ReadAllText(filePath);

        var actorData = JsonSerializer.Deserialize<ActorData>(json);

        var costumes = new List<SceneActorCostume>();
        foreach (var costume in actorData.Costumes)
        {
            var animations = new List<Animation>();
            foreach (var anim in costume.Animations)
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
            costumes.Add(new SceneActorCostume
            {
                Name = costume.Name,
                Animations = animations
            });
        }

        var defaultCostume = costumes
            .Single(c =>
                string.Equals(c.Name, "default", StringComparison.OrdinalIgnoreCase)
            );
        var sceneActor = new SceneActor
        {
            Name = actorData.Name,
            Position = new Coord(0, 0),
            Scale = 10f,
            Actor = actorData,
            CurrentCostume = defaultCostume,
            Costumes = costumes,
            CurrentAnimation = defaultCostume.Animations.First(),
        };

        return sceneActor;
    }


    public void Update(GameTime gameTime)
    {
        foreach (var actor in CurrentScene.Actors)
        {
            actor.CurrentAnimation?.Update(gameTime);
            if (actor.IsShowChildren)
            {
                foreach (var child in actor.Children)
                {
                    child.CurrentAnimation?.Update(gameTime);
                }
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // No scene?
        if (_sceneTexture == null)
        {
            return;
        }

        // Draw background.
        spriteBatch.FillRectangle(
            Viewport,
            ColorEx.FromHexString("#474747FF"),
            layerDepth: Constants.LayerDepthViewport
        );

        // Draw the scene.
        spriteBatch.Draw(
            _sceneTexture,
            sourceRectangle: new Rectangle(
                (int)_camPos.X,
                (int)_camPos.Y,
                (int)(Viewport.Width * _camZoom),
                (int)(Viewport.Height * _camZoom)
            ),
            destinationRectangle: Viewport.ToRectangle(),
            effects: SpriteEffects.None,
            rotation: 0,
            origin: Vector2.Zero,
            color: Color.White,
            layerDepth: Constants.LayerDepthScene
        );

        spriteBatch.DrawRectangle(
            Viewport.X - (_camPos.X / _camZoom),
            Viewport.Y - (_camPos.Y / _camZoom),
            _sceneTexture.Width / _camZoom,
            _sceneTexture.Height / _camZoom,
            Color.CornflowerBlue,
            thickness: 5f,
            layerDepth: Constants.LayerDepthWireframe
        );

        if (Settings.IsDrawWalkableAreas)
        {
            DrawWalkableAreas(spriteBatch);
        }
        if (Settings.IsDrawScaleMap)
        {
            DrawScaleMap(spriteBatch);
        }
        if (Settings.IsDrawExits)
        {
            DrawExits(spriteBatch);
        }
        if (Settings.IsDrawRegions)
        {
            DrawRegions(spriteBatch);
        }
        if (Settings.IsDrawProps)
        {
            DrawProps(spriteBatch);
        }
        if (Settings.IsDrawWalkbehinds)
        {
            DrawWalkbehinds(spriteBatch);
        }
        if (Settings.IsDrawActors)
        {
            DrawActors(spriteBatch);
        }
        if (Settings.IsDrawLights)
        {
            DrawLights(spriteBatch);
        }

        spriteBatch.DrawRectangle(Viewport, Color.Black, 5f);
    }

    public void Export()
    {
        if (string.IsNullOrWhiteSpace(CurrentScene.Name))
        {
            _logger.Error("Exported scene name cannot be empty");
            return;
        }

        var scenePathSuffix = $"scenes/{CurrentScene.Name}";
        var sceneAssetPath = $"{Settings.ASSET_BASE_PATH}/{scenePathSuffix}";

        // TODO: Create export dir
        // 
        if (!Directory.Exists(sceneAssetPath))
        {
            if (Settings.IsDryRun)
            {
                _logger.Info($"[DR] Create directory: {sceneAssetPath}");
            }
            else
            {
                Directory.CreateDirectory(sceneAssetPath);
            }
        }

        // Export scalemap
        var scaleMapPath = $"{sceneAssetPath}/{CurrentScene.Name}.scalemap.png";
        if (Settings.IsDryRun)
        {
            _logger.Info($"[DR] Export scale map to `{scaleMapPath}`");
        }
        else
        {
            CurrentScene.ScaleMap.ExportToPng(
                _sceneTexture.Width,
                _sceneTexture.Height,
                _graphics.GraphicsDevice,
                scaleMapPath
            );
            var mgcbScaleMapDefTemplate = File.ReadAllText("Templates/template_scalemap_mgcb.txt");
            mgcbScaleMapDefTemplate = mgcbScaleMapDefTemplate.Replace("$path", scaleMapPath);

            File.AppendAllText($"{Settings.CONTENT_BASE_PATH}/content.mgcb", mgcbScaleMapDefTemplate);
        }

        // Create BDSM definition if doesn't exist.
        var bdsmPath = $"{Settings.SCRIPTS_BASE_PATH}/{scenePathSuffix}.bs";
        if (!File.Exists(bdsmPath))
        {
            if (Settings.IsDryRun)
            {
                _logger.Info($"[DR] Create BDSM script at `{bdsmPath}`");
            }
            else
            {
                var sceneDefTemplate = File.ReadAllText("Templates/template_scene.txt");
                sceneDefTemplate = sceneDefTemplate.Replace("$declname", CurrentScene.Name);
                var titledName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(CurrentScene.Name);
                sceneDefTemplate = sceneDefTemplate.Replace("$name", titledName);

                File.WriteAllText(bdsmPath, sceneDefTemplate);
            }
        }

        // Add to content pipeline...
        var destFile = $"{sceneAssetPath}/{CurrentScene.Name}.png";
        if (destFile != $"{Settings.CONTENT_BASE_PATH}/{CurrentScene.TexturePath}")
        {
            if (Settings.IsDryRun)
            {
                _logger.Info($"[DR] Export scene texture to `{destFile}`");
            }
            else
            {
                File.Copy(CurrentScene.TexturePath, destFile, overwrite: true);
                CurrentScene.TexturePath = $"{scenePathSuffix}/{CurrentScene.Name}.png";

                var mgcbSceneDefTemplate = File.ReadAllText("Templates/template_scene_mgcb.txt");
                mgcbSceneDefTemplate = mgcbSceneDefTemplate.Replace("$path", $"{scenePathSuffix}/{CurrentScene.Name}.png");

                File.AppendAllText($"{Settings.CONTENT_BASE_PATH}/content.mgcb", mgcbSceneDefTemplate);
            }
        }

        // TODO: Cache file for mgcb so that we know we already added an asset.

        // Export scene JSON.
        var json = JsonSerializer.Serialize(CurrentScene);
        var jsonPath = $"{Settings.JSON_RES_BASE_PATH}/{scenePathSuffix}";
        if (Settings.IsDryRun)
        {
            _logger.Info($"[DR] Write scene JSON to `{jsonPath}/{CurrentScene.Name}.json`");
            _logger.Info($"[DR]");
            _logger.Info($"[DR] {json}");
        }
        else
        {
            if (!Directory.Exists(jsonPath))
            {
                Directory.CreateDirectory(jsonPath);
            }
            File.WriteAllText($"{jsonPath}/{CurrentScene.Name}.json", json);
        }
    }

    public void OpenScene(string filePath)
    {
        if (!File.Exists(filePath))
        {
            _logger.Error($"File doesn't exist: {filePath}");
            return;
        }

        ResetEnvironment();

        var json = File.ReadAllText(filePath);

        CurrentScene = JsonSerializer.Deserialize<SceneData>(json);

        foreach (var actor in CurrentScene.Actors)
        {
            // Load actor...
            var actorPath = $"{Settings.JSON_RES_BASE_PATH}/actors/{actor.Name}/{actor.Name}.actor.json";

            var sceneActor = ReadActor(actorPath);

            actor.Actor = sceneActor.Actor;
            actor.CurrentCostume = sceneActor.CurrentCostume;
            actor.Costumes = sceneActor.Costumes;
            actor.CurrentAnimation = actor.CurrentCostume.Animations.FirstOrDefault();
            actor.IsShowChildren = sceneActor.IsShowChildren;
            actor.State = sceneActor.State;

            foreach (var child in actor.Children)
            {
                var childPath = $"{Settings.JSON_RES_BASE_PATH}/actors/{child.Name}/{child.Name}.actor.json";
                var sceneChildActor = ReadActor(childPath);

                child.Parent = actor;
                child.Actor = sceneChildActor.Actor;
                child.CurrentCostume = sceneChildActor.CurrentCostume;
                child.Costumes = sceneChildActor.Costumes;
                child.CurrentAnimation = sceneChildActor.CurrentCostume.Animations.FirstOrDefault();
                child.IsShowChildren = sceneChildActor.IsShowChildren;
                child.State = sceneChildActor.State;
            }
        }

        var textureRelativePath = CurrentScene.TexturePath;
        var textureAbsolutePath = $"{Settings.ASSET_BASE_PATH}/{textureRelativePath}";
        using (var fs = new FileStream(textureAbsolutePath, FileMode.Open))
        {
            _sceneTexture = Texture2D.FromStream(_graphics.GraphicsDevice, fs);
        }

        CurrentScene.ScaleMap?.BuildMap();
    }

    private void DrawWalkableAreas(SpriteBatch spriteBatch)
    {
        DrawNodesItems(
            spriteBatch,
            CurrentScene.WalkableAreas,
            colorSelector: (_) => Color.Red
        );
    }

    private void DrawScaleMap(SpriteBatch spriteBatch)
    {
        DrawNodesItems(
            spriteBatch,
            [CurrentScene.ScaleMap],
            colorSelector: (_) => Color.Yellow
        );

        // Draw the pivots
        if (CurrentScene.ScaleMap.MaxPivot != null)
        {
            var node = CurrentScene.ScaleMap.MaxPivot;
            var nodePos = new Vector2(node.X, node.Y);
            nodePos -= _camPos;
            nodePos /= _camZoom;
            nodePos += Viewport.TopLeft;

            // Draw the node
            spriteBatch.DrawCircle(
                nodePos.X, nodePos.Y,
                color: Color.White,
                radius: 5f,
                sides: 10,
                layerDepth: Constants.LayerDepthNode
            );
        }
        if (CurrentScene.ScaleMap.MinPivot != null)
        {
            var node = CurrentScene.ScaleMap.MinPivot;
            var nodePos = new Vector2(node.X, node.Y);
            nodePos -= _camPos;
            nodePos /= _camZoom;
            nodePos += new Vector2(Viewport.X, Viewport.Y);

            // Draw the node
            spriteBatch.DrawCircle(
                nodePos.X, nodePos.Y,
                color: Color.White,
                radius: 5f,
                sides: 10,
                layerDepth: Constants.LayerDepthNode
            );
        }
    }

    private void DrawExits(SpriteBatch spriteBatch)
    {
        Func<ISceneNodeList, Color> colorSelector = (exit) =>
            exit == SelectedExit ? Color.LimeGreen : Color.Green;
        DrawNodesItems(
            spriteBatch,
            CurrentScene.Exits,
            colorSelector: colorSelector
        );

        foreach (var exit in CurrentScene.Exits)
        {
            // Draw ExitPoint
            var exitPointPos = new Vector2(exit.ExitPoint.X, exit.ExitPoint.Y);
            exitPointPos -= _camPos;
            exitPointPos /= _camZoom;
            exitPointPos += Viewport.TopLeft;

            // Draw the node
            spriteBatch.DrawCircle(
                exitPointPos.X, exitPointPos.Y,
                color: colorSelector(exit),
                radius: 5f,
                sides: 10,
                layerDepth: Constants.LayerDepthNode
            );
        }
    }

    private void DrawRegions(SpriteBatch spriteBatch)
    {
        Func<ISceneNodeList, Color> colorSelector = (region) =>
            region == SelectedRegion ? Color.LightBlue : Color.Blue;
        DrawNodesItems(
            spriteBatch,
            CurrentScene.Regions,
            colorSelector: colorSelector
        );
    }

    private void DrawProps(SpriteBatch spriteBatch)
    {
        Func<ISceneNodeList, Color> colorSelector = (prop) =>
            prop == SelectedProp ? Color.Pink : Color.Purple;
        DrawNodesItems(
            spriteBatch,
            CurrentScene.Props,
            colorSelector: colorSelector
        );
    }

    private void DrawWalkbehinds(SpriteBatch spriteBatch)
    {
        Func<ISceneNodeList, Color> colorSelector = (walkbehind) =>
            walkbehind == SelectedWalkbehind ? Color.White : Color.LightGray;
        DrawNodesItems(
            spriteBatch,
            CurrentScene.Walkbehinds,
            colorSelector: colorSelector
        );
    }

    private void DrawActors(SpriteBatch spriteBatch)
    {
        foreach (var actor in CurrentScene.Actors)
        {
            DrawActor(actor, spriteBatch);
            if (actor.IsShowChildren)
            {
                foreach (var child in actor.Children)
                {
                    DrawActor(child, spriteBatch);
                }
            }
        }
    }

    private void DrawLights(SpriteBatch spriteBatch)
    {
        foreach (var light in CurrentScene.Lights)
        {
            DrawLight(light, spriteBatch);
        }
    }

    private void DrawNodesItems(
        SpriteBatch spriteBatch,
        IEnumerable<ISceneNodeList> sceneNodesItems,
        Func<ISceneNodeList, Color> colorSelector
    )
    {
        foreach (var item in sceneNodesItems)
        {
            var color = colorSelector(item);

            // Draw the Exit
            var sortedNodes = item.Nodes.OrderBy(n => n.Id).ToList();
            for (var i = 0; i < sortedNodes.Count; ++i)
            {
                var node = sortedNodes[i];
                var nodePos = new Vector2(node.X, node.Y);
                nodePos -= _camPos;
                nodePos /= _camZoom;
                nodePos += Viewport.TopLeft;

                // Is the node in the viewport?
                if (Viewport.Contains(nodePos))
                {
                    // Do we want to color-code the node?
                    var nodeColor = Color.Yellow;
                    if (node == _selectedNode)
                    {
                        nodeColor = Color.Green;
                    }
                    if (node == _hoveredNode)
                    {
                        nodeColor = Color.Blue;
                    }

                    // Draw the node
                    spriteBatch.DrawPoint(
                        nodePos.X, nodePos.Y,
                        nodeColor,
                        size: 5f,
                        layerDepth: Constants.LayerDepthNode
                    );
                }

                // Is there a polygon?
                if (sortedNodes.Count > 2)
                {
                    var secondNodeId = 0;
                    // Do we have a next node?
                    if (i < sortedNodes.Count - 1)
                    {
                        secondNodeId = i + 1;
                    }

                    // TODO: 
                    // For the edges, we want to calculate a subset of that edge that we will draw.
                    // Need to find an intersection point...
                    // Kinda ugly, but just clamp it for now.
                    var secondNode = sortedNodes[secondNodeId];
                    var secondNodePos = new Vector2(secondNode.X, secondNode.Y);
                    secondNodePos -= _camPos;
                    secondNodePos /= _camZoom;
                    secondNodePos += Viewport.TopLeft;
                    spriteBatch.DrawLine(
                        Math.Clamp(nodePos.X, Viewport.X, Viewport.Right),
                        Math.Clamp(nodePos.Y, Viewport.Y, Viewport.Bottom),
                        Math.Clamp(secondNodePos.X, Viewport.X, Viewport.Right),
                        Math.Clamp(secondNodePos.Y, Viewport.Y, Viewport.Bottom),
                        color,
                        thickness: 1f,
                        layerDepth: Constants.LayerDepthEdge
                    );
                }
            }
        }
    }

    private void SelectOrCreateNode(
        ISceneNodeList sceneNodeList,
        SceneEditorState stateOnNodeSelect,
        Vector2 scenePos
    )
    {
        // Are we targeting some specific node?
        if (_hoveredNode is not null)
        {
            _selectedNode = _hoveredNode;
            _state = stateOnNodeSelect;
        }
        else // Create a new node.
        {
            var id = 0;
            if (sceneNodeList.Nodes.Count != 0)
            {
                id = sceneNodeList.Nodes.Max(n => n.Id) + 1;
            }
            var node = new SceneNode
            {
                Id = id,
                X = (int)scenePos.X,
                Y = (int)scenePos.Y
            };
            sceneNodeList.Nodes.Add(node);
        }
    }

    private void DrawActor(SceneActor actor, SpriteBatch spriteBatch)
    {
        if (actor.CurrentAnimation is null)
        {
            return;
        }

        var actorPos = actor.Position.ToVector2();
        actorPos -= _camPos;
        actorPos /= _camZoom;
        actorPos += Viewport.TopLeft;

        var frame = actor.CurrentAnimation.GetCurrentFrame();

        var destinationRectangle = actor.GetBoundingRect(
            actorPos,
            _camZoom,
            CurrentScene.ScaleMap.GetScale(actor.Position.ToVector2())
        );

        spriteBatch.Draw(
            frame,
            destinationRectangle: destinationRectangle,
            sourceRectangle: new Rectangle(0, 0, frame.Width, frame.Height),
            effects: SpriteEffects.None,
            rotation: 0,
            origin: Vector2.Zero,
            color: Color.White,
            layerDepth: Constants.LayerDepthActor
        );

        var wireframeColor = Color.LightGray;
        if (actor == _hoveredActor)
        {
            wireframeColor = Color.Green;
        }
        if (actor == SelectedActor)
        {
            wireframeColor = Color.LimeGreen;
        }
        spriteBatch.DrawRectangle(
            destinationRectangle,
            wireframeColor,
            thickness: 1f,
            layerDepth: Constants.LayerDepthActor + Constants.LayerDepthStep
        );
    }

    private void DrawLight(SceneLight light, SpriteBatch spriteBatch)
    {
        var lightPos = new Vector2(light.LightPosition.X, light.LightPosition.Y);
        lightPos -= _camPos;
        lightPos /= _camZoom;
        lightPos += Viewport.TopLeft;

        var radius = 50f;

        spriteBatch.DrawCircle(
            center: lightPos,
            radius: radius,
            color: light.LightColor,
            sides: 32,
            thickness: 10,
            layerDepth: Constants.LayerDepthLight
        );

        var wireframeColor = Color.LightGray;
        if (light == _hoveredLight)
        {
            wireframeColor = Color.Green;
        }
        if (light == SelectedLight)
        {
            wireframeColor = Color.LimeGreen;
        }

        var wfTopLeft = lightPos - new Vector2(radius, radius);
        spriteBatch.DrawRectangle(
            new Rectangle((int)wfTopLeft.X, (int)wfTopLeft.Y, (int)radius * 2, (int)radius * 2),
            wireframeColor,
            thickness: 1f,
            layerDepth: Constants.LayerDepthLight + Constants.LayerDepthStep
        );
    }

    public HashSet<InputContext> GetInputContexts()
    {
        return [_inputContextManager.GetOrCreateByName(nameof(SceneEditor))];
    }

    public InputEventConsumeResult OnInputEvent(InputEventArgs args)
    {
        switch (args.EventType)
        {
            case InputEventType.LMBClicked:
                OnLeftClick(args.GetCursorPosition());
                break;

            case InputEventType.RMBClicked:
                OnRightClick(args.GetCursorPosition());
                break;

            case InputEventType.MouseMoved:
                OnMouseMoved(args.GetCursorPosition());
                break;

            case InputEventType.KeyHit:
                {
                    if (_keyboardMap.IsMatch(EditorEventType.ZoomIn, args.HitOrPressedKey))
                    {
                        _camZoom -= CamZoomSpeed;
                    }
                    if (_keyboardMap.IsMatch(EditorEventType.ZoomOut, args.HitOrPressedKey))
                    {
                        _camZoom += CamZoomSpeed;
                    }
                    if (_keyboardMap.IsMatch(EditorEventType.Delete, args.HitOrPressedKey))
                    {
                        OnDelete();
                    }
                    if (_keyboardMap.IsMatch(EditorEventType.Scale, args.HitOrPressedKey))
                    {
                        OnActorScale();
                    }
                    if (_keyboardMap.IsMatch(EditorEventType.Cancel, args.HitOrPressedKey))
                    {
                        SoftResetState();
                    }
                    break;
                }
            case InputEventType.KeyPressed:
                {
                    if (_keyboardMap.IsMatch(EditorEventType.MoveLeft, args.HitOrPressedKey))
                    {
                        _camPos = new Vector2(_camPos.X - CamSpeed, _camPos.Y);
                    }
                    if (_keyboardMap.IsMatch(EditorEventType.MoveRight, args.HitOrPressedKey))
                    {
                        _camPos = new Vector2(_camPos.X + CamSpeed, _camPos.Y);
                    }
                    if (_keyboardMap.IsMatch(EditorEventType.MoveUp, args.HitOrPressedKey))
                    {
                        _camPos = new Vector2(_camPos.X, _camPos.Y - CamSpeed);
                    }
                    if (_keyboardMap.IsMatch(EditorEventType.MoveDown, args.HitOrPressedKey))
                    {
                        _camPos = new Vector2(_camPos.X, _camPos.Y + CamSpeed);
                    }
                    break;
                }

            default:
                break;
        }
        return new();
    }
}
