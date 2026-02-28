using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Beta.DI;
using Beta.Logging;
using BDSM.ExecutionContexts;
using Beta.Actors;
using Beta.Commands;

namespace Beta.CommandManagement;

public class CommandManager
{
    private readonly ILogger _logger;

    private readonly Dictionary<string, CommandQueue> _actorQueues = [];
    private readonly CommandQueue _sharedQueue = new();

    // TODO: We have a leak in that when we interrupt
    // something off async queue, we never delete the tag.
    public EventHandler<Guid> RequestAsyncCallback { get; set; } = (_, _) => { };
    private readonly Dictionary<Guid, CommandQueue> _asyncQueues = [];

    public CommandManager()
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();

        _sharedQueue.Name = "SHARED";
    }

    public void Update(GameTime gameTime)
    {
        _sharedQueue.Update(gameTime);
        foreach (var actorQueue in _actorQueues)
        {
            actorQueue.Value.Update(gameTime);
        }
        UpdateAsyncCommands(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _sharedQueue.Draw(spriteBatch);
        foreach (var actorQueue in _actorQueues)
        {
            actorQueue.Value.Draw(spriteBatch);
        }
        DrawAsyncCommands(spriteBatch);
    }

    public void DispatchCommands(ExecutionContext context, params Command[] commands)
    {
        switch (context.ContextType)
        {
            // TODO: Assign skipping rules here?
            case ExecutionContextType.Shared:
                foreach (var command in commands)
                {
                    if (command is ActorSayCommand or MoveCommand or PlayAnimationCommand)
                    {
                        command.SkipStyle = CommandSkipStyle.SkipOne;
                    }
                    _sharedQueue.Enqueue(command);
                }
                break;

            case ExecutionContextType.Actor:
                foreach (var command in commands)
                {
                    if (command is ActorSayCommand or PlayAnimationCommand)
                    {
                        command.SkipStyle = CommandSkipStyle.SkipOne;
                    }
                    if (command is MoveCommand)
                    {
                        command.SkipStyle = CommandSkipStyle.SkipAll;
                    }

                    if (!_actorQueues.TryGetValue(context.ActorName, out var value))
                    {
                        value = new CommandQueue
                        {
                            Name = context.ActorName
                        };
                        _actorQueues[context.ActorName] = value;
                    }

                    value.Enqueue(command);
                }
                break;

            case ExecutionContextType.Async:
                foreach (var command in commands)
                {
                    if (!_asyncQueues.TryGetValue(context.AsyncTag, out var value))
                    {
                        var queue = new CommandQueue()
                        {
                            IsActive = false,
                            Name = context.AsyncTag.ToString()
                        };

                        // Need to get the last command off shared queue,
                        // to know when to activate this queue.
                        var lastCommand = _sharedQueue.LastOrDefault<Command>();
                        if (lastCommand != null)
                        {
                            lastCommand.Completed += (_, _) =>
                            {
                                queue.IsActive = true;
                            };
                            lastCommand.Interrupted += (_, _) =>
                            {
                                queue.IsActive = true;
                            };
                        }

                        value = queue;
                        _asyncQueues.Add(context.AsyncTag, value);
                    }

                    value.Enqueue(command);
                }

                break;
        }
    }

    public T? FirstOrDefault<T>(Actor caller) where T : Command
    {
        if (_actorQueues.TryGetValue(caller.DeclName, out var value))
        {
            if (value.IsBusy)
            {
                return value.FirstOrDefault<T>();
            }
        }

        return null;
    }


    public bool IsBusy()
    {
        return _sharedQueue.IsBusy || _actorQueues.Any(q => q.Value.IsBusy);
    }

    public bool IsSharedQueueBusy()
    {
        return _sharedQueue.IsBusy;
    }

    public bool IsBusy(Actor caller)
    {
        if (_sharedQueue.IsBusy)
        {
            return true;
        }

        if (_actorQueues.TryGetValue(caller.DeclName, out var value))
        {
            return value.IsBusy;
        }

        // Async?

        return false;
    }

    public void SkipFirst()
    {
        if (_sharedQueue.IsBusy)
        {
            _sharedQueue.Skip();
        }
        else
        {
            var firstBusyQueue = _actorQueues.FirstOrDefault(q => q.Value.IsBusy);
            if (!firstBusyQueue.Equals(default(KeyValuePair<Actor, CommandQueue>)))
            {
                firstBusyQueue.Value.Skip();
            }
        }
    }
    public void Skip(Actor caller)
    {
        if (_sharedQueue.IsBusy)
        {
            _sharedQueue.Skip();
        }
        else
        {
            if (_actorQueues.TryGetValue(caller.DeclName, out var value))
            {
                value.Skip();
            }
        }

        // Async?
    }

    public void Interrupt(Actor caller)
    {
        if (_actorQueues.TryGetValue(caller.DeclName, out var value))
        {
            value.Interrupt();
            //caller.ForceState(ActorState.Idle); // TODO: Is this right?
        }

        // Async?
    }

    public void Interrupt(bool interruptAsync)
    {
        _sharedQueue.Interrupt();

        foreach (var actorQueue in _actorQueues)
        {
            actorQueue.Value.Interrupt();
        }

        if (interruptAsync)
        {
            foreach (var asyncQueue in _asyncQueues)
            {
                asyncQueue.Value.Interrupt();
                asyncQueue.Value.AsyncInterruptFlag = true;
            }
        }
    }

    private void UpdateAsyncCommands(GameTime gameTime)
    {
        foreach (var queue in _asyncQueues)
        {
            queue.Value.Update(gameTime);

            // Is the queue empty?
            if (!queue.Value.IsBusy)
            {
                _logger.Debug($"\n\n<{queue.Key}>");
                _logger.Debug("No more commands with this tag. Remove and callback.");

                // Remove the context key and fire the callback.
                _asyncQueues.Remove(queue.Key);
                if (!queue.Value.AsyncInterruptFlag)
                {
                    RequestAsyncCallback(this, queue.Key);
                }
                queue.Value.AsyncInterruptFlag = false;
            }
        }
    }

    private void DrawAsyncCommands(SpriteBatch spriteBatch)
    {
        foreach (var asyncTag in _asyncQueues)
        {
            asyncTag.Value.Draw(spriteBatch);
        }
    }
}