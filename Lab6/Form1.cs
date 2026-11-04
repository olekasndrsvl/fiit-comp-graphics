using Lab6.Core.Geometry;
using Lab6.Core.Mathematics;
using Lab6.Core.Scene;
using Lab6.IO;
using Lab6.Modeling;
using MathNet.Numerics.LinearAlgebra;

namespace Lab6;

public partial class Form1 : Form
{
    private SceneObject? sceneObject;
    private int selectedModel = 1;
    private string? currentObjPath;

    public Form1()
    {
        InitializeComponent();
        canvas.RenderPending += m => status.Text = m;
        canvas.ShowAxes = true;      
        Shown += (_, _) => UpdateScene("Выберите модель");
    }

    private void SelectModel(int index)
    {
        selectedModel = index;
        for (var i = 0; i < modelButtons.Count; i++)
            MarkSelected(modelButtons[i], i == selectedModel);
        sceneObject = new SceneObject(PlatonicSolids.Create(index));
        canvas.SceneObject = sceneObject;
        currentObjPath = null;
        objFileInfo.Text = "Встроенная модель";
        UpdateScene("Модель создана");
    }

    private void LoadObjModel()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Загрузить модель Wavefront OBJ",
            Filter = "Wavefront OBJ (*.obj)|*.obj|Все файлы (*.*)|*.*",
            FilterIndex = 1,
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        var geometry = ObjSerializer.Load(dialog.FileName);
        sceneObject = new SceneObject(geometry);
        canvas.SceneObject = sceneObject;
        currentObjPath = dialog.FileName;
        objFileInfo.Text = Path.GetFileName(dialog.FileName);
        foreach (var button in modelButtons)
            MarkSelected(button, false);
        UpdateScene($"OBJ загружен: {Path.GetFileName(dialog.FileName)}");
    }

    private void SaveObjModel()
    {
        var item = SelectedObject();
        using var dialog = new SaveFileDialog
        {
            Title = "Сохранить модель Wavefront OBJ",
            Filter = "Wavefront OBJ (*.obj)|*.obj|Все файлы (*.*)|*.*",
            FilterIndex = 1,
            AddExtension = true,
            DefaultExt = "obj",
            OverwritePrompt = true,
            FileName = currentObjPath is null ? "model.obj" : Path.GetFileName(currentObjPath)
        };
        if (currentObjPath is not null)
            dialog.InitialDirectory = Path.GetDirectoryName(currentObjPath);

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        ObjSerializer.Save(dialog.FileName, item.Geometry, item.Model);
        currentObjPath = dialog.FileName;
        objFileInfo.Text = Path.GetFileName(dialog.FileName);
        UpdateScene($"OBJ сохранён: {Path.GetFileName(dialog.FileName)}");
    }

    private void AddRevolutionPoint() =>
        throw new NotImplementedException("TODO 2: добавление точки образующей.");

    private void RemoveRevolutionPoint() =>
        throw new NotImplementedException("TODO 2: удаление выбранной точки образующей.");

    private void ClearRevolutionPoints() =>
        throw new NotImplementedException("TODO 2: очистка точек образующей.");

    private void BuildRevolutionModel() =>
        throw new NotImplementedException("TODO 2: построение фигуры вращения.");


    private void BuildFunctionSurface()
    {
        var function = surfaceFunction.SelectedIndex switch
        {
            0 => SurfaceFunction.SinXPlusCosY,
            1 => SurfaceFunction.SinRadius,
            2 => SurfaceFunction.XSquaredMinusYSquared,
            3 => SurfaceFunction.CosXSinY,
            _ => throw new InvalidOperationException("Выберите функцию.")
        };

        double xMin = (double)surfaceXMin.Value;
        double xMax = (double)surfaceXMax.Value;
        double yMin = (double)surfaceYMin.Value;
        double yMax = (double)surfaceYMax.Value;

        int xDivisions = (int)surfaceXDivisions.Value;
        int yDivisions = (int)surfaceYDivisions.Value;

        var geometry = FunctionSurfaceBuilder.Create(
            function,
            xMin,
            xMax,
            yMin,
            yMax,
            xDivisions,
            yDivisions);

        sceneObject = new SceneObject(geometry);
        canvas.SceneObject = sceneObject;

        currentObjPath = null;

        foreach (var button in modelButtons)
            MarkSelected(button, false);

        objFileInfo.Text = "Поверхность функции";

        UpdateScene("График функции построен");
    }


    private SceneObject SelectedObject() =>
        sceneObject ?? throw new InvalidOperationException("Сначала создайте модель.");

    private void TranslateModel()
    {
        var item = SelectedObject();
        var offset = ReadPoint(translation);
        item.Apply(TransformMatrix.Translation(offset.X, offset.Y, offset.Z));
        UpdateScene("Смещение выполнено");
    }

    private void Rotate(bool arbitrary)
    {
        var item = SelectedObject();
        var radians = (double)(arbitrary ? lineAngle.Value : angle.Value) * Math.PI / 180;
        Matrix<double> operation;
        if (arbitrary)
        {
            operation = TransformMatrix.RotationAroundLine(ReadPoint(lineStart), ReadPoint(lineEnd), radians);
        }
        else
        {
            var direction = rotationAxis.SelectedIndex switch
            {
                0 => new Point3D(1, 0, 0),
                1 => new Point3D(0, 1, 0),
                _ => new Point3D(0, 0, 1)
            };
            operation = TransformMatrix.Rotation(direction.X, direction.Y, direction.Z, radians);
            if (rotationCenter.Checked)
                operation = TransformMatrix.Around(item.Center, operation);
        }
        item.Apply(operation);
        UpdateScene("Поворот выполнен");
    }

    private void ScaleModel()
    {
        var item = SelectedObject();
        var scale = ReadPoint(scaleInputs);
        var operation = TransformMatrix.Scale(scale.X, scale.Y, scale.Z);
        if (scaleCenter.Checked)
            operation = TransformMatrix.Around(item.Center, operation);
        item.Apply(operation);
        UpdateScene("Масштабирование выполнено");
    }

    private void Reflect()
    {
        var item = SelectedObject();
        item.Apply(TransformMatrix.Reflection(reflectionPlane.Text));
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
        catch (Exception ex) when (ex is
            NotImplementedException or ArgumentException or InvalidOperationException or
            FormatException or IOException or UnauthorizedAccessException)
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
