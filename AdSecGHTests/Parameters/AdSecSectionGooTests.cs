using System;
using System.Collections.Generic;
using System.Drawing;

using AdSecGH.Parameters;

using Rhino;
using Rhino.DocObjects;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecSectionGooTests {
    private readonly AdSecSectionGoo sectionGoo;

    public AdSecSectionGooTests() {
      var section = SampleData.GetSectionDesign();
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
      var doc = RhinoDoc.Create(string.Empty);
      var obj_ids = new List<Guid>();
      sectionGoo.BakeGeometry(doc, obj_ids);

      Assert.True(obj_ids.Count > 0, $"Expected at least one object to be created, but got {obj_ids.Count}");
      doc.Dispose();
    }

    [Fact]
    public void ShouldBakeWithAttributes() {
      var doc = RhinoDoc.Create(string.Empty);
      var obj_ids = new List<Guid>();

      var objectAttributes = new ObjectAttributes() {
        Name = "Test",
      };
      sectionGoo.BakeGeometry(doc, objectAttributes, obj_ids);

      Assert.Equal("Test", doc.Objects.FindId(obj_ids[0]).Attributes.Name);
      doc.Dispose();
    }

    [Fact]
    public void ShouldDrawForSelected() {
      sectionGoo.UpdateGeometryRepresentation(false);
      var a = sectionGoo._drawInstructions[0].Color;
      var notSelectedColour = Color.FromArgb(a.A, a.R, a.G, a.B);
      sectionGoo.UpdateGeometryRepresentation(true);
      var b = sectionGoo._drawInstructions[0].Color;
      var selectedColour = Color.FromArgb(b.A, b.R, b.G, b.B);

      Assert.NotEqual(notSelectedColour, selectedColour);
    }
  }
}
