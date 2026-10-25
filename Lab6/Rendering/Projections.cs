using Lab6.Core.Mathematics;

namespace Lab6.Rendering;

public static class Projections
{
    // TODO [3]: собрать матрицу перспективы с учётом отношения сторон холста.
    // Согласовать её с направлением взгляда и последующим делением на w.
    public static Matrix4 Perspective(double aspect) =>
        throw new NotImplementedException("TODO [3]: перспективная проекция.");

    // TODO [3]: составить повороты и параллельную проекцию (слайд 39).
    public static Matrix4 Axonometric(double aspect) =>
        throw new NotImplementedException("TODO [3]: аксонометрическая проекция.");
}
