using System;
using System.Text;

namespace Beta.Common.Extensions;

public static class StringExtensions
{
    private static readonly char[] _wordSeparator = [' '];

    public static string SetLineWidth(this string s, int width)
    {
        var sb = new StringBuilder();

        var lines = s.Split("\n");

        foreach (var line in lines)
        {
            var lineLength = 0;
            var words = line.Split(_wordSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                sb.AppendLine();
                continue;
            }

            foreach (var word in words)
            {
                if (lineLength + word.Length + 1 > width)
                {
                    sb.AppendLine();
                    lineLength = 0;
                }
                if (lineLength > 0)
                {
                    sb.Append(' ');
                }

                sb.Append(word);
                lineLength += word.Length + 1;
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}