namespace Beta;

public class Settings
{
    public static bool IsDebug { get; set; }
    public static bool IsTutorialDebug { get; }

    public static float SoundVolume { get; set; } = 0.8f;
    public static float MusicVolume { get; set; } = 0.3f;

    public static float TextSpeed { get; set; } = 0.8f;
}