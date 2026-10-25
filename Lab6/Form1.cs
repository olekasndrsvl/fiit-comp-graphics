using Lab6.Core.Geometry;
using Lab6.Core.Mathematics;
using Lab6.Core.Scene;
using Lab6.Modeling;

namespace Lab6;

public partial class Form1 : Form
{
    private SceneObject? sceneObject;
    private int selectedModel = 1;

    public Form1()
    {
        InitializeComponent();
        canvas.RenderPending += message => status.Text = message;
        Shown += (_, _) => UpdateScene("Выберите модель");
    }

    private void SelectModel(int index)
    {
        selectedModel = index;
        for (var i = 0; i < modelButtons.Count; i++)
            MarkSelected(modelButtons[i], i == selectedModel);
        sceneObject = new SceneObject(PlatonicSolids.Create(index));
        canvas.SceneObject = sceneObject;
        UpdateScene("Модель создана");
    }

    private SceneObject SelectedObject() =>
        sceneObject ?? throw new InvalidOperationException("Сначала создайте модель.");

    private void TranslateModel()
    {
        var item = SelectedObject();
        var offset = ReadPoint(translation);
        item.Apply(Matrix4.Translation(offset.X, offset.Y, offset.Z));
        UpdateScene("Смещение выполнено");
    }

    private void Rotate(bool arbitrary)
    {
        var item = SelectedObject();
        var radians = (double)(arbitrary ? lineAngle.Value : angle.Value) * Math.PI / 180;
        Matrix4 operation;
        if (arbitrary)
        {
            operation = Matrix4.RotationAroundLine(ReadPoint(lineStart), ReadPoint(lineEnd), radians);
        }
        else
        {
            var direction = rotationAxis.SelectedIndex switch
            {
                0 => new Point3D(1, 0, 0),
                1 => new Point3D(0, 1, 0),
                _ => new Point3D(0, 0, 1)
            };
            operation = Matrix4.Rotation(direction.X, direction.Y, direction.Z, radians);
            if (rotationCenter.Checked)
                operation = Matrix4.Around(item.Center, operation);
        }
        item.Apply(operation);
        UpdateScene("Поворот выполнен");
    }

    private void ScaleModel()
    {
        var item = SelectedObject();
        var scale = ReadPoint(scaleInputs);
        var operation = Matrix4.Scale(scale.X, scale.Y, scale.Z);
        if (scaleCenter.Checked)
            operation = Matrix4.Around(item.Center, operation);
        item.Apply(operation);
        UpdateScene("Масштабирование выполнено");
    }

    private void Reflect()
    {
        var item = SelectedObject();
        item.Apply(Matrix4.Reflection(reflectionPlane.Text));
        UpdateScene("Отражение выполнено");
    }

    private void ResetModel()
    {
        sceneObject?.Reset();
        UpdateScene(sceneObject is null ? "Модель не создана" : "Преобразования сброшены");
    }

    private void RunAction(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex) when (ex is NotImplementedException or ArgumentException or InvalidOperationException)
        {
            // В каркасе нажатие кнопки показывает задачу, а не закрывает приложение.
            status.Text = ex.Message;
        }
    }

    private void UpdateScene(string message)
    {
        canvas.Perspective = projectionPicker.SelectedIndex == 1;
        canvas.RefreshFrame();
        modelInfo.Text = sceneObject is null ? "Модель не создана" :
            $"Вершин: {sceneObject.Geometry.Positions.Count} · Граней: {sceneObject.Geometry.Faces.Count}";
        centerInfo.Text = "Центр: —";
        if (sceneObject is not null)
        {
            try
            {
                var center = sceneObject.Center;
                centerInfo.Text = $"Центр: ({center.X:0.##}; {center.Y:0.##}; {center.Z:0.##})";
            }
            catch (NotImplementedException)
            {
                // Данные модели доступны, даже если вычисление центра ещё не готово.
            }
        }
        status.Text = message;
    }

    private static Point3D ReadPoint(NumericUpDown[] fields) =>
        new((double)fields[0].Value, (double)fields[1].Value, (double)fields[2].Value);
}
