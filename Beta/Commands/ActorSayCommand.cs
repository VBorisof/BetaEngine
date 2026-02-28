using System;
using System.Text.RegularExpressions;
using Beta.Actors;
using Beta.DI;
using Beta.Logging;
using Beta.Phrases;
using Beta.Scenes;
using Beta.Services;
using Beta.Services.Sounds;
using Beta.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Beta.Commands;

public partial class ActorSayCommand : ActorCommand
{
    private readonly ILogger _logger;
    private readonly Phrase _phrase;
    private readonly string[] _lines;
    private readonly PhraseSoundPlayer? _phrasePlayer;
    private readonly ITextManager _textManager;
    private readonly HistoryService _historyService;
    private readonly SceneManager _sceneManager;
    private readonly OrthographicCamera _camera;
    private EventHandler _onComplete = (_, __) => { };

    private float _timePassed;

    public ActorSayCommand(Actor actor, Phrase phrase) : base(actor)
    {
        SkipStyle = CommandSkipStyle.SkipOne;
        _phrase = phrase;
        _lines = _phrase.Text.Split('\n');
        _textManager = DependencyContainer.Instance.Get<ITextManager>();
        _onComplete += (_, _) => Actor.SuggestState(ActorState.Idle);
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _camera = DependencyContainer.Instance.Get<OrthographicCamera>();
        _historyService = DependencyContainer.Instance.Get<HistoryService>();
        _sceneManager = DependencyContainer.Instance.Get<SceneManager>();
        if (Actor.SpeechSound is not null)
        {
            _phrasePlayer = new(Actor.SpeechSound, _phrase.Text);
        }
    }

    public override void Startup()
    {
        if (Actor.Scene != _sceneManager.CurrentScene)
        {
            IsDone = true;
            return;
        }

        if (string.IsNullOrEmpty(_phrase.Text))
        {
            IsDone = true;
            return;
        }
        Actor.SuggestState(ActorState.Talk);
    }

    public void Then(EventHandler then)
    {
        _onComplete += then;
    }

    public override bool Update(GameTime gameTime)
    {
        _logger.Trace("Update.");

        _timePassed += gameTime.GetElapsedSeconds();

        _phrasePlayer?.Update(gameTime);

        if (_timePassed >= _phrase.SecondsDuration)
        {
            IsDone = true;
        }

        return IsDone;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (IsDone)
        {
            return;
        }

        var size = _textManager.GetLinesSize(_lines, TextManagerModule.Main);

        var position = new Vector2(
            Actor.GetBoundingRect().X + (Actor.GetBoundingRect().Width / 2) + 20,
            Actor.GetBoundingRect().Y - 70
        );

        // Clamp text position.
        var margin = 20f;

        if (position.X - (size.Width / 2) < _camera.BoundingRectangle.Left)
        {
            position.X = _camera.BoundingRectangle.Left + (size.Width / 2) + margin;
        }
        if (position.X + (size.Width / 2) > _camera.BoundingRectangle.Right)
        {
            position.X = _camera.BoundingRectangle.Right - (size.Width / 2) - margin;
        }

        var top = _camera.BoundingRectangle.Top + 50;
        if (position.Y < top)
        {
            position.Y = top + margin;
        }
        var bottom = _camera.BoundingRectangle.Bottom - 400;
        if (position.Y + size.Height > bottom)
        {
            position.Y = bottom - size.Height - margin;
        }

        _textManager.WriteLines(
            spriteBatch,
            _lines,
            new TextWriteArgs
            {
                FontBinding = TextManagerModule.Main,
                Position = position,
                Color = Actor.SpeechColor,
                TextAlignment = TextAlignment.Center,
                LayerDepth = Constants.LayerDepthSpeech,
            }
        );
    }

    private void AppendToHistory()
    {
        var text = NewLineRegex().Replace(_phrase.Text, " ");
        _historyService.Append($"{Actor.Name}: {text}");
    }

    public override void OnComplete()
    {
        base.OnComplete();
        AppendToHistory();
        _logger.Debug("");
        // _sfxInstance?.Stop();
        _onComplete(this, EventArgs.Empty);
    }

    public override void OnInterrupt()
    {
        base.OnInterrupt();
        AppendToHistory();
        _logger.Debug("");
        // _sfxInstance?.Stop();
        _onComplete(this, EventArgs.Empty);
    }

    [GeneratedRegex(@"\t|\n|\r")]
    private static partial Regex NewLineRegex();
}