using System;
using System.Drawing;

using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Parameters;

using AdSecGHTests.Helpers;

using Grasshopper;
using Grasshopper.Kernel;

using OasysUnits;
using OasysUnits.Units;

using Rhino;
using Rhino.Geometry;

using Xunit;
namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecNeutralAxisGooTests {
    private readonly AdSecNeutralAxisGoo _neutralAxisGoo;
    private readonly NeutralAxis _neutralAxis;
    private static SectionSolution Solution { get; set; } = null;
    public AdSecNeutralAxisGooTests() {
      if (Solution == null) {
        Solution = new SolutionBuilder().Build();
      }
      _neutralAxis = new NeutralAxis() { Offset = new Length(100, LengthUnit.Millimeter), Angle = Math.PI / 4, Solution = Solution };

      _neutralAxisGoo = new AdSecNeutralAxisGoo(_neutralAxis);
    }

    [Fact]
    public void TypeName_ShouldReturnLine() {
      Assert.Equal("Neutral Axis", _neutralAxisGoo.TypeName);
    }

    [Fact]
    public void TypeDescription_ShouldReturnAdSecLineParameter() {
      Assert.Equal("AdSec Neutral Axis Parameter", _neutralAxisGoo.TypeDescription);
    }

    [Fact]
    public void Constructor_ShouldCreateAxisLine() {
      Assert.True(_neutralAxisGoo.AxisLine.Length > 0);
    }

    [Fact]
    public void ClippingBox_ShouldEqualBoundingbox() {
      Assert.Equal(_neutralAxisGoo.Boundingbox, _neutralAxisGoo.ClippingBox);
    }

    [Fact]
    public void Transform_WhenGivenValidTransform_ShouldReturnNewTransformedInstance() {
      var transform = Transform.Translation(new Vector3d(10, 10, 10));
      var transformed = _neutralAxisGoo.Transform(transform);
      var maxPoint = _neutralAxisGoo.Boundingbox.Max;
      var minPoint = _neutralAxisGoo.Boundingbox.Min;
      var expectedMaxPoint = new Point3d(maxPoint.X + 10, maxPoint.Y + 10, maxPoint.Z + 10);
      var expectedMinPoint = new Point3d(minPoint.X + 10, minPoint.Y + 10, minPoint.Z + 10);
      var expectedBoundingBox = new BoundingBox(expectedMinPoint, expectedMaxPoint);
      bool areEqual = AdSecUtility.IsBoundingBoxEqual(expectedBoundingBox, transformed.Boundingbox);
      Assert.True(areEqual);
      Assert.NotNull(transformed);
      Assert.NotSame(transformed, _neutralAxisGoo);
      Assert.IsType<AdSecNeutralAxisGoo>(transformed);
    }

    [Fact]
    public void DuplicateGeometry_ShouldCreateNewInstance() {
      var duplicate = _neutralAxisGoo.DuplicateGeometry();
      Assert.NotNull(duplicate);
      Assert.NotSame(duplicate, _neutralAxisGoo);
      Assert.IsType<AdSecNeutralAxisGoo>(duplicate);
    }

    [Fact]
    public void Morph_ShouldThrowNotImplementedException() {
      Assert.Throws<NotImplementedException>(() => _neutralAxisGoo.Morph(null));
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString() {
      var result = _neutralAxisGoo.ToString();
      Assert.StartsWith("Line(Length = ", result);
      Assert.EndsWith(")", result);
    }

    [Fact]
    public void GetBoundingBox_WhenGivenIdentityTransform_ShouldReturnAxisLineBoundingBox() {
      var transform = Transform.Identity;
      var boundingBox = _neutralAxisGoo.GetBoundingBox(transform);
      Assert.Equal(_neutralAxisGoo.AxisLine.BoundingBox, boundingBox);
    }

    [Fact]
    public void ShouldDrawLoadAxisWhenNotSelected() {
      using var doc = RhinoDoc.Create(string.Empty);
      var ghPreviewWireArgs = ComponentTestHelper.CreatePreviewArgs(doc, Instances.Settings.GetValue("DefaultPreviewColour", Color.White));
      _neutralAxisGoo.DrawViewportWires(ghPreviewWireArgs);
      Assert.NotEmpty(_neutralAxisGoo.DrawInstructions);
    }

    [Fact]
    public void ShouldDrawLoadAxisWhenSelected() {
      using var doc = RhinoDoc.Create(string.Empty);
      var ghPreviewWireArgs = ComponentTestHelper.CreatePreviewArgs(doc, Color.Turquoise);
      _neutralAxisGoo.DrawViewportWires(ghPreviewWireArgs);
      Assert.NotEmpty(_neutralAxisGoo.DrawInstructions);
    }

    [Fact]
    public void ShouldDrawFailureLoadAxisWhenNotSelected() {
      using var doc = RhinoDoc.Create(string.Empty);
      var ghPreviewWireArgs = ComponentTestHelper.CreatePreviewArgs(doc, Instances.Settings.GetValue("DefaultPreviewColour", Color.White));
      _neutralAxisGoo.Value.IsFailureNeutralAxis = true;
      _neutralAxisGoo.DrawViewportWires(ghPreviewWireArgs);
      Assert.NotEmpty(_neutralAxisGoo.DrawInstructions);
    }

    [Fact]
    public void ShouldDrawFailureLoadAxisWhenSelected() {
      using var doc = RhinoDoc.Create(string.Empty);
      var ghPreviewWireArgs = ComponentTestHelper.CreatePreviewArgs(doc, Color.Turquoise);
      _neutralAxisGoo.Value.IsFailureNeutralAxis = true;
      _neutralAxisGoo.DrawViewportWires(ghPreviewWireArgs);
      Assert.NotEmpty(_neutralAxisGoo.DrawInstructions);
    }
  }
}
