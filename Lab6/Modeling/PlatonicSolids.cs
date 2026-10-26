using Lab6.Core.Geometry;

namespace Lab6.Modeling;

public static class PlatonicSolids
{
    public static Polyhedron Create(int index)
    {
        var model = index switch
        {
            0 => CreateTetrahedron(),
            1 => CreateHexahedron(),
            2 => CreateOctahedron(),
            3 => CreateIcosahedron(),
            4 => CreateDodecahedron(),
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
        model.RebuildEdges();
        return model;
    }

    // TODO [1]: задать 4 вершины и 4 треугольные грани.
    public static Polyhedron CreateTetrahedron() =>
        throw new NotImplementedException("TODO [1]: построить тетраэдр.");

    // TODO [1]: задать 8 вершин и 6 квадратных граней.
    public static Polyhedron CreateHexahedron() =>
        throw new NotImplementedException("TODO [1]: построить гексаэдр.");

    // TODO [1]: задать 6 вершин и 8 треугольных граней.
    public static Polyhedron CreateOctahedron() =>
        throw new NotImplementedException("TODO [1]: построить октаэдр.");

    // TODO [1]: бонус — 12 вершин и 20 треугольных граней, все рёбра равны.
    public static Polyhedron CreateIcosahedron() =>
        throw new NotImplementedException("TODO [1]: построить икосаэдр.");

    // TODO [1]: бонус — 20 вершин и 12 пятиугольных граней, все рёбра равны.
    public static Polyhedron CreateDodecahedron() =>
        throw new NotImplementedException("TODO [1]: построить додекаэдр.");
}
