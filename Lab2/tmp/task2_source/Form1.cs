using System.Numerics;

namespace Task2;

public partial class Form1 : Form
{
    private readonly ObjectUiState[] _objects =
    [
        new("Куб 1"),
        new("Куб 2"),
        new("Шар 1"),
        new("Шар 2")
    ];

    private readonly LightUiState[] _lights =
    [
        new(true, 0m, 3.6m, 0.5m),
        new(false, -3m, 1.5m, 1.5m)
    ];

    private string? _mirroredWall;
    private bool _updatingLightControls;
    private CancellationTokenSource? _renderCancellation;
    private Bitmap? _renderedImage;

    public Form1()
    {
        InitializeComponent();
        InitializeSceneControls();
        Shown += Form1_Shown;
    }

    private void InitializeSceneControls()
    {
        objectSelector.Items.AddRange(_objects.Select(item => (object)item.Name).ToArray());
        wallSelector.Items.AddRange(
        [
            "Левая", "Правая", "Задняя", "Передняя", "Пол", "Потолок"
        ]);
        lightSelector.Items.AddRange(
        [
            "Источник 1 (основной)",
            "Источник 2 (дополнительный)"
        ]);

        objectSelector.SelectedIndex = 0;
        wallSelector.SelectedIndex = 2;
        lightSelector.SelectedIndex = 0;
        UpdateObjectStatus();
        UpdateWallStatus();
        LoadSelectedLight();
    }

    private void Form1_Shown(object? sender, EventArgs e)
    {
        Shown -= Form1_Shown;
        renderButton.PerformClick();
    }

    private ObjectUiState? SelectedObject =>
        objectSelector.SelectedIndex is >= 0 and < 4
            ? _objects[objectSelector.SelectedIndex]
            : null;

    private LightUiState? SelectedLight =>
        lightSelector.SelectedIndex is >= 0 and < 2
            ? _lights[lightSelector.SelectedIndex]
            : null;

    private void ObjectSelector_SelectedIndexChanged(object? sender, EventArgs e) => UpdateObjectStatus();

    private void MirrorOnButton_Click(object? sender, EventArgs e)
    {
        if (SelectedObject is { } state)
            state.IsMirror = true;
        UpdateObjectStatus();
    }

    private void MirrorOffButton_Click(object? sender, EventArgs e)
    {
        if (SelectedObject is { } state)
            state.IsMirror = false;
        UpdateObjectStatus();
    }

    private void TransparencyOnButton_Click(object? sender, EventArgs e)
    {
        if (SelectedObject is { } state)
            state.IsTransparent = true;
        UpdateObjectStatus();
    }

    private void TransparencyOffButton_Click(object? sender, EventArgs e)
    {
        if (SelectedObject is { } state)
            state.IsTransparent = false;
        UpdateObjectStatus();
    }

    private void UpdateObjectStatus()
    {
        if (SelectedObject is not { } state)
            return;

        objectStateLabel.Text =
            $"Зеркальность: {(state.IsMirror ? "вкл." : "выкл.")}   " +
            $"Прозрачность: {(state.IsTransparent ? "вкл." : "выкл.")}";
    }

    private void WallMirrorOnButton_Click(object? sender, EventArgs e)
    {
        _mirroredWall = wallSelector.SelectedItem?.ToString();
        UpdateWallStatus();
    }

    private void WallMirrorOffButton_Click(object? sender, EventArgs e)
    {
        _mirroredWall = null;
        UpdateWallStatus();
    }

    private void UpdateWallStatus()
    {
        wallStateLabel.Text = _mirroredWall is null
            ? "Зеркальная стена: нет"
            : $"Зеркальная стена: {_mirroredWall}";
    }

    private void LightSelector_SelectedIndexChanged(object? sender, EventArgs e) => LoadSelectedLight();

    private void LoadSelectedLight()
    {
        if (SelectedLight is not { } light)
            return;

        _updatingLightControls = true;
        lightX.Value = light.X;
        lightY.Value = light.Y;
        lightZ.Value = light.Z;
        _updatingLightControls = false;
        UpdateLightStatus();
    }

    private void LightOnButton_Click(object? sender, EventArgs e)
    {
        if (SelectedLight is { } light)
            light.Enabled = true;
        UpdateLightStatus();
    }

    private void LightOffButton_Click(object? sender, EventArgs e)
    {
        if (SelectedLight is { } light)
            light.Enabled = false;
        UpdateLightStatus();
    }

    private void LightPosition_ValueChanged(object? sender, EventArgs e)
    {
        if (_updatingLightControls || SelectedLight is not { } light)
            return;

        light.X = lightX.Value;
        light.Y = lightY.Value;
        light.Z = lightZ.Value;
        UpdateLightStatus();
    }

