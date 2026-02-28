using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Beta.DI;
using Beta.Common;
using Beta.Verbs;
using MonoGame.Extended;

namespace Beta.Cursors;

public class Cursor
{
    private readonly OrthographicCamera _camera;
    private readonly ContentCache _contentCache;

    public Texture2D CursorTexture { get; private set; }
    public Texture2D PickupTexture { get; private set; }
    public Texture2D InteractTexture { get; private set; }
    public Texture2D TalkTexture { get; private set; }
    public Texture2D LookTexture { get; private set; }
    public Texture2D ExitTexture { get; private set; }
    private Texture2D _currentCursor;

    public Cursor()
    {
        _contentCache = DependencyContainer.Instance.Get<ContentCache>();

        CursorTexture = _contentCache.Get<Texture2D>("img/cursor/cursor");
        PickupTexture = _contentCache.Get<Texture2D>("img/cursor/pickup");
        InteractTexture = _contentCache.Get<Texture2D>("img/cursor/interact");
        TalkTexture = _contentCache.Get<Texture2D>("img/cursor/talk");
        LookTexture = _contentCache.Get<Texture2D>("img/cursor/look");
        ExitTexture = _contentCache.Get<Texture2D>("img/cursor/exit");
        _currentCursor = CursorTexture;

        _camera = DependencyContainer.Instance.Get<OrthographicCamera>();
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 cursorPos)
    {
        const int cursorWidth = 64;
        var cursorHeight = cursorWidth * _currentCursor.Height / _currentCursor.Width;

        float rotation = 0;
        var origin = Vector2.Zero;
        if (_currentCursor == ExitTexture)
        {
            origin = new Vector2(cursorWidth / 2, cursorHeight / 2);
            rotation = GetExitArrowRotation(
                _camera.ScreenToWorld(cursorPos),
                _camera.ScreenToWorld(_camera.Center)
            );
        }

        spriteBatch.Draw(
            _currentCursor,
            sourceRectangle: new Rectangle(
                0, 0, _currentCursor.Width, _currentCursor.Height
            ),
            destinationRectangle: new Rectangle(
                (int)cursorPos.X - (cursorWidth / 2), (int)cursorPos.Y - (cursorHeight / 2), cursorWidth, cursorHeight
            ),
            color: Color.White,
            rotation: rotation,
            origin: origin,
            effects: SpriteEffects.None,
            layerDepth: Constants.LayerDepthCursor
        );
    }

    public void SetCursor(Verb verb)
    {
        switch (verb)
        {
            case Verb.Walk:
                _currentCursor = CursorTexture;
                break;
            case Verb.Look:
                _currentCursor = LookTexture;
                break;
            case Verb.Pickup:
                _currentCursor = PickupTexture;
                break;
            case Verb.Interact:
                _currentCursor = InteractTexture;
                break;
            case Verb.Talk:
                _currentCursor = TalkTexture;
                break;
            case Verb.Use:
                //_currentCursor = Use;
                break;
            default:
                break;
        }
    }

    public void SetExit()
    {
        _currentCursor = ExitTexture;
    }


    public static float GetExitArrowRotation(Vector2 scenePos, Vector2 sceneCenterPos)
    {
        var vec1 = new Vector2(1, 0); // Where the arrow originally points.
        var vec2 = scenePos - sceneCenterPos;

        // Little hack so the arrow doesn't roll around like crazy
        const int tolerance = 100;
        if (vec2.Length() < tolerance)
        {
            return (float)-System.Math.PI / 2;
        }
        else
        {
            double dot = Vector2.Dot(vec1, vec2);
            var cosTh = dot / (vec1.Length() * vec2.Length());
            var theta = (float)System.Math.Acos(cosTh);

            double crossProduct = (vec1.X * vec2.Y) - (vec1.Y * vec2.X);
            var isClockwise = crossProduct < 0;

            return isClockwise ? -theta : theta;
        }
    }
}