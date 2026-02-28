using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Beta.Common.Extensions;
using Beta.Gui.Styles;
using Beta.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Beta.Gui.Elements;

public class TextBox : GuiElement
{
    private Box _box { get; set; }

    private string _text;
    public string Text
    {
        get => _text;
        set => SetText(value);
    }

    private const int ScrollButtonSize = 20;
    private readonly TextButton _scrollUpButton;
    private readonly TextButton _scrollDownButton;

    private Vector2 _padding = new(10, 10);
    private readonly int _maxLines;
    private readonly int _lineOffset;
    private int _startLine;
    private List<string> _allLines = [];
    private bool _hasScrollbar;

    private readonly TimeSpan _fastScrollTriggerTime = TimeSpan.FromMilliseconds(250);
    private readonly Stopwatch _sw = new();

    public TextBox(string text, GuiElementStyle style, string? extraInputContexts) : base(style, extraInputContexts)
    {
        if (Gui.Instance.TextManager is null || Gui.Instance.MainFont is null)
        {
            var message = "Text Manager is not properly loaded.";
            Gui.Instance.Logger?.Error(message);
            throw new InvalidOperationException(message);
        }

        _box = new Box(style with { RelativePosition = Vector2.Zero }, extraInputContexts);
        AddElement(_box);

        _box.SubscribeOnDrag((_, args) =>
        {
            // Do not scroll if not enough lines.
            if (_allLines.Count < _maxLines)
            {
                return;
            }

            // TODO: The movement here is still a bit jerky, not sure why.
            const float sensitivityThreshold = 5f;
            if (args.DragVector.Y >= sensitivityThreshold)
            {
                ScrollUp(rollover: false);
            }
            if (args.DragVector.Y <= sensitivityThreshold)
            {
                ScrollDown(rollover: false);
            }
        });

        _box.SubscribeOnScroll((_, args) =>
        {
            // Do not scroll if not enough lines.
            if (_allLines.Count < _maxLines)
            {
                return;
            }

            if (args.ScrollWheelDiff < 0)
            {
                ScrollUp(rollover: false);
            }
            else
            {
                ScrollDown(rollover: false);
            }
        });

        _lineOffset = Gui.Instance.MainFont.Font!.LineSpacing;
        _maxLines = (int)Math.Floor((_box.Style.Size.Height - _padding.Y) / _lineOffset);

        _scrollUpButton = new TextButton("^", style with
        {
            Size = new SizeF(ScrollButtonSize, ScrollButtonSize),
            RelativePosition = new Vector2(Style.Size.Width - ScrollButtonSize, 0),
            LayerDepth = Style.LayerDepth + (Constants.LayerDepthStep * 3)
        }, extraInputContexts);
        _scrollUpButton.SubscribeOnLeftClick((_, __) =>
        {
            if (_sw.Elapsed <= _fastScrollTriggerTime)
            {
                ScrollUp(rollover: true);
            }
            _sw.Reset();
        });
        _scrollUpButton.SubscribeOnLeftPress((_, __) =>
        {
            if (_sw.Elapsed.Equals(TimeSpan.Zero))
            {
                _sw.Restart();
            }
            if (_sw.Elapsed > _fastScrollTriggerTime)
            {
                ScrollUp(rollover: false);
            }
        });

        _scrollDownButton = new TextButton("v", style with
        {
            Size = new SizeF(ScrollButtonSize, ScrollButtonSize),
            RelativePosition = new Vector2(Style.Size.Width - ScrollButtonSize, Style.Size.Height - ScrollButtonSize),
            LayerDepth = Style.LayerDepth + (Constants.LayerDepthStep * 3)
        }, extraInputContexts);
        _scrollDownButton.SubscribeOnLeftClick((_, __) =>
        {
            if (_sw.Elapsed <= _fastScrollTriggerTime)
            {
                ScrollDown(rollover: true);
            }
            _sw.Reset();
        });
        _scrollDownButton.SubscribeOnLeftPress((_, __) =>
        {
            if (_sw.Elapsed.Equals(TimeSpan.Zero))
            {
                _sw.Restart();
            }
            if (_sw.Elapsed > _fastScrollTriggerTime)
            {
                ScrollDown(rollover: false);
            }
        });

        _text = text;
        SetText(text);
    }

    private void ScrollUp(bool rollover)
    {
        if (_startLine == 0)
        {
            if (rollover)
            {
                ScrollToBottom();
            }
        }
        else
        {
            --_startLine;
        }
    }
    private void ScrollDown(bool rollover)
    {
        if (_startLine + 1 >= _allLines.Count - _maxLines)
        {
            if (rollover)
            {
                _startLine = 0;
            }
        }
        else
        {
            ++_startLine;
        }
    }

    public void ScrollToBottom()
    {
        _startLine = _allLines.Count - _maxLines - 1;
    }

    private void SetText(string text)
    {
        _text = text.TrimEnd().SetLineWidth(
                (int)Math.Floor(
                    (Style.Size.Width - _padding.X)
                    / Gui.Instance.MainFont!.MeasureString("w").Width));

        _allLines = [.. _text.Split('\n')];

        if (_allLines.Count > _maxLines)
        {
            _box.AddElement(_scrollUpButton);
            _box.AddElement(_scrollDownButton);
            _hasScrollbar = true;
        }
        else if (_hasScrollbar)
        {
            _box.RemoveElement(_scrollUpButton);
            _box.RemoveElement(_scrollDownButton);
            _hasScrollbar = false;
        }
    }


    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        if (Style.Hidden)
        {
            return;
        }

        if (Gui.Instance.TextManager is null || Gui.Instance.MainFont is null)
        {
            return;
        }

        var linesToDraw = _allLines.Skip(_startLine).Take(_maxLines).ToList();
        for (var i = 0; i < linesToDraw.Count; ++i)
        {
            Gui.Instance.TextManager.WriteLine(
                spriteBatch,
                linesToDraw[i],
                new TextWriteArgs
                {
                    FontBinding = Gui.Instance.MainFont,
                    Position = _box.GetAbsolutePosition() + _padding + new Vector2(0, i * _lineOffset),
                    Color = Style.TextColor,
                    // TODO: OutlineColor = 
                    LayerDepth = Style.LayerDepth + Constants.LayerDepthStep,
                }
            );
        }
    }

    public override string ToString()
    {
        return $"{nameof(TextBox)} .{ElemClass} #{ElemId}";
    }
}