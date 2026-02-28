using Beta.DI;
using Beta.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beta.Videos;

public class VideoPlayer
{
    private Video? _currentVideo;

    public bool IsPlaying { get; private set; }
    private readonly ContentCache _contentCache;

    public VideoPlayer()
    {
        _contentCache = DependencyContainer.Instance.Get<ContentCache>();
    }

    // TODO: Also RollVideo... Also actual videos...
    public void StartFadeVideo(string name, float fadeInSpeed, float fadeOutSpeed, float frameDuration)
    {
        var video = new FadeVideo(
            fadeInDuration: fadeInSpeed,
            fadeOutDuration: fadeOutSpeed,
            frameDuration: frameDuration,
            frame: _contentCache.Get<Texture2D>($"img/vid/{name}")
        );
        _currentVideo = video;
        _currentVideo.Reset();
        IsPlaying = true;
    }

    public void Update(GameTime gameTime)
    {
        if (_currentVideo != null)
        {
            _currentVideo.Update(gameTime);
            if (_currentVideo.IsDone)
            {
                IsPlaying = false;
                _currentVideo = null;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _currentVideo?.Draw(spriteBatch);
    }
}