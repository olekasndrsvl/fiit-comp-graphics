using Lab6.Core.Geometry;
using Lab6.Core.Mathematics;
using Lab6.Core.Scene;
using Lab6.IO;
using Lab6.Modeling;
using MathNet.Numerics.LinearAlgebra;

namespace Lab6;

public partial class Form1 : Form
{
    private readonly Scene scene = new();
    private int selectedModel = 1;
    private readonly FigureRevolution figureRevolution = new();

    public Form1()
    {
        InitializeComponent();
        canvas.Scene = scene;
        canvas.RenderPending += m => status.Text = m;
        canvas.ShowAxes = true;
        Shown += (_, _) => UpdateScene("Выберите модель");
    }

    private void SelectModel(int index)
    {
        selectedModel = index;
        for (var i = 0; i < modelButtons.Count; i++)
            MarkSelected(modelButtons[i], i == selectedModel);
        var names = new[] { "Тетраэдр", "Гексаэдр", "Октаэдр", "Икосаэдр", "Додекаэдр" };
        AddObject(names[index], PlatonicSolids.Create(index));
        UpdateScene($"Добавлен объект: {names[index]}");
    }

    private void LoadObjModel()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Загрузить модель Wavefront OBJ",
            Filter = "Wavefront OBJ (*.obj)|*.obj|Все файлы (*.*)|*.*",
            FilterIndex = 1,
            CheckFileExists = true,
            Multiselect = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        foreach (var filePath in dialog.FileNames)
        {
            var geometry = ObjSerializer.Load(filePath);
            AddObject(Path.GetFileNameWithoutExtension(filePath), geometry, filePath);
        }

        foreach (var button in modelButtons)
            MarkSelected(button, false);
        UpdateScene($"Добавлено OBJ-файлов: {dialog.FileNames.Length}");
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
            FileName = item.SourcePath is null ? $"{item.Name}.obj" : Path.GetFileName(item.SourcePath)
        };
        if (item.SourcePath is not null)
            dialog.InitialDirectory = Path.GetDirectoryName(item.SourcePath);

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        ObjSerializer.Save(dialog.FileName, item.Geometry, item.Model);
        item.SourcePath = dialog.FileName;
        UpdateScene($"OBJ сохранён: {Path.GetFileName(dialog.FileName)}");
    }

    private void AddRevolutionPoint()
    {
        var point = ReadPoint(revolutionPoint);

        figureRevolution.AddPoint(point);
        RefreshRevolutionPointList(); // обновлем список точек на экране

        revolutionPoints.SelectedIndex = revolutionPoints.Items.Count - 1;

        status.Text = $"Точка образующей добавлена. Всего точек: {figureRevolution.Profile.Count}";
    }

    private void RemoveRevolutionPoint()
    {
        var index = revolutionPoints.SelectedIndex;

        figureRevolution.RemovePoint(index);
        RefreshRevolutionPointList();

        status.Text = "Точка образующей удалена";
    }

    private void ClearRevolutionPoints()
    {
        figureRevolution.Clear();
        revolutionPoints.Items.Clear();

        status.Text = "Образующая очищена";
    }

    private void BuildRevolutionModel()
    {
        var divisions = decimal.ToInt32(revolutionDivisions.Value);

        var axisIndex = revolutionAxis.SelectedIndex;
        if (axisIndex < 0) axisIndex = 0;

        var geometry = figureRevolution.Generate(axisIndex, divisions);

        AddObject("Фигура вращения", geometry);

        foreach (var button in modelButtons)
            MarkSelected(button, false);

        UpdateScene($"Фигура вращения построена: {geometry.Positions.Count} вершин, {geometry.Faces.Count} граней");
    }

    private void RefreshRevolutionPointList()
    {
        var selectedIndex = revolutionPoints.SelectedIndex;

        revolutionPoints.BeginUpdate();
        revolutionPoints.Items.Clear();

        for (var i = 0; i < figureRevolution.Profile.Count; i++)
        {
            var point = figureRevolution.Profile[i];

            revolutionPoints.Items.Add(
                $"{i + 1}. ({point.X:0.##}; {point.Y:0.##}; {point.Z:0.##})");
        }

        if (figureRevolution.Profile.Count > 0)
        {
            revolutionPoints.SelectedIndex = Math.Clamp(
                selectedIndex,
                0,
                figureRevolution.Profile.Count - 1);
        }

        revolutionPoints.EndUpdate();
    }

    private void LoadRevolutionTemplate()
    {
        var points = revolutionTemplate.SelectedIndex switch
        {
            // Сосуд
            0 => new[]
            {
            new Point3D(0.8, 0.0, 0),
            new Point3D(1.2, 0.2, 0),
            new Point3D(1.5, 0.8, 0),
            new Point3D(1.6, 1.5, 0),
            new Point3D(1.4, 2.0, 0),
            new Point3D(1.0, 2.3, 0),
            new Point3D(0.6, 2.5, 0),
            new Point3D(0.5, 2.8, 0),
            new Point3D(0.6, 3.0, 0)
        },

            // Ваза
            1 => new[]
            {
            new Point3D(0.5, 0.0, 0),
            new Point3D(0.7, 0.3, 0),
            new Point3D(1.0, 0.8, 0),
            new Point3D(1.3, 1.3, 0),
            new Point3D(1.4, 1.8, 0),
            new Point3D(1.2, 2.2, 0),
            new Point3D(0.8, 2.5, 0),
            new Point3D(0.9, 2.8, 0),
            new Point3D(1.2, 3.0, 0)
        },

            // Бокал
            2 => new[]
            {
            new Point3D(0.0, 0.0, 0),
            new Point3D(0.5, 0.0, 0),
            new Point3D(0.6, 0.1, 0),
            new Point3D(0.2, 0.2, 0),
            new Point3D(0.12, 0.4, 0),
            new Point3D(0.12, 1.1, 0),
            new Point3D(0.25, 1.2, 0),
            new Point3D(0.55, 1.35, 0),
            new Point3D(0.8, 1.55, 0),
            new Point3D(1.0, 1.85, 0),
            new Point3D(1.1, 2.2, 0),
            new Point3D(1.15, 2.5, 0)
        },

            // Ракета
            3 => new[]
            {
            new Point3D(0.0, 4.0, 0),
            new Point3D(0.25, 3.7, 0),
            new Point3D(0.5, 3.3, 0),
            new Point3D(0.65, 2.9, 0),
            new Point3D(0.65, 1.2, 0),
            new Point3D(0.7, 0.8, 0),
            new Point3D(1.0, 0.25, 0),
            new Point3D(0.75, 0.4, 0),
            new Point3D(0.65, 0.6, 0),
            new Point3D(0.65, 0.1, 0),
            new Point3D(0.0, 0.0, 0)
        },

            // Песочные часы
            4 => new[]
            {
            new Point3D(0.9, 0.0, 0),
            new Point3D(1.0, 0.5, 0),
            new Point3D(0.7, 1.0, 0),
            new Point3D(0.3, 1.5, 0),
            new Point3D(0.1, 1.9, 0),
            new Point3D(0.3, 2.1, 0),
            new Point3D(0.7, 2.5, 0),
            new Point3D(1.0, 3.0, 0)
        },

            _ => throw new InvalidOperationException("Неизвестный шаблон.")
        };

        figureRevolution.Clear();

        foreach (var point in points)
        {
            figureRevolution.AddPoint(point);
        }

        RefreshRevolutionPointList();

        status.Text =
            $"Загружен шаблон «{revolutionTemplate.SelectedItem}». Точек: {points.Length}";
    }


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

        AddObject("График функции", geometry);

        foreach (var button in modelButtons)
            MarkSelected(button, false);

        UpdateScene("График функции построен");
    }

    private SceneObject AddObject(
        string name,
        Polyhedron geometry,
        string? sourcePath = null)
    {
        var item = scene.Add(name, geometry, sourcePath);
        RefreshSceneObjectList();
        return item;
    }

    private void RefreshSceneObjectList()
    {
        sceneObjects.BeginUpdate();
        sceneObjects.Items.Clear();
        foreach (var item in scene.Objects)
            sceneObjects.Items.Add(item);
        sceneObjects.SelectedItem = scene.SelectedObject;
        sceneObjects.EndUpdate();
    }

    private void SelectSceneObject()
    {
        if (sceneObjects.SelectedItem is not SceneObject item)
            return;

        scene.Select(item);
        UpdateScene($"Выбран объект: {item.Name}");
    }

    private void RemoveSelectedObject()
    {
        var removed = scene.RemoveSelected();
        if (removed is null)
            throw new InvalidOperationException("Сначала выберите объект сцены.");

        RefreshSceneObjectList();
        UpdateScene($"Удалён объект: {removed.Name}");
    }



    private SceneObject SelectedObject() =>
        scene.SelectedObject ?? throw new InvalidOperationException("Сначала выберите объект сцены.");

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
        var item = scene.SelectedObject;
        item?.Reset();
        UpdateScene(item is null ? "Объект не выбран" : "Преобразования сброшены");
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
        var selected = scene.SelectedObject;
        modelInfo.Text = selected is null ? $"Объектов в сцене: {scene.Objects.Count}" :
            $"Вершин: {selected.Geometry.Positions.Count} · Граней: {selected.Geometry.Faces.Count}";
        centerInfo.Text = "Центр: —";
        if (selected is not null)
        {
            try
            {
                var center = selected.Center;
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
