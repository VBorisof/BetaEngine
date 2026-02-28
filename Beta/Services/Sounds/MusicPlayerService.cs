using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Beta.Common;
using Beta.DI;
using Microsoft.Xna.Framework;
using System;
using Beta.Logging;

namespace Beta.Services.Sounds;

public enum MusicType
{
    // NB: Order important, acts as comparer
    None     = 0,
    Scene    = 1,
    Scripted = 2
}

public enum MusicPlayerState
{
    Stopped,
    Playing,
    Stopping,
}

public class MusicPlayerService
{
    private readonly Dictionary<string, SoundEffect> _songs = [];
    private SoundEffect? _currentSong;
    private SoundEffectInstance? _currentSongInstance;

    private bool _isPause;
    private readonly ContentCache _contentCache;
    private readonly ILogger _logger;
    private MusicPlayerState _state = MusicPlayerState.Stopped;
    private MusicType _currentMusicType = MusicType.None;

    public MusicPlayerService()
    {
        _contentCache = DependencyContainer.Instance.Get<ContentCache>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
    }

    public void AddSong(string songName, string path)
    {
        if (_songs.ContainsKey(songName))
        {
            return;
        }
        try
        {
            _songs[songName] = _contentCache.Get<SoundEffect>(path);
        }
        catch (Exception) // See SoundService
        {
            _logger.Warning("Failed to load sound.");
        }
    }

    public void Play(string? name, MusicType musicType)
    {
        if (musicType < _currentMusicType)
        {
            return;
        }

        // TODO: Reconsider support for this.
        // It's basically for scenes that have no music.
        if (string.IsNullOrWhiteSpace(name))
        {
            StopImmediately();
            return;
        }

        if (_songs[name] == _currentSong)
        {
            return;
        }

        _currentSongInstance?.Stop();

        if (!_songs.TryGetValue(name, out var song))
        {
            return;
        }
        _currentSong = song;
        _currentSongInstance = _currentSong.CreateInstance();
        _currentSongInstance.IsLooped = true;
        _currentMusicType = musicType;
        _state = MusicPlayerState.Playing;

        Update(new());

        _currentSongInstance.Play();
    }

    public void StopWithFade()
    {
        _state = MusicPlayerState.Stopping;
    }

    public void StopImmediately()
    {
        _currentSongInstance?.Stop();
        _currentSong = null;
        _currentMusicType = MusicType.None;
        _state = MusicPlayerState.Stopped;
    }

    public void Pause()
    {
        if (_currentSongInstance != null)
        {
            _isPause = true;
            _currentSongInstance.Pause();
        }
    }
    public void Resume()
    {
        if (_isPause)
        {
            _isPause = false;
            _currentSongInstance?.Resume();
        }
    }

    public void Update(GameTime gameTime)
    {
        switch (_state)
        {
            case MusicPlayerState.Stopped:
                break;
            case MusicPlayerState.Playing:
                {
                    if (_currentSongInstance is null)
                    {
                        return;
                    }
                   _currentSongInstance.Volume = Settings.MusicVolume;
                }
                break;
            case MusicPlayerState.Stopping:
                {
                    if (_currentSongInstance is null)
                    {
                        return;
                    }
                    const float fadeSpeed = 0.0001f;
                    var fadeAmount = fadeSpeed * gameTime.ElapsedGameTime.Milliseconds;
                    if (fadeAmount < _currentSongInstance.Volume)
                    {
                        _currentSongInstance.Volume -= fadeAmount;
                    }
                    else
                    {
                        _currentSongInstance.Volume = 0f;
                    }

                    if (_currentSongInstance.Volume <= 0f)
                    {
                        _currentSongInstance.Volume = 0;
                        StopImmediately();
                    }
                }
                break;
        }
    }
}