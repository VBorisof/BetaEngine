using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace Beta.Entities.Animations;

public class Animation
{
    public string Name { get; set; }
    public float Speed { get; set; }
    public bool Repeat { get; set; }
    public bool IsDone { get; set; }

    public List<string> FramePaths { get; private set; }
    private readonly List<Texture2D> _frames;
    private int _currentFrameId;

    private float _timer;

    public Animation(string name, float speed, bool repeat, List<string> framePaths, List<Texture2D> frames)
    {
        Name = name;
        Speed = speed;
        Repeat = repeat;
        _frames = frames;
        FramePaths = framePaths;
    }

    public Texture2D GetFirstFrame()
    {
        return _frames.First();
    }

    public Texture2D GetCurrentFrame()
    {
        return _frames[_currentFrameId];
    }

    public void Update(GameTime gameTime)
    {
        _timer += gameTime.ElapsedGameTime.Milliseconds;
        if (_timer >= 1f / Speed)
        {
            _timer = 0;
            if (_currentFrameId >= _frames.Count - 1)
            {
                _currentFrameId = 0;
                if (!Repeat)
                {
                    IsDone = true;
                }
            }
            else
            {
                ++_currentFrameId;
            }
        }
    }
}