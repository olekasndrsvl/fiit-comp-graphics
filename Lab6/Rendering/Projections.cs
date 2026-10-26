using Lab6.Core.Mathematics;
using MathNet.Numerics.LinearAlgebra;

namespace Lab6.Rendering;

public static class Projections
{
    // TODO [3]: собрать матрицу перспективы с учётом отношения сторон холста.
    // Согласовать её с направлением взгляда и последующим делением на w.
    public static Matrix<double> Perspective(double aspect) =>
        throw new NotImplementedException("TODO [3]: перспективная проекция.");

    // TODO [3]: составить повороты и параллельную проекцию (слайд 39).
    public static Matrix<double> Axonometric(double aspect) =>
        throw new NotImplementedException("TODO [3]: аксонометрическая проекция.");
}