    private void UpdateLightStatus()
    {
        if (SelectedLight is not { } light)
            return;

        lightStateLabel.Text = light.Enabled
            ? $"Включён: ({light.X:0.0}; {light.Y:0.0}; {light.Z:0.0})"
            : "Выключен";
    }

    private async void RenderButton_Click(object? sender, EventArgs e)
    {
        if (_renderCancellation is not null)
        {
            _renderCancellation.Cancel();
            return;
        }

        var settings = CreateRenderSettings();
        var width = Math.Clamp(renderViewport.ClientSize.Width, 480, 800);
        var height = Math.Clamp(renderViewport.ClientSize.Height, 360, 620);
        var cancellation = new CancellationTokenSource();
        _renderCancellation = cancellation;
        renderButton.Text = "Отмена";
        resetButton.Enabled = false;
        renderStatusLabel.Text = "Построение изображения...";
        statusLabel.Text = "Рендеринг: 0%";

        var progress = new Progress<int>(value => statusLabel.Text = $"Рендеринг: {value}%");

        try
        {
            var bitmap = await Task.Run(
                () => RayTracer.Render(settings, width, height, progress, cancellation.Token),
                cancellation.Token);

            var oldImage = _renderedImage;
            _renderedImage = bitmap;
            renderViewport.Image = bitmap;
            oldImage?.Dispose();

            var enabledLights = _lights.Count(light => light.Enabled);
            renderStatusLabel.Text = $"Готово. Источников света: {enabledLights}.";
            statusLabel.Text = $"Готово: {width} × {height}";
        }
        catch (OperationCanceledException)
        {
            renderStatusLabel.Text = "Рендеринг отменён.";
            statusLabel.Text = "Отменено";
        }
        catch (Exception exception)
        {
            renderStatusLabel.Text = "Ошибка рендеринга.";
            statusLabel.Text = exception.Message;
            MessageBox.Show(
                this,
                exception.Message,
                "Ошибка рендеринга",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _renderCancellation = null;
            cancellation.Dispose();
            renderButton.Text = "Рендер";
            resetButton.Enabled = true;
        }
    }

    private SceneSettings CreateRenderSettings()
    {
        var objects = _objects
            .Select(item => new ObjectRenderSettings(item.IsMirror, item.IsTransparent))
            .ToArray();

        var lights = _lights
            .Select((item, index) => new PointLightSettings(
                item.Enabled,
                new Vector3((float)item.X, (float)item.Y, (float)item.Z),
                index == 0 ? 1.65f : 1.25f))
            .ToArray();

        return new SceneSettings(objects, lights, _mirroredWall);
    }

    private void ResetButton_Click(object? sender, EventArgs e)
    {
        foreach (var state in _objects)
        {
            state.IsMirror = false;
            state.IsTransparent = false;
        }

        _mirroredWall = null;
        _lights[0].Set(true, 0m, 3.6m, 0.5m);
        _lights[1].Set(false, -3m, 1.5m, 1.5m);
        UpdateObjectStatus();
        UpdateWallStatus();
        LoadSelectedLight();
        renderStatusLabel.Text = "Параметры сцены сброшены.";
        statusLabel.Text = "Нажмите «Рендер» для обновления";
    }

    private void RenderViewport_Paint(object? sender, PaintEventArgs e)
    {
        if (renderViewport.Image is not null)
            return;

        const string text = "Подготовка Корнуэльской комнаты...";
        var size = e.Graphics.MeasureString(text, Font);
        var x = (renderViewport.ClientSize.Width - size.Width) / 2f;
        var y = (renderViewport.ClientSize.Height - size.Height) / 2f;
        e.Graphics.DrawString(text, Font, Brushes.Gainsboro, x, y);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _renderCancellation?.Cancel();
        _renderCancellation?.Dispose();
        _renderedImage?.Dispose();
        base.OnFormClosed(e);
    }

    private sealed class ObjectUiState(string name)
    {
        public string Name { get; } = name;
        public bool IsMirror { get; set; }
        public bool IsTransparent { get; set; }
    }

    private sealed class LightUiState(bool enabled, decimal x, decimal y, decimal z)
    {
        public bool Enabled { get; set; } = enabled;
        public decimal X { get; set; } = x;
        public decimal Y { get; set; } = y;
        public decimal Z { get; set; } = z;

        public void Set(bool isEnabled, decimal newX, decimal newY, decimal newZ)
        {
            Enabled = isEnabled;
            X = newX;
            Y = newY;
            Z = newZ;
        }
    }
}
