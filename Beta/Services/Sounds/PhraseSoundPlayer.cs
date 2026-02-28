using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Beta.Services.Sounds;

public class PhraseSoundPlayer
{
    private int _currentCharIdx;
    private int _currentWordIdx;

    private string[] _words;
    private TimeSpan _charDuration;
    private TimeSpan? _elapsed;
    private readonly SoundEffect _sfx;

    private readonly Random _random = new();

    public PhraseSoundPlayer(SoundEffect sfx, string text)
    {
        _words = text.Split(' ');
        _currentCharIdx = 0;
        _currentWordIdx = 0;
        _charDuration = sfx.Duration;
        _elapsed = null;
        _sfx = sfx;
    }

    public void PlayChar(char c, float percentWord)
    {
        var volume = Settings.SoundVolume;

        // The higher the percent, the higher the pitch we prefer?
        var pitchDir = _random.NextSingle() < percentWord ? 1 : -1;

        var randWeight = 0.08f; // Kind of emotion-level

        var pitch = pitchDir * (_random.NextSingle() * randWeight);
        _sfx.Play(volume, pitch, pan: 0.0f);
    }

    public void Update(GameTime gameTime)
    {
        if (_currentWordIdx > _words.Length - 1)
        {
            return;
        }

        if (_elapsed is not null && _elapsed < _charDuration)
        {
            _elapsed += gameTime.ElapsedGameTime;
            return;
        }

        ++_currentCharIdx;
        if (_currentCharIdx > _words[_currentWordIdx].Length - 1)
        {
            _currentCharIdx = 0;
            ++_currentWordIdx;
            _elapsed = TimeSpan.Zero;
            return;
        }
        _elapsed = TimeSpan.Zero;

        /*
        const int skipEvery = 10;
        if (_totalChars % skipEvery == 0)
        {
            return;
        }
        */

        var ch = _words[_currentWordIdx][_currentCharIdx];
        if (char.IsLetter(ch))
        {
            var percentWord = (float)_currentCharIdx / _words[_currentWordIdx].Length;
            PlayChar(ch, percentWord);
        }
    }
}
