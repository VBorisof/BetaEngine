using Beta.Gui.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Beta.Gui.Behaviors;

public abstract class GuiElementBehavior
{
    protected GuiElement GuiElement { get; }
    public bool IsDone { get; protected set; }

    public event EventHandler Done;

    public GuiElementBehavior(GuiElement guiElement)
    {
        GuiElement = guiElement;
        Done = (_, _) => { };
    }

    public abstract void Update(GameTime gameTime);
    public virtual void Draw(SpriteBatch spriteBatch) { }

    public void OnDone()
    {
        Done.Invoke(this, new EventArgs());
    }
}