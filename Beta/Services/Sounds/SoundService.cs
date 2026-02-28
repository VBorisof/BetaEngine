using Beta.Common;
using Beta.DI;
using Beta.Logging;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace Beta.Services.Sounds;

public class SoundService
{
    private readonly Dictionary<GameSoundType, SoundEffect> _sfxDict = [];
    private readonly ILogger _logger;
    private SoundEffectInstance? _sfx;

    public SoundService()
    {
        var contentCache = DependencyContainer.Instance.Get<ContentCache>();
        _logger = DependencyContainer.Instance.Get<ILogger>();

        try
        {
            _sfxDict[GameSoundType.Click] = contentCache.Get<SoundEffect>("sounds/ui_click");
            _sfxDict[GameSoundType.Hover] = contentCache.Get<SoundEffect>("sounds/ui_hover");
            _sfxDict[GameSoundType.DialogueOptionHover] = contentCache.Get<SoundEffect>("sounds/ui_hover");
            _sfxDict[GameSoundType.InventoryPick] = contentCache.Get<SoundEffect>("sounds/game_confirm");
            _sfxDict[GameSoundType.ItemUse] = contentCache.Get<SoundEffect>("sounds/game_confirm");
            _sfxDict[GameSoundType.HoverUiElemSound] = contentCache.Get<SoundEffect>("sounds/ui_hover");
            _sfxDict[GameSoundType.ClickUiElemSound] = contentCache.Get<SoundEffect>("sounds/ui_click");
            _sfxDict[GameSoundType.NotifySound] = contentCache.Get<SoundEffect>("sounds/game_notif");
        }
        // This will definitely happen with the current recovery cycle.
        // I have a strong suspision this is related to this sound buffer allocation adjustment:
        // https://github.com/MonoGame/MonoGame/commit/c6e47a1637e99982a6360020c7432968ac971202
        // and the way we clean up resources for the next game launch, in particular calling the GC.Collect().
        // To fix, probably requires re-thinking the way we clean up or re-writing the importer.
        // For now though, just silently ignore and leave the game with no sound because fuck it.
        catch (Exception)
        {
            _logger.Warning("Failed to load sound.");
        }
    }

    public SoundEffect? GetSoundEffect(GameSoundType soundType)
    {
        _sfxDict.TryGetValue(soundType, out var effect);
        return effect;
    }

    public void PlaySound(GameSoundType soundType)
    {
        if (_sfxDict.TryGetValue(soundType, out var effect))
        {
            PlaySound(effect);
        }
    }

    public void PlaySound(SoundEffect soundEffect)
    {
        _sfx?.Stop(immediate: true);
        _sfx = soundEffect.CreateInstance();
        _sfx.Volume = Settings.SoundVolume;

        _sfx.Play();
    }

    public void Stop()
    {
        _sfx?.Stop();
    }
}