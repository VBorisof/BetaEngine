using Beta.Common;
using Beta.DI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Beta.AdditionalUi;

//
// TODO: Think of a better name.
public class AdditionalUiManager
{
    public FadingAdditionalUiElement AutosaveSpinner { get; }
    public FadingAdditionalUiElement LogoBanner { get; }

    public AdditionalUiManager()
    {
        var spinnerTexture = DependencyContainer.Instance.Get<ContentCache>()
            .Get<Texture2D>("img/ui/autosave-badge");
        var logoTexture = DependencyContainer.Instance.Get<ContentCache>()
            .Get<Texture2D>("img/ui/logo");

        AutosaveSpinner = new FadingAdditionalUiElement(
            texture: spinnerTexture,
            destinationRectangle: new Rectangle(1820, 160, 80, 80),
            isRotating: true,
            showForAtLeast: TimeSpan.FromSeconds(1));

        LogoBanner = new FadingAdditionalUiElement(
            texture: logoTexture,
            destinationRectangle: new Rectangle(660, 240, 500, 274),
            isRotating: false,
            showForAtLeast: TimeSpan.Zero);
    }

    public void StopAllImmediately()
    {
        AutosaveSpinner.StopImmediately();
        LogoBanner.StopImmediately();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        AutosaveSpinner.Draw(spriteBatch);
        LogoBanner.Draw(spriteBatch);
    }
    public void Update(GameTime gameTime)
    {
        AutosaveSpinner.Update(gameTime);
        LogoBanner.Update(gameTime);
    }
}