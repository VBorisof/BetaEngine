using Beta.Scenes;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Beta.Extensions;

public static class CameraExtensions
{
    // TODO: Game time?
    private static float _cameraSpeed = 10f;

    public static void MoveCameraRight(this OrthographicCamera camera, SceneManager sceneManager)
    {
        if (camera.Position.X + camera.BoundingRectangle.Width
            < sceneManager.CurrentScene?.Texture?.Bounds.Width - _cameraSpeed)
        {
            camera.Position += new Vector2(_cameraSpeed, 0);
        }
    }

    public static void MoveCameraLeft(this OrthographicCamera camera)
    {
        if (camera.Position.X > _cameraSpeed)
        {
            camera.Position -= new Vector2(_cameraSpeed, 0);
        }
    }

    public static void ClampCameraX(this OrthographicCamera camera, float minX, float maxX)
    {
        if (camera.Position.X > maxX)
        {
            camera.Position = new Vector2(maxX, camera.Position.Y);
        }
        if (camera.Position.X < minX)
        {
            camera.Position = new Vector2(minX, camera.Position.Y);
        }
    }

    public static void ClampCameraY(this OrthographicCamera camera, float minY, float maxY)
    {
        if (camera.Position.Y > maxY)
        {
            camera.Position = new Vector2(camera.Position.X, maxY);
        }
        if (camera.Position.Y < minY)
        {
            camera.Position = new Vector2(camera.Position.X, minY);
        }
    }
}


