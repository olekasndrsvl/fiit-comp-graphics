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

    public static Polyhedron CreateTetrahedron()
    {
        var positions = new List<Point3D>
        {
            new(1, 1, 1),
            new(1, -1, -1),
            new(-1, 1, -1),
            new(-1, -1, 1)
        };

        var faces = new List<Polygon>
        {
            new(new[] { new FaceVertex(0), new FaceVertex(1), new FaceVertex(2) }),
            new(new[] { new FaceVertex(0), new FaceVertex(3), new FaceVertex(1) }),
            new(new[] { new FaceVertex(0), new FaceVertex(2), new FaceVertex(3) }),
            new(new[] { new FaceVertex(1), new FaceVertex(3), new FaceVertex(2) })
        };

        return new Polyhedron(positions, faces);
    }

    public static Polyhedron CreateHexahedron()
    {
        var positions = new List<Point3D>
        {
            new(-1, -1, -1),
            new(1, -1, -1),
            new(1, 1, -1),
            new(-1, 1, -1),
            new(-1, -1, 1),
            new(1, -1, 1),
            new(1, 1, 1),
            new(-1, 1, 1)
        };

        var faces = new List<Polygon>
        {
            new(new[] { new FaceVertex(0), new FaceVertex(3), new FaceVertex(2), new FaceVertex(1) }),
            new(new[] { new FaceVertex(4), new FaceVertex(5), new FaceVertex(6), new FaceVertex(7) }),
            new(new[] { new FaceVertex(0), new FaceVertex(1), new FaceVertex(5), new FaceVertex(4) }),
            new(new[] { new FaceVertex(2), new FaceVertex(3), new FaceVertex(7), new FaceVertex(6) }),
            new(new[] { new FaceVertex(0), new FaceVertex(4), new FaceVertex(7), new FaceVertex(3) }),
            new(new[] { new FaceVertex(1), new FaceVertex(2), new FaceVertex(6), new FaceVertex(5) })
        };

        return new Polyhedron(positions, faces);
    }

    public static Polyhedron CreateOctahedron()
    {
        var positions = new List<Point3D>
        {
            new(1, 0, 0),
            new(-1, 0, 0),
            new(0, 1, 0),
            new(0, -1, 0),
            new(0, 0, 1),
            new(0, 0, -1)
        };

        var faces = new List<Polygon>
        {
            new(new[] { new FaceVertex(0), new FaceVertex(2), new FaceVertex(4) }),
            new(new[] { new FaceVertex(2), new FaceVertex(1), new FaceVertex(4) }),
            new(new[] { new FaceVertex(1), new FaceVertex(3), new FaceVertex(4) }),
            new(new[] { new FaceVertex(3), new FaceVertex(0), new FaceVertex(4) }),
            new(new[] { new FaceVertex(0), new FaceVertex(4), new FaceVertex(2) }) 
        };

        faces.Clear();
        faces.Add(new Polygon(new[] { new FaceVertex(0), new FaceVertex(2), new FaceVertex(4) }));
        faces.Add(new Polygon(new[] { new FaceVertex(2), new FaceVertex(1), new FaceVertex(4) }));
        faces.Add(new Polygon(new[] { new FaceVertex(1), new FaceVertex(3), new FaceVertex(4) }));
        faces.Add(new Polygon(new[] { new FaceVertex(3), new FaceVertex(0), new FaceVertex(4) }));
        faces.Add(new Polygon(new[] { new FaceVertex(2), new FaceVertex(0), new FaceVertex(5) }));
        faces.Add(new Polygon(new[] { new FaceVertex(1), new FaceVertex(2), new FaceVertex(5) }));
        faces.Add(new Polygon(new[] { new FaceVertex(3), new FaceVertex(1), new FaceVertex(5) }));
        faces.Add(new Polygon(new[] { new FaceVertex(0), new FaceVertex(3), new FaceVertex(5) }));

        return new Polyhedron(positions, faces);
    }

    public static Polyhedron CreateIcosahedron()
    {
        var phi = (1.0 + Math.Sqrt(5.0)) / 2.0;
        var positions = new List<Point3D>
        {
            new(-1, phi, 0),
            new(1, phi, 0),
            new(-1, -phi, 0),
            new(1, -phi, 0),
            new(0, -1, phi),
            new(0, 1, phi),
            new(0, -1, -phi),
            new(0, 1, -phi),
            new(phi, 0, -1),
            new(phi, 0, 1),
            new(-phi, 0, -1),
            new(-phi, 0, 1)
        };

        var faces = new List<Polygon>
        {
            new(new[] { new FaceVertex(0), new FaceVertex(11), new FaceVertex(5) }),
            new(new[] { new FaceVertex(0), new FaceVertex(5), new FaceVertex(1) }),
            new(new[] { new FaceVertex(0), new FaceVertex(1), new FaceVertex(7) }),
            new(new[] { new FaceVertex(0), new FaceVertex(7), new FaceVertex(10) }),
            new(new[] { new FaceVertex(0), new FaceVertex(10), new FaceVertex(11) }),
            new(new[] { new FaceVertex(1), new FaceVertex(5), new FaceVertex(9) }),
            new(new[] { new FaceVertex(5), new FaceVertex(11), new FaceVertex(4) }),
            new(new[] { new FaceVertex(11), new FaceVertex(10), new FaceVertex(2) }),
            new(new[] { new FaceVertex(10), new FaceVertex(7), new FaceVertex(6) }),
            new(new[] { new FaceVertex(7), new FaceVertex(1), new FaceVertex(8) }),
            new(new[] { new FaceVertex(3), new FaceVertex(9), new FaceVertex(4) }),
            new(new[] { new FaceVertex(3), new FaceVertex(4), new FaceVertex(2) }),
            new(new[] { new FaceVertex(3), new FaceVertex(2), new FaceVertex(6) }),
            new(new[] { new FaceVertex(3), new FaceVertex(6), new FaceVertex(8) }),
            new(new[] { new FaceVertex(3), new FaceVertex(8), new FaceVertex(9) }),
            new(new[] { new FaceVertex(4), new FaceVertex(9), new FaceVertex(5) }),
            new(new[] { new FaceVertex(2), new FaceVertex(4), new FaceVertex(11) }),
            new(new[] { new FaceVertex(6), new FaceVertex(2), new FaceVertex(10) }),
            new(new[] { new FaceVertex(8), new FaceVertex(6), new FaceVertex(7) }),
            new(new[] { new FaceVertex(9), new FaceVertex(8), new FaceVertex(1) })
        };

        return new Polyhedron(positions, faces);
    }

    public static Polyhedron CreateDodecahedron()
    {
        var phi = (1.0 + Math.Sqrt(5.0)) / 2.0;
        var invPhi = 1.0 / phi;

        var positions = new List<Point3D>
    {
        new( 1,  1,  1), 
        new( 1,  1, -1), 
        new( 1, -1,  1), 
        new( 1, -1, -1), 
        new(-1,  1,  1), 
        new(-1,  1, -1), 
        new(-1, -1,  1), 
        new(-1, -1, -1), 

        new( 0,  invPhi,  phi),
        new( 0, -invPhi,  phi),
        new( 0,  invPhi, -phi),
        new( 0, -invPhi, -phi),

        new( invPhi,  phi,  0),
        new(-invPhi,  phi,  0),
        new( invPhi, -phi,  0),
        new(-invPhi, -phi,  0),

        new( phi,  0,  invPhi),
        new( phi,  0, -invPhi),
        new(-phi,  0,  invPhi),
        new(-phi,  0, -invPhi) 
    };

        var faces = new List<Polygon>
    {
        new(new[]
        {
            new FaceVertex(1),
            new FaceVertex(12),
            new FaceVertex(0),
            new FaceVertex(16),
            new FaceVertex(17)
        }),

        new(new[]
        {
            new FaceVertex(2),
            new FaceVertex(16),
            new FaceVertex(0),
            new FaceVertex(8),
            new FaceVertex(9)
        }),

        new(new[]
        {
            new FaceVertex(4),
            new FaceVertex(8),
            new FaceVertex(0),
            new FaceVertex(12),
            new FaceVertex(13)
        }),

        new(new[]
        {
            new FaceVertex(11),
            new FaceVertex(10),
            new FaceVertex(1),
            new FaceVertex(17),
            new FaceVertex(3)
        }),

        new(new[]
        {
            new FaceVertex(13),
            new FaceVertex(12),
            new FaceVertex(1),
            new FaceVertex(10),
            new FaceVertex(5)
        }),

        new(new[]
        {
            new FaceVertex(17),
            new FaceVertex(16),
            new FaceVertex(2),
            new FaceVertex(14),
            new FaceVertex(3)
        }),

        new(new[]
        {
            new FaceVertex(15),
            new FaceVertex(14),
            new FaceVertex(2),
            new FaceVertex(9),
            new FaceVertex(6)
        }),

        new(new[]
        {
            new FaceVertex(7),
            new FaceVertex(11),
            new FaceVertex(3),
            new FaceVertex(14),
            new FaceVertex(15)
        }),

        new(new[]
        {
            new FaceVertex(19),
            new FaceVertex(18),
            new FaceVertex(4),
            new FaceVertex(13),
            new FaceVertex(5)
        }),

        new(new[]
        {
            new FaceVertex(9),
            new FaceVertex(8),
            new FaceVertex(4),
            new FaceVertex(18),
            new FaceVertex(6)
        }),

        new(new[]
        {
            new FaceVertex(7),
            new FaceVertex(19),
            new FaceVertex(5),
            new FaceVertex(10),
            new FaceVertex(11)
        }),

        new(new[]
        {
            new FaceVertex(7),
            new FaceVertex(15),
            new FaceVertex(6),
            new FaceVertex(18),
            new FaceVertex(19)
        })
    };

        return new Polyhedron(positions, faces);
    }
    
}