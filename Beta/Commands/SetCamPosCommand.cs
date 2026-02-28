using Beta.DI;
using Beta.Logging;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Beta.Commands;

public class SetCamPosCommand : Command
{
    public int X { get; }
    public int Y { get; }

    private readonly OrthographicCamera _camera;
    private readonly ILogger _logger;

    public SetCamPosCommand(int x, int y)
    {
        X = x;
        Y = y;

        _camera = DependencyContainer.Instance.Get<OrthographicCamera>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
    }

    public override void Startup()
    {
        _camera.Position = new Vector2(X, Y);
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
