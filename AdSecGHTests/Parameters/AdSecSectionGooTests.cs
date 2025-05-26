using System;
using System.Collections.Generic;
using System.Drawing;

using AdSecGH.Parameters;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Geometry.Morphs;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecSectionGooTests {
    private readonly AdSecSectionGoo sectionGoo;

    public AdSecSectionGooTests() {
      var section = SampleData.GetCompositeSectionDesign();
      sectionGoo = new AdSecSectionGoo(new AdSecSection(section));
    }

    [Fact]
    public void ShouldNotBeBakeCapable() {
      Assert.False(new AdSecSectionGoo().IsBakeCapable);
    }

    [Fact]
    public void ShouldbeBakeCapable() {
      Assert.True(sectionGoo.IsBakeCapable);
    }

    [Fact]
    public void ShouldBakeIds() {
      using var doc = RhinoDoc.Create(string.Empty);
      var obj_ids = new List<Guid>();
      sectionGoo.BakeGeometry(doc, obj_ids);

      Assert.True(obj_ids.Count > 0, $"Expected at least one object to be created, but got {obj_ids.Count}");
    }

    [Fact]
    public void ShouldBakeWithAttributes() {
      using var doc = RhinoDoc.Create(string.Empty);
      var obj_ids = new List<Guid>();

      var objectAttributes = new ObjectAttributes() {
        Name = "Test",
      };
      sectionGoo.BakeGeometry(doc, objectAttributes, obj_ids);

      Assert.Equal("Test", doc.Objects.FindId(obj_ids[0]).Attributes.Name);
    }

    [Fact]
    public void ShouldDrawForSelected() {
      sectionGoo.UpdateGeometryRepresentation(false);
      var a = sectionGoo.DrawInstructionsList[0].Color;
      var notSelectedColour = Color.FromArgb(a.A, a.R, a.G, a.B);
      sectionGoo.UpdateGeometryRepresentation(true);
      var b = sectionGoo.DrawInstructionsList[0].Color;
      var selectedColour = Color.FromArgb(b.A, b.R, b.G, b.B);

      Assert.NotEqual(notSelectedColour, selectedColour);
    }

    [Fact]
    public void ShouldDrawOnViewPort() {
      sectionGoo.DrawInstructionsList.Clear();
      using var doc = RhinoDoc.Create(string.Empty);
      sectionGoo.DrawViewportWires(ComponentTestHelper.CreatePreviewArgs(doc, Color.White));
      Assert.NotEmpty(sectionGoo.DrawInstructionsList);
    }

    [Fact]
    public void IsValid_ReturnTrue_WhenValidSolidBrep() {
      Assert.True(sectionGoo.IsValid);
    }

    [Fact]
    public void IsValid_ReturnFalse_WhenInvalidSolidBrep() {
      var newGoo = new AdSecSectionGoo();
      Assert.False(newGoo.IsValid);
    }

    [Fact]
    public void Duplicate_ReturnGoo_WhenValidSolidBrep() {
      var result = sectionGoo.DuplicateAdSecSection();
      Assert.NotNull(result);
      Assert.Equal(sectionGoo.IsValid, result.IsValid);
      Assert.Equal(sectionGoo.TypeDescription, result.TypeDescription);
      Assert.Equal(sectionGoo.TypeName, result.TypeName);
      Assert.Equal(sectionGoo.Value.DesignCode, result.Value.DesignCode);
    }

    [Fact]
    public void Duplicate_ReturnNull_WhenInvalidSolidBrep() {
      var newGoo = new AdSecSectionGoo();
      Assert.Null(newGoo.DuplicateAdSecSection());
    }

    [Fact]
    public void CastFrom_ReturnFalse_Always() {
      Assert.False(sectionGoo.CastFrom(null));
      Assert.False(sectionGoo.CastFrom(new AdSecSection()));
    }

    [Fact]
    public void GetBoundingBox_ReturnEmpty_WhenInvalidGoo() {
      Assert.Equal(BoundingBox.Empty, new AdSecSectionGoo().GetBoundingBox(Transform.Identity));
    }

    [Fact]
    public void GetBoundingBox_ReturnNonEmptyBBox_WhenValidGoo() {
      Assert.NotEqual(BoundingBox.Empty, sectionGoo.GetBoundingBox(Transform.Identity));
    }

    [Fact]
    public void Morph_ReturnNull_Always() {
      Assert.Null(sectionGoo.Morph(null));
      Assert.Null(sectionGoo.Morph(new BendSpaceMorph(Point3d.Origin, Point3d.Origin, Point3d.Origin, true, true)));
    }

    [Fact]
    public void Transform_ReturnNull_Always() {
      Assert.Null(sectionGoo.Transform(Transform.Identity));
      Assert.Null(sectionGoo.Transform(new Transform()));
    }

    [Fact]
    public void ToString_ReturnValidText() {
      Assert.Contains(sectionGoo.TypeName, sectionGoo.ToString());
      Assert.Contains("Invalid", new AdSecSectionGoo().ToString());
    }

  }
}
