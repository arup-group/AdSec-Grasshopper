using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Parameters;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  [SuppressMessage("Major Bug", "S4143:Collection elements should not be replaced unconditionally",
    Justification = "<Pending>")]
  public class AdSecFailureSurfaceGooTests {
    private readonly AdSecFailureSurfaceGoo _testGoo;
    private static SectionSolution Solution { get; set; }

    public AdSecFailureSurfaceGooTests() {
      Solution ??= new SolutionBuilder().Build();
      var loadSurface = new LoadSurfaceDesign() {
        LoadSurface = Solution.Strength.GetFailureSurface(),
        LocalPlane = OasysPlane.PlaneXY
      };
      _testGoo = new AdSecFailureSurfaceGoo(loadSurface);
    }

    [Fact]
    public void GetBoundingBox_ReturnsEmpty_WhenValueIsNull() {
      _testGoo.Value = null;
      var result = _testGoo.GetBoundingBox(Transform.Identity);

      Assert.Equal(BoundingBox.Empty, result);
    }

    [Fact]
    public void GetBoundingBox_ReturnsTransformedBoundingBox_WhenValueExists() {
      var mesh = new Mesh();
      mesh.Vertices.Add(0, 0, 0);
      mesh.Vertices.Add(1, 0, 0);
      mesh.Vertices.Add(0, 1, 0);
      mesh.Faces.AddFace(0, 1, 2);

      var scale = Transform.Scale(new Point3d(0, 0, 0), 2.0);

      // Test native Rhino method with unsafe calls
      var bbox = mesh.GetBoundingBox(scale);

      Assert.Equal(new Point3d(0, 0, 0), bbox.Min);
      Assert.Equal(new Point3d(2, 2, 0), bbox.Max);
    }

    [Fact]
    public void CastFrom_ReturnsFalse_Always() {
      var objects = new List<object> {
        null,
        int.MaxValue,
        new object(),
      };

      foreach (object obj in objects) {
        Assert.False(_testGoo.CastFrom(obj));
      }
    }

    [Fact]
    public void DuplicateGeometry_ReturnsNull_WhenValueIsInvalid() {
      _testGoo.Value = new Mesh();
      _testGoo.Value.Faces.Clear();

      var result = _testGoo.DuplicateGeometry();

      Assert.Null(result);
    }

    [Fact]
    public void DuplicateGeometry_ReturnsNewAdSecFailureSurfaceGooInstance_WhenValueIsValid() {
      var validMesh = new Mesh();
      validMesh.Vertices.Add(0, 0, 0);
      validMesh.Vertices.Add(1, 1, 0);
      validMesh.Vertices.Add(1, 0, 1);
      validMesh.Faces.AddFace(0, 1, 2);

      _testGoo.Value = validMesh;
      var result = _testGoo.DuplicateGeometry();

      Assert.NotNull(result);
      Assert.IsType<AdSecFailureSurfaceGoo>(result);
      var newGoo = (AdSecFailureSurfaceGoo)result;
      Assert.Equal(_testGoo.FailureSurface, newGoo.FailureSurface);
    }

    [Fact]
    public void Morph_ReturnsNull_WhenValueIsNull() {
      _testGoo.Value = null;
      var result = _testGoo.Morph(new TestSpaceMorph());
      Assert.Null(result);
    }

    [Fact]
    public void Morph_ReturnsNull_WhenValueIsInvalid() {
      var invalidMesh = new Mesh();
      invalidMesh.Faces.Clear();
      _testGoo.Value = invalidMesh;
      var result = _testGoo.Morph(new TestSpaceMorph());
      Assert.Null(result);
    }

    [Fact]
    public void Morph_ValidValue_ReturnsModifiedAdSecFailureSurfaceGoo() {
      var xmorph = new TestSpaceMorph();
      var result = _testGoo.Morph(xmorph);

      Assert.NotNull(result);
      Assert.IsType<AdSecFailureSurfaceGoo>(result);
      var newGoo = (AdSecFailureSurfaceGoo)result;

      Assert.Equal(_testGoo.FailureSurface, newGoo.FailureSurface);

      var originalVertex = _testGoo.Value.Vertices[0];
      var newVertex = newGoo.Value.Vertices[0];
      Assert.Equal(originalVertex.X + 1, newVertex.X);
    }

    [Fact]
    public void Transform_ReturnsNull_WhenValueIsNull() {
      _testGoo.Value = null;
      var transform = Transform.Translation(1, 2, 3);
      var result = _testGoo.Transform(transform);
      Assert.Null(result);
    }

    [Fact]
    public void Transform_ReturnsNull_WhenValueIsInvalid() {
      var mesh = new Mesh();
      mesh.Vertices.Add(0, 0, 0);
      mesh.Vertices.Add(0, 0, 0);
      mesh.Vertices.Add(0, 0, 0);
      _testGoo.Value = mesh;

      var transform = Transform.Translation(1, 0, 0);
      var result = _testGoo.Transform(transform);
      Assert.Null(result);
    }

    [Fact]
    public void Transform_ReturnsTransformedAdSecFailureSurfaceGoo_WhenValueIsValid() {
      var transform = Transform.Translation(1, 0, 0);
      var result = _testGoo.Transform(transform);

      Assert.NotNull(result);
      Assert.IsType<AdSecFailureSurfaceGoo>(result);

      var newGoo = (AdSecFailureSurfaceGoo)result;
      var originalVertex = _testGoo.Value.Vertices[0];
      var transformedVertex = newGoo.Value.Vertices[0];
      Assert.Equal(_testGoo.FailureSurface, newGoo.FailureSurface);
      Assert.Equal(originalVertex.X + 1, transformedVertex.X, 5);
      Assert.Equal(originalVertex.Y, transformedVertex.Y, 5);
      Assert.Equal(originalVertex.Z, transformedVertex.Z, 5);
    }

    private class TestSpaceMorph : SpaceMorph {
      public override Point3d MorphPoint(Point3d point) {
        return new Point3d(point.X + 1, point.Y, point.Z);
      }
    }
  }
}
