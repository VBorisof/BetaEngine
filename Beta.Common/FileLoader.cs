using Microsoft.Xna.Framework;
using System.IO;

namespace Beta.Common;

public static class FileLoader
{
    public static string ReadAllFromFile(string path)
    {
        using var stream = TitleContainer.OpenStream(path);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

