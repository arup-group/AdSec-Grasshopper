using System;
using System.Collections.Generic;

using AdSecGH.Parameters;

using Rhino;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecSectionGooTests {
    [Fact]
    public void IsBakeCapable_WithNullValue_ReturnsFalse() {
      Assert.False(new AdSecSectionGoo().IsBakeCapable);
    }

    [Fact]
    public void IsBakeCapable_WithValidValue_ReturnsTrue() {
      var section = SampleData.GetSectionDesign();
      var sectionGoo = new AdSecSectionGoo(new AdSecSection(section));
      Assert.True(sectionGoo.IsBakeCapable);
    }

    [Fact]
    public void BakeGeometry_WithValidSection_AddsMultipleObjectIds() {
      var section = SampleData.GetSectionDesign();
      var sectionGoo = new AdSecSectionGoo(new AdSecSection(section));
      var doc = RhinoDoc.Create(string.Empty);
      var obj_ids = new List<Guid>();

      sectionGoo.BakeGeometry(doc, obj_ids);

      Assert.True(obj_ids.Count > 0, $"Expected at least one object to be created, but got {obj_ids.Count}");
      doc.Dispose();
    }
  }
}
