using System;
using System.Collections.Generic;
using Beta.DI;
using Beta.Services.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Beta.Verbs;

public class VerbPickerMenu
{
    public bool IsOpen { get; private set; }

    private const int CircleRadius = 128;
    private const int CancelDistFar = CircleRadius + 150;
    private const int CancelDistClose = 32;

    private readonly VerbPickerOption _look;
    private readonly VerbPickerOption _pickup;
    private readonly VerbPickerOption _talk;
    private readonly VerbPickerOption _interact;

    private Vector2 _position;

    private readonly List<VerbPickerOption> _options = [];

    public EventHandler OnSelectLook { get; set; } = (_, _) => { };
    public EventHandler OnSelectPickup { get; set; } = (_, _) => { };
    public EventHandler OnSelectTalk { get; set; } = (_, _) => { };
    public EventHandler OnSelectInteract { get; set; } = (_, _) => { };
    public EventHandler OnCancel { get; set; } = (_, _) => { };

    private readonly SoundService _soundService;

    public VerbPickerMenu()
    {
        _soundService = DependencyContainer.Instance.Get<SoundService>();
        _look = new("img/cursor/look", "img/cursor/look_hovered");
        _look.OnClick += (_, __) => OnSelectLook(this, EventArgs.Empty);
        _options.Add(_look);

        _pickup = new("img/cursor/pickup", "img/cursor/pickup_hovered");
        _pickup.OnClick += (_, __) => OnSelectPickup(this, EventArgs.Empty);
        _options.Add(_pickup);

        _talk = new("img/cursor/talk", "img/cursor/talk_hovered");
        _talk.OnClick += (_, __) => OnSelectTalk(this, EventArgs.Empty);
        _options.Add(_talk);

        _interact = new("img/cursor/interact", "img/cursor/interact_hovered");
        _interact.OnClick += (_, __) => OnSelectInteract(this, EventArgs.Empty);
        _options.Add(_interact);
    }

    public void Close()
    {
        IsOpen = false;
        _options.ForEach(o => o.Reset());
    }

    public void Open()
    {
        _soundService.PlaySound(GameSoundType.Click);
        IsOpen = true;
    }

    public void SetPosition(Vector2 position)
    {
        _position = position;

        _look.SetPosition(_position + new Vector2(0, -CircleRadius));
        _talk.SetPosition(_position + new Vector2(0, CircleRadius));

        _pickup.SetPosition(_position + new Vector2(CircleRadius, 0));
        _interact.SetPosition(_position + new Vector2(-CircleRadius, 0));
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawCircle(
            center: _position,
            radius: CircleRadius,
            sides: 64,
            color: Color.Black * 0.3f,
            thickness: CircleRadius / 2,
            layerDepth: Constants.LayerDepthGui - Constants.LayerDepthStep
        );

        foreach (var option in _options)
        {
            option.Draw(spriteBatch);
        }
    }

    public void OnMoveCursor(Vector2 pos)
    {
        if (IsCursorTooFar(pos))
        {
            OnCancel.Invoke(this, EventArgs.Empty);
        }

        foreach (var option in _options)
        {
            option.OnCursorMoved(pos);
        }
    }

    // See if we're too far, and want to close the menu.
    public bool IsCursorTooFar(Vector2 pos)
    {
        return (_position - pos).Length() >= CancelDistFar;
    }

    public bool IsCursorTooClose(Vector2 pos)
    {
        return (_position - pos).Length() <= CancelDistClose;
    }

    public void OnCursorMainAction(Vector2 pos)
    {
        if (IsCursorTooClose(pos))
        {
            OnCancel.Invoke(this, EventArgs.Empty);
        }

        foreach (var option in _options)
        {
            option.OnLeftClick(pos);
        }
    }
}