using Beta.DI;
using Beta.Logging;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Beta.Commands;

public class CamZoomCommand : Command
{
    public int Zoom { get; }

    private readonly OrthographicCamera _camera;
    private readonly ILogger _logger;

    public CamZoomCommand(int zoom)
    {
        Zoom = zoom;
        _camera = DependencyContainer.Instance.Get<OrthographicCamera>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
    }

    public override void Startup()
    {
        _camera.Zoom = Zoom;
        IsDone = true;
    }

    public override bool Update(GameTime gameTime)
    {
        return IsDone;
    }

    public override void OnComplete()
    {
        base.OnComplete();
        _logger.Debug("");
    }

    public override void OnInterrupt()
    {
        base.OnInterrupt();
        _logger.Debug("");
    }
}