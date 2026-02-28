using Beta.DI;
using Beta.Common;
using Beta.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Beta.Services.Sounds;

namespace Beta.Commands;

public class PlaySoundCommand : Command
{
    private readonly ILogger _logger;

    public string Name { get; }

    private readonly SoundService _soundService;
    private readonly SoundEffect _soundEffect;

    public PlaySoundCommand(string name)
    {
        _soundService = DependencyContainer.Instance.Get<SoundService>();
        _logger = DependencyContainer.Instance.Get<ILogger>();

        Name = name;

        var contentCache = DependencyContainer.Instance.Get<ContentCache>();
        _soundEffect = contentCache.Get<SoundEffect>($"sounds/{name}");
    }

    public override void Startup()
    {
        _soundService.PlaySound(_soundEffect);
        IsDone = true;
    }

    public override bool Update(GameTime gameTime)
    {
        return IsDone;
    }

    public override void OnInterrupt()
    {
        base.OnInterrupt();
        _logger.Debug("");
        _soundService.Stop();
    }

    public override void OnComplete()
    {
        base.OnComplete();
        _logger.Debug("");
    }
}