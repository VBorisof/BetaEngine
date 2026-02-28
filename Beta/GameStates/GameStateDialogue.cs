using System.Collections.Generic;
using System.Linq;
using Beta.Actors;
using Beta.Commands;
using Beta.DI;
using Beta.Dialogues;
using Beta.Input;
using Beta.Scenes;
using Beta.Text;
using Beta.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Beta.CommandManagement;
using BDSM.ExecutionContexts;
using BDSM.Runtime;
using Beta.Phrases;
using BDSM.Language;
using Beta.Services.Sounds;
using Beta.InputMapping;

namespace Beta.GameStates;

public class GameStateDialogue : GameState
{
    private readonly Dialogue _dialogue;
    private DialogueNode _currentNode;
    private List<DialogueOption> _currentOptions = [];
    private int _currentOptionId;
    private string _currentNodePostScript = string.Empty;

    private readonly ITextManager _textManager;

    private Rectangle _optionsWindow;

    private int _optionsWidth;
    private readonly int _optionsHeight = 350;
    private readonly int _singleOptionHeight = 55;

    private readonly OrthographicCamera _camera;
    private readonly Actor _player;
    private readonly CommandManager _commandManager;
    private DialogueState _dialogueState = DialogueState.OtherSpeaking;
    private readonly SceneManager _sceneManager;
    private readonly Driver _driver;
    private readonly EntityManager _entityManager;
    private readonly SoundService _soundService;

    public override string Name => nameof(GameStateDialogue);

    public GameStateDialogue(GameStateManager manager, Actor player, Dialogue dialogue, int nodeIndex) : base(manager)
    {
        _soundService = DependencyContainer.Instance.Get<SoundService>();

        _dialogue = dialogue;
        _textManager = DependencyContainer.Instance.Get<ITextManager>();
        _optionsWindow = new Rectangle(0, 0, _optionsWidth, _optionsHeight);
        _camera = DependencyContainer.Instance.Get<OrthographicCamera>();
        _player = player;

        _optionsWidth = (int)_camera.BoundingRectangle.Width;

        _commandManager = DependencyContainer.Instance.Get<CommandManager>();
        _commandManager.Interrupt(_dialogue.Actor);

        _sceneManager = DependencyContainer.Instance.Get<SceneManager>();

        _driver = DependencyContainer.Instance.Get<Driver>();

        _currentNode = _dialogue.GetNodeById(nodeIndex);

        _entityManager = DependencyContainer.Instance.Get<EntityManager>();

        SetOptions();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _sceneManager.Draw(spriteBatch);
        _commandManager.Draw(spriteBatch);

        if (_dialogueState == DialogueState.WaitForInput && _currentNode != null)
        {
            var pos = new Vector2(_optionsWindow.X, _optionsWindow.Y);

            /*
            spriteBatch.DrawRectBorder(_optionsWidth, _optionsHeight, pos, Settings.LAYER_DEPTH_GUI);

            // Stylistic question of whether we want to draw the background...
            spriteBatch.FillRectangle(
                pos,
                new Size2(_optionsWidth, _optionsHeight),
                _optionsFillColor,
                layerDepth: Constants.LAYER_DEPTH_GUI
            );
            */

            foreach (var option in _currentOptions)
            {
                var outlineColor = _currentOptions.IndexOf(option) == _currentOptionId
                    ? Constants.MainTextHighlightColor
                    : Constants.MainTextOutlineColor;
                _textManager.WriteLine(
                    spriteBatch,
                    $"{option.Index}. {option.Option.Text}",
                    new TextWriteArgs
                    {
                        FontBinding = TextManagerModule.Main,
                        Position = pos + option.Position,
                        Color = Constants.MainTextColor,
                        OutlineColor = outlineColor
                    }
                );
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        _commandManager.Update(gameTime);
        _entityManager.Update(gameTime);
        if (_dialogueState == DialogueState.OtherSpeaking)
        {
            if (_currentNode.IsSkip)
            {
                _dialogueState = DialogueState.WaitForInput;
            }
            else
            {
                var commands = new List<ActorSayCommand>();
                foreach (var phrase in _currentNode.Phrases)
                {
                    var actor = _entityManager.Get<Actor>(phrase.Who);

                    var command = new ActorSayCommand(
                        actor,
                        new Phrase(phrase.What, Constants.TextWidthActorSpeech)
                    );
                    commands.Add(command);
                }

                var lastCommand = commands.LastOrDefault();
                if (lastCommand is not null)
                {
                    lastCommand.Then((_, __) =>
                    {
                        if (!string.IsNullOrWhiteSpace(_currentNodePostScript))
                        {
                            var statements = _driver.ScanAndParse(_currentNodePostScript);
                            _driver.Interpreter.Interpret(statements);
                        }

                        if (_dialogue.IsNodeTerminal(_currentNode))
                        {
                            Manager.RequestStatePlaying();
                        }
                        else
                        {
                            _commandManager.DispatchCommands(
                                ExecutionContext.Shared,
                                new CommandLambda(() => _dialogueState = DialogueState.WaitForInput)
                            );
                        }
                    });
                }
                _dialogueState = DialogueState.WaitText;
                _commandManager.DispatchCommands(ExecutionContext.Shared, commands.ToArray());
            }
        }

        if (_dialogueState == DialogueState.WaitForInput)
        {
            _optionsWidth = (int)_camera.BoundingRectangle.Width; // /2;
            _optionsWindow.Width = _optionsWidth;

            var pos = new Vector2(_camera.Center.X - _optionsWidth / 2, _camera.BoundingRectangle.Bottom - _optionsHeight);
            _optionsWindow.X = (int)pos.X;
            _optionsWindow.Y = (int)pos.Y;
        }
    }


    private void OnCursorMainAction()
    {
        if (_commandManager.IsBusy())
        {
            _commandManager.SkipFirst();
            return;
        }

        if (_dialogueState == DialogueState.WaitForInput)
        {
            OnDialogueOptionSubmit();
        }
    }


    private void OnCursorMainActionAtPosition(Vector2 pos)
    {
        if (_commandManager.IsBusy())
        {
            _commandManager.SkipFirst();
            return;
        }

        if (_dialogueState != DialogueState.WaitForInput)
        {
            return;
        }

        var opt = GetOptionAtPositionOrDefault(pos);
        if (opt != null)
        {
            SubmitDialogueOption(opt);
        }
    }

    private void OnDialogueOptionPrev()
    {
        if (_dialogueState == DialogueState.WaitForInput)
        {
            _soundService.PlaySound(GameSoundType.DialogueOptionHover);
            if (_currentOptionId <= 0)
            {
                _currentOptionId = _currentOptions.Count - 1;
            }
            else
            {
                --_currentOptionId;
            }
        }
    }

    private void OnDialogueOptionNext()
    {
        if (_dialogueState == DialogueState.WaitForInput)
        {
            _soundService.PlaySound(GameSoundType.DialogueOptionHover);
            if (_currentOptionId >= _currentOptions.Count - 1)
            {
                _currentOptionId = 0;
            }
            else
            {
                ++_currentOptionId;
            }
        }
    }

    private void SetOptions()
    {
        float optionSpacing = _singleOptionHeight + _singleOptionHeight/2;
        var padding = new Vector2(20, 20);

        // Can I ever undo this beast that I've become?
        _currentOptions = _dialogue.GetOptions(_currentNode).Where(nextOption =>
        {
            // We need to check the condition on the option, so we know whether
            // to add it or not.
            if (!string.IsNullOrWhiteSpace(nextOption.Condition))
            {
                // Condition must be a BDSM boolean expression.
                var condition = _driver.ScanAndParse(nextOption.Condition);
                if (condition is not null)
                {
                    var condExpression = (ExpressionStatement)condition.First()!;
                    // Interpret the expression and return the result to filter.
                    return
                        (bool)_driver.Interpreter
                            .Evaluate(condExpression!.expr!, ExecutionContext.Shared);
                }
            }
            // No condition, so assume we can include this option
            return true;
        })
        .Select(
            (o, i) => new DialogueOption
            {
                Index = i + 1,
                Option = o,
                Position = padding + new Vector2(0, i * optionSpacing)
            }
        )
        .ToList();
        _currentOptionId = -1;
    }

    private void OnDialogueOptionSubmit()
    {
        if (_currentOptionId < 0 || _currentOptionId >= _currentOptions.Count)
        {
            return;
        }

        if (_dialogueState == DialogueState.WaitForInput)
        {
            // _soundService.PlaySound(GameSoundType.DialogueOptionSubmit);
            var currentOption = _currentOptions[_currentOptionId];

            SubmitDialogueOption(currentOption);
        }
    }

    private void SubmitDialogueOption(DialogueOption option)
    {
        var command = new ActorSayCommand(
            _player,
            new Phrase(option.Option.Text, Constants.TextWidthActorSpeech)
        );
        command.Then((_, __) =>
        {
            _currentNode = _dialogue.GetNodeById(option.Option.To);
            _currentNodePostScript = option.Option.Script;

            SetOptions();

            _dialogueState = DialogueState.OtherSpeaking;
        }
        );

        _dialogueState = DialogueState.PlayerSpeaking;
        _commandManager.DispatchCommands(ExecutionContext.Shared, command);
    }

    private DialogueOption? GetOptionAtPositionOrDefault(Vector2 pos)
    {
        return _currentOptions.SingleOrDefault(
            o =>
            {
                var optionPosY = _optionsWindow.Y + o.Position.Y;

                const float tolerance = 5f;

                return optionPosY - (_singleOptionHeight / 2) - tolerance < pos.Y
                    && optionPosY + (_singleOptionHeight / 2) + tolerance > pos.Y;
            }
        );
    }

    private void OnCursorPositionChanged(Vector2 pos)
    {
        if (_dialogueState != DialogueState.WaitForInput)
        {
            return;
        }

        var opt = GetOptionAtPositionOrDefault(pos);
        if (opt is not null)
        {
            var prevOptionId = _currentOptionId;
            _currentOptionId = _currentOptions.IndexOf(opt);
            if (prevOptionId != _currentOptionId)
            {
                _soundService.PlaySound(GameSoundType.DialogueOptionHover);
            }
        }
    }

    private void OnNumberSelected(int number)
    {
        var opt = _currentOptions.SingleOrDefault(o => o.Index == number);
        if (opt is null)
        {
            return;
        }

        if (_commandManager.IsBusy())
        {
            _commandManager.SkipFirst();
            return;
        }

        _currentOptionId = _currentOptions.IndexOf(opt);
        if (_dialogueState == DialogueState.WaitForInput)
        {
            OnDialogueOptionSubmit();
        }
    }

    public override InputEventConsumeResult OnInputEvent(InputEventArgs args)
    {
        if (InputMapper.IsMatch(args, GameInputType.DialogueOptionNext))
        {
            OnDialogueOptionNext();
        }
        if (InputMapper.IsMatch(args, GameInputType.DialogueOptionPrev))
        {
            OnDialogueOptionPrev();
        }
        if (InputMapper.IsMatch(args, GameInputType.DialogueOptionSubmit))
        {
            OnDialogueOptionSubmit();
        }
        if (InputMapper.IsMatch(args, GameInputType.CursorPositionChanged))
        {
            OnCursorPositionChanged(args.GetCursorPosition());
        }
        if (InputMapper.IsMatch(args, GameInputType.CursorDragged))
        {
            OnCursorPositionChanged(args.GetCursorPosition());
        }
        if (InputMapper.IsMatch(args, GameInputType.CursorMainAction))
        {
            OnCursorMainAction();
        }
        if (InputMapper.IsMatch(args, GameInputType.CursorMainActionAtPosition))
        {
            OnCursorMainActionAtPosition(args.GetCursorPosition());
        }
        if (InputMapper.IsMatch(args, GameInputType.NumberSelected))
        {
            var number = args.GetSelectedNumber();
            OnNumberSelected(number);
        }

        return new();
    }
}
