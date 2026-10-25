namespace Lab6.UI;

internal static class EditorTheme
{
    public static Color Background => SystemColors.Control;
    public static Color Input => SystemColors.Window;
    public static Color Text => SystemColors.ControlText;
    public static Color Muted => SystemColors.GrayText;
    public static Color Border => SystemColors.ControlDark;

    public static Bitmap LoadIcon(string name)
    {
        using var stream = typeof(EditorTheme).Assembly.GetManifestResourceStream($"Lab6.Assets.UI.{name}.png")
            ?? throw new InvalidOperationException($"Не найдена иконка {name}.");
        using var image = Image.FromStream(stream);
        var result = new Bitmap(image.Width, image.Height);
        using var graphics = Graphics.FromImage(result);
        using var attributes = new System.Drawing.Imaging.ImageAttributes();
        attributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(
        [
            [0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0],
            [0, 0, 0, 1, 0],
            [Text.R / 255f, Text.G / 255f, Text.B / 255f, 0, 1]
        ]));
        graphics.DrawImage(image, new Rectangle(0, 0, result.Width, result.Height),
            0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
        return result;
    }
}
