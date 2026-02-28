using System;
using Beta.Common.Extensions;

namespace Beta.Phrases;

public class Phrase
{
    public string Text { get; set; }

    public Phrase(string text, int width)
    {
        Text = text.SetLineWidth(width);
    }

    public float GetSpeed()
    {
        const float maxSpeed = 0.01f;
        const float minSpeed = 0.5f;
        return minSpeed - (Settings.TextSpeed * (minSpeed - maxSpeed));
    }

    // Experimentally (by me) backed is that the rate is
    // about 0.05 s/length
    public float SecondsDuration => Math.Max(1f, Text.Length * GetSpeed());
}