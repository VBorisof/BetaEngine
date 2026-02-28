using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Beta.Commands;

namespace Beta.CommandManagement;

public class CommandQueue
{
    public string Name { get; set; } = "CommandQueue";
    private Command? _currentCommand;
    private readonly Queue<Command> _queue = [];

    public bool IsActive { get; set; } = true;
    public bool IsBusy => _currentCommand != null || _queue.Count != 0;

    public bool AsyncInterruptFlag { get; set; }

    public void Enqueue(Command command)
    {
        _queue.Enqueue(command);
    }

    public T? LastOrDefault<T>() where T : Command
    {
        var command = _queue.LastOrDefault(c => c is T);
        return command is null ? default : (T)command;
    }
    public T? FirstOrDefault<T>() where T : Command
    {
        var command = _queue.FirstOrDefault(c => c is T);
        return command is null ? default : (T)command;
    }

    public void Update(GameTime gameTime)
    {
        if (!IsActive)
        {
            return;
        }

        var hasCommand = _currentCommand is not null;
        if (!hasCommand)
        {
            hasCommand = _queue.TryDequeue(out _currentCommand);
            if (hasCommand)
            {
                _currentCommand?.Startup();
            }
        }

        // Previous command might've killed everything.
        hasCommand = _currentCommand is not null;
        if (hasCommand)
        {
            if (_currentCommand!.IsDone)
            {
                _currentCommand.OnComplete();
                _currentCommand = null;
            }
            else
            {
                _currentCommand.Update(gameTime);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _currentCommand?.Draw(spriteBatch);
    }

    public void Skip()
    {
        if (_currentCommand != null)
        {
            switch (_currentCommand.SkipStyle)
            {
                case CommandSkipStyle.Disabled:
                    break;
                case CommandSkipStyle.SkipOne:
                    _currentCommand.OnInterrupt();
                    _currentCommand = null;
                    break;
                case CommandSkipStyle.SkipAll:
                    _currentCommand.OnInterrupt();
                    _currentCommand = null;
                    while (_queue.Count != 0)
                    {
                        var command = _queue.Dequeue();
                        command.OnInterrupt();
                    }
                    break;
            }
        }
    }

    public void Interrupt()
    {
        if (!IsActive)
        {
            return;
        }

        _queue.Clear();
        _currentCommand?.OnInterrupt();
        _currentCommand = null;
    }

    public object GetDebugInfo()
    {
        var currentCommandName = _currentCommand is null ? "None" : _currentCommand.GetType().Name;
        return new
        {
            name = Name,
            current = currentCommandName,
            queue = _queue.Select(c => c.GetType().Name).ToList()
        };
    }
}