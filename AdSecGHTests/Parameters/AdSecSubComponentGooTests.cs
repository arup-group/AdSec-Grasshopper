using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.UI;

using AdSecGHTests.Helpers;

using Oasys.AdSec;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;
using Oasys.Taxonomy.Profiles;

using OasysUnits;
using OasysUnits.Units;

using Rhino;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Geometry.Morphs;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecSubComponentGooTests {
    private readonly AdSecSubComponentGoo subComponentGoo;

    public AdSecSubComponentGooTests() {
      var point = IPoint.Create(new Length(1, LengthUnit.Meter), new Length(2, LengthUnit.Meter));
      var subComponent = ISubComponent.Create(GetAdSecSectionGoo().Value.Section, point);
      subComponentGoo = new AdSecSubComponentGoo(subComponent, Plane.WorldXY, new AdSecDesignCode().DesignCode, "test",
        string.Empty);
    }

    [Fact]
    public void CanCreateFromSubComponent() {
      var point = IPoint.Create(new Length(1, LengthUnit.Meter), new Length(2, LengthUnit.Meter));
      var newSubComponentGoo = new AdSecSubComponentGoo(new SubComponent() {
        ISubComponent = ISubComponent.Create(GetAdSecSectionGoo().Value.Section, point),
        SectionDesign = new SectionDesign() {
          LocalPlane = OasysPlane.PlaneXY,
          MaterialName = "TEST",
          CodeName = "TEST",
          DesignCode = new DesignCode(),
          Section = GetAdSecSectionGoo().Value.Section,
        },
      });

      Assert.True(newSubComponentGoo.IsValid);
    }

    [Fact]
    public void CastFromReturnFalseAlways() {
      Assert.False(subComponentGoo.CastFrom(null));
      Assert.False(subComponentGoo.CastFrom(subComponentGoo));
      Assert.False(subComponentGoo.CastFrom(0));
    }

    [Fact]
    public void DuplicateGeometryReturnsValidObject() {
      var result = subComponentGoo.DuplicateGeometry();
      Assert.NotNull(result);
      Assert.IsType<AdSecSubComponentGoo>(result);
      Assert.Equal(subComponentGoo.IsValid, result.IsValid);
      Assert.Equal(subComponentGoo.IsGeometryLoaded, result.IsGeometryLoaded);
      Assert.Equal(subComponentGoo.TypeDescription, result.TypeDescription);
      Assert.Equal(subComponentGoo.TypeName, result.TypeName);
    }

    [Fact]
    public void GetBoundingBoxReturnsValidBoundingBoxForDefaultTransform() {
      var boundingBox = subComponentGoo.GetBoundingBox(new Transform());
      Assert.NotEqual(BoundingBox.Empty, boundingBox);
    }

    [Fact]
    public void GetBoundingBoxReturnsValidBoundingBox() {
      var boundingBox = subComponentGoo.GetBoundingBox(new Transform(2));
      Assert.NotEqual(BoundingBox.Empty, boundingBox);
    }

    [Fact]
    public void MorphReturnsNullAlways() {
      Assert.Null(subComponentGoo.Morph(null));
      Assert.Null(subComponentGoo.Morph(new BendSpaceMorph(Point3d.Unset, Point3d.Unset, Point3d.Unset, false, false)));
    }

    [Fact]
    public void TransformRturnsNullAlways() {
      Assert.Null(subComponentGoo.Transform(new Transform()));
      Assert.Null(subComponentGoo.Transform(new Transform(2)));
    }

    [Fact]
    public void ToStringContainsInfoAboutOffset() {
      Assert.Contains("Offset", subComponentGoo.ToString());
    }

    [Fact]
    public void ShouldDrawOnViewPortWires() {
      using var doc = RhinoDoc.Create(string.Empty);
      var ghPreviewWireArgs = ComponentTestHelper.CreatePreviewArgs(doc, Color.White);

      Assert.Empty(subComponentGoo.DrawInstructionsList);

      subComponentGoo.DrawViewportWires(ghPreviewWireArgs);

      Assert.NotEmpty(subComponentGoo.DrawInstructionsList);
      Assert.Single(subComponentGoo.DrawInstructionsList);
      Assert.Equal(Colour.OasysYellow, subComponentGoo.DrawInstructionsList[0].Color);
    }

    [Fact]
    public void ShouldDrawOnViewportMesh() {
      using var doc = RhinoDoc.Create(string.Empty);
      var ghPreviewWireArgs
        = ComponentTestHelper.CreatePreviewMeshArgs(doc, new DisplayMaterial(Color.White)); // color doesn;t matter here

      Assert.Empty(subComponentGoo.DrawInstructionsList);

      subComponentGoo.DrawViewportMeshes(ghPreviewWireArgs);

      Assert.NotEmpty(subComponentGoo.DrawInstructionsList);
      Assert.Single(subComponentGoo.DrawInstructionsList);
    }

    private static AdSecSectionGoo GetAdSecSectionGoo() {
      var length = new Length(1, LengthUnit.Meter);
      var profile = AdSecProfiles.CreateProfile(new AngleProfile(length, new Flange(length, length),
        new WebConstant(length)));
      var material = Steel.ASTM.A242_46;
      var section = ISection.Create(profile, material);
      var designCode = new AdSecDesignCode().DesignCode;
      return new AdSecSectionGoo(new AdSecSection(section, designCode, "", "", Plane.WorldXY));
    }
  }
}
