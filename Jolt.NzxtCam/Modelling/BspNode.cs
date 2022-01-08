using System.Linq;
using System.Numerics;
using static Jolt.NzxtCam.MathF;
using static System.Numerics.Plane;
namespace Jolt.NzxtCam;

class BspTree
{
    private BspNode? rootNode;

    public void Add(Model4 model) {
        foreach (var face in model.Faces) {
            Add(model, face);
        }
    }

    public void Add(Model4 model, Face face)
        => BspNode.Add(ref rootNode, model, face);

    public IEnumerable<(Model4 Model, Face Face)> Iterate() 
        => rootNode?.Iterate() ?? Enumerable.Empty<(Model4, Face)>();
}

class BspNode {
    public Plane Plane { get; private set; }
    public List<(Model4, Face)> Faces { get; } = new();
    private BspNode? negNode;
    private BspNode? posNode;

    public BspNode(Model4 model, Face face, Plane? plane) {
        Faces.Add((model, face));
        Plane = plane ?? model.CalculatePlane(face);
    }

    internal static void Add(ref BspNode? node, Model4 model, Face? face, Plane? plane = null) {
        if (face == null) {
            // Do nothing.
        } else if (node == null) {
            node = new BspNode(model, face, plane);
        } else {
            node.Add(model, face, plane);
        }
    }

    public void Add(Model4 model, Face face, Plane? plane) {
        var (negFace, zeroFace, posFace) = FaceHelper.Split(model.Positions, face, Plane);

        Add(ref negNode, model, negFace, plane);
        if (zeroFace != null) {
            Faces.Add((model, zeroFace));
        }
        Add(ref posNode, model, posFace, plane);
    }

    public IEnumerable<(Model4, Face)> Iterate() {
        var sign = Sign(DotCoordinate(Plane, Vector3.Zero));
        var first = (sign < 0 ? negNode : posNode)?.Iterate() ?? Enumerable.Empty<(Model4, Face)>();
        var last = (sign < 0 ? posNode : negNode)?.Iterate() ?? Enumerable.Empty<(Model4, Face)>();
        return first.Concat(Faces).Concat(last);
    }
}