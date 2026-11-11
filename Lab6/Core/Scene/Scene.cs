using Lab6.Core.Geometry;

namespace Lab6.Core.Scene;

public sealed class Scene
{
    private readonly List<SceneObject> objects = [];

    public IReadOnlyList<SceneObject> Objects => objects;
    public SceneObject? SelectedObject { get; private set; }

    public SceneObject Add(string name, Polyhedron geometry, string? sourcePath = null)
    {
        var item = new SceneObject(UniqueName(name), geometry, sourcePath);
        objects.Add(item);
        SelectedObject = item;
        return item;
    }

    public void Select(SceneObject item)
    {
        if (!objects.Contains(item))
            throw new ArgumentException("Объект не принадлежит сцене.", nameof(item));
        SelectedObject = item;
    }

    public SceneObject? RemoveSelected()
    {
        if (SelectedObject is null)
            return null;

        var index = objects.IndexOf(SelectedObject);
        var removed = SelectedObject;
        objects.RemoveAt(index);
        SelectedObject = objects.Count == 0
            ? null
            : objects[Math.Min(index, objects.Count - 1)];
        return removed;
    }

    private string UniqueName(string name)
    {
        var baseName = string.IsNullOrWhiteSpace(name) ? "Модель" : name.Trim();
        if (objects.All(item => item.Name != baseName))
            return baseName;

        var number = 2;
        while (objects.Any(item => item.Name == $"{baseName} ({number})"))
            number++;
        return $"{baseName} ({number})";
    }
}
