using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beta.Commands;

public enum CommandSkipStyle
{
    Disabled,
    SkipOne,
    SkipAll,
}

public abstract class Command
{

    public CommandSkipStyle SkipStyle { get; set; } = CommandSkipStyle.Disabled;
    public bool IsDone { get; set; }

    public abstract void Startup();
    public abstract bool Update(GameTime gameTime);
    public virtual void Draw(SpriteBatch spriteBatch) { }
    public virtual void OnComplete()
    {
        Completed.Invoke(this, EventArgs.Empty);
    }
    public virtual void OnInterrupt()
    {
        Interrupted.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler Completed = (_, _) => { };
    public event EventHandler Interrupted = (_, _) => { };
}
