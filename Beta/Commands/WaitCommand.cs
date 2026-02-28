using Beta.DI;
using Beta.Logging;
using Microsoft.Xna.Framework;

namespace Beta.Commands;

public class WaitCommand : Command
{
    private readonly ILogger _logger;
    private float _timePassed;

    public int Timeout { get; set; }

    public WaitCommand(int timeout)
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        Timeout = timeout;
    }

    public override void Startup()
    {

    }

    public override bool Update(GameTime gameTime)
    {
        _timePassed += gameTime.ElapsedGameTime.Milliseconds;

        if (_timePassed >= Timeout)
        {
            _logger.Debug($"{_timePassed}ms passed / {Timeout}.");
            _timePassed = 0f;
            IsDone = true;
        }

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