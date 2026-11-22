namespace Task2;

internal readonly record struct ColorRgb(float R, float G, float B)
{
    public static ColorRgb Black => new(0f, 0f, 0f);
    public static ColorRgb White => new(1f, 1f, 1f);

    public static ColorRgb operator +(ColorRgb left, ColorRgb right) =>
        new(left.R + right.R, left.G + right.G, left.B + right.B);

    public static ColorRgb operator *(ColorRgb color, float factor) =>
        new(color.R * factor, color.G * factor, color.B * factor);

    public static ColorRgb operator *(float factor, ColorRgb color) => color * factor;

    public static ColorRgb operator *(ColorRgb left, ColorRgb right) =>
        new(left.R * right.R, left.G * right.G, left.B * right.B);

    public static ColorRgb Lerp(ColorRgb start, ColorRgb end, float amount) =>
        start * (1f - amount) + end * amount;
}
