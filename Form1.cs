using FastBitmap;

namespace Lab2;

public partial class Form1 : Form
{
    private Bitmap? sourceBitmap;
    private bool updatingHsvControls;

    public Form1()
    {
        InitializeComponent();
    }

    public event EventHandler? SourceImageChanged;
    public event EventHandler? GrayscaleRequested;
    public event EventHandler? ChannelsRequested;
    public event EventHandler? HsvPreviewRequested;
    public event EventHandler<SaveResultEventArgs>? SaveHsvRequested;

    public Bitmap? SourceImage => sourceBitmap;
    public int HueOffset => hueTrackBar.Value;
    public int SaturationOffset => saturationTrackBar.Value;
    public int ValueOffset => valueTrackBar.Value;

    public void SetGrayscaleResults(
        Image? first,
        Image? second,
        Image? difference,
        IEnumerable<int>? firstHistogram = null,
        IEnumerable<int>? secondHistogram = null)
    {
        grayscaleFirstPreview.Image = first;
        grayscaleSecondPreview.Image = second;
        differencePreview.Image = difference;
        grayscaleFirstHistogram.SetValues(firstHistogram);
        grayscaleSecondHistogram.SetValues(secondHistogram);
    }

    public void SetChannelResults(
        Image? red,
        Image? green,
        Image? blue,
        IEnumerable<int>? redValues = null,
        IEnumerable<int>? greenValues = null,
        IEnumerable<int>? blueValues = null)
    {
        redChannelPreview.Image = red;
        greenChannelPreview.Image = green;
        blueChannelPreview.Image = blue;
        redHistogram.SetValues(redValues);
        greenHistogram.SetValues(greenValues);
        blueHistogram.SetValues(blueValues);
    }

    public void SetHsvResult(Image? result)
    {
        hsvResultPreview.Image = result;
        saveHsvButton.Enabled = result is not null;
    }

    private void OpenImageButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            CheckFileExists = true,
            Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff|PNG|*.png|JPEG|*.jpg;*.jpeg|Bitmap|*.bmp|Все файлы|*.*",
            Title = "Выберите исходное изображение"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            using var loadedImage = new Bitmap(dialog.FileName);
            // Select из FastBitmap создаёт отдельную копию без изменения цветов.
            var newBitmap = loadedImage.Select(color => color);

            sourcePreview.Image = null;
            sourceBitmap?.Dispose();
            sourceBitmap = newBitmap;
            sourcePreview.Image = sourceBitmap;

            clearImageButton.Enabled = true;
            runGrayscaleButton.Enabled = true;
            runChannelsButton.Enabled = true;
            updateHsvButton.Enabled = true;

            ClearResultViews();
            SourceImageChanged?.Invoke(this, EventArgs.Empty);
            if (autoPreviewCheckBox.Checked)
            {
                HsvPreviewRequested?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                $"Не удалось открыть изображение.\n\n{exception.Message}",
                "Ошибка загрузки",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ClearImageButton_Click(object? sender, EventArgs e)
    {
        sourcePreview.Image = null;
        sourceBitmap?.Dispose();
        sourceBitmap = null;

        clearImageButton.Enabled = false;
        runGrayscaleButton.Enabled = false;
        runChannelsButton.Enabled = false;
        updateHsvButton.Enabled = false;
        ClearResultViews();
        SourceImageChanged?.Invoke(this, EventArgs.Empty);
    }

    private void HsvTrackBar_ValueChanged(object? sender, EventArgs e)
    {
        hueValueLabel.Text = FormatSignedValue(hueTrackBar.Value, "°");
        saturationValueLabel.Text = FormatSignedValue(saturationTrackBar.Value, "%");
        valueValueLabel.Text = FormatSignedValue(valueTrackBar.Value, "%");

        if (!updatingHsvControls && sourceBitmap is not null && autoPreviewCheckBox.Checked)
        {
            HsvPreviewRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ResetHsvButton_Click(object? sender, EventArgs e)
    {
        updatingHsvControls = true;
        hueTrackBar.Value = 0;
        saturationTrackBar.Value = 0;
        valueTrackBar.Value = 0;
        updatingHsvControls = false;
        HsvTrackBar_ValueChanged(sender, e);
    }

    private void SaveHsvButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            AddExtension = true,
            DefaultExt = "png",
            Filter = "PNG|*.png|JPEG|*.jpg;*.jpeg|Bitmap|*.bmp",
            FileName = "hsv-result.png",
            OverwritePrompt = true,
            Title = "Сохранить результат"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            SaveHsvRequested?.Invoke(this, new SaveResultEventArgs(dialog.FileName));
        }
    }

    private void ClearResultViews()
    {
        SetGrayscaleResults(null, null, null);
        SetChannelResults(null, null, null);
        SetHsvResult(null);
    }

    private static string FormatSignedValue(int value, string suffix) => $"{(value > 0 ? "+" : string.Empty)}{value}{suffix}";

}

public sealed class SaveResultEventArgs(string fileName) : EventArgs
{
    public string FileName { get; } = fileName;
}
