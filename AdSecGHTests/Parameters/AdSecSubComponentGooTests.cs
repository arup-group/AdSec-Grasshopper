using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
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
    private SubComponent _subComponent;

    public AdSecSubComponentGooTests() {
      var point = IPoint.Create(new Length(1, LengthUnit.Meter), new Length(2, LengthUnit.Meter));
      var section = new SectionBuilder().WithHeight(1).WithWidth(1).Build();
      _subComponent = new SubComponent() {
        ISubComponent = ISubComponent.Create(section, point),
        SectionDesign = new SectionDesign() {
          Section = section,
          DesignCode = new DesignCode() {
            IDesignCode = IS456.Edition_2000,
          },
          LocalPlane = OasysPlane.PlaneXY,
        }
      };
      subComponentGoo = new AdSecSubComponentGoo(_subComponent);
    }

    [Fact]
    public void BoundingBoxReturnsEmptyWhenValueIsNull() {
      subComponentGoo.Value = null;

      Assert.Equal(BoundingBox.Empty, subComponentGoo.Boundingbox);
      Assert.Equal(subComponentGoo.ClippingBox, subComponentGoo.Boundingbox);
    }

    [Fact]
    public void ShouldProduceAxisWhenPlaneIsNotXYZ() {
      var nonXYZPlane = new OasysPlane() { Origin = OasysPoint.Zero, XAxis = OasysPoint.ZAxis, YAxis = OasysPoint.YAxis };
      _subComponent.SectionDesign.LocalPlane = nonXYZPlane;
      var componentGoo = new AdSecSubComponentGoo(_subComponent);
      Assert.True(componentGoo.previewXaxis.IsValid);
    }

    [Fact]
    public void BoundingBoxReturnsNotEmptyWhenValueIsValid() {
      Assert.NotEqual(BoundingBox.Empty, subComponentGoo.Boundingbox);
      Assert.Equal(subComponentGoo.ClippingBox, subComponentGoo.Boundingbox);
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
    public void TransformReturnsNullAlways() {
      Assert.Null(subComponentGoo.Transform(new Transform()));
      Assert.Null(subComponentGoo.Transform(new Transform(2)));
    }

    [Fact]
    public void ToStringContainsInfoAboutOffset() {
      Assert.Contains("Offset", subComponentGoo.ToString());
    }

    [Fact]
    public void ShouldPrepareInstructionsForDrawingWires() {
      Assert.Empty(subComponentGoo.DrawInstructionsList);

      subComponentGoo.TryPrepareDrawWiresInstructions(Color.Red);

      Assert.NotEmpty(subComponentGoo.DrawInstructionsList);
      Assert.Single(subComponentGoo.DrawInstructionsList);
      // Assert.Contains( subComponentGoo.DrawInstructionsList.Any(x => x.GetType() == typeof()));
    }

    [Fact]
    public void ShouldDrawWires() {
      using var doc = RhinoDoc.Create(string.Empty);
      GH_PreviewWireArgs ghPreviewWireArgs = ComponentTestHelper.CreatePreviewArgs(doc, Color.White);

      subComponentGoo.DrawViewportWires(ghPreviewWireArgs);

      Assert.NotEmpty(subComponentGoo.DrawInstructionsList);
    }

    [Fact]
    public void ShouldDrawViewportMeshes() {
      using var doc = RhinoDoc.Create(string.Empty);
      var previewMeshArgs = ComponentTestHelper.CreatePreviewMeshArgs(doc, new DisplayMaterial(Color.White));

      subComponentGoo.DrawViewportMeshes(previewMeshArgs);

      Assert.NotEmpty(subComponentGoo.DrawInstructionsList);
    }

    [Fact]
    public void ShouldPrepareInstructionsWhenBrepIsValid() {
      Assert.Empty(subComponentGoo.DrawInstructionsList);

      subComponentGoo.TryPrepareDrawMeshesInstructions();

      Assert.NotEmpty(subComponentGoo.DrawInstructionsList);
      Assert.Single(subComponentGoo.DrawInstructionsList);
    }

    [Fact]
    public void ShouldNotDrawOnViewportMeshesWhenSectionIsNull() {
      subComponentGoo.section = null; // set to null to simulate no brep
      Assert.Empty(subComponentGoo.DrawInstructionsList);

      subComponentGoo.TryPrepareDrawMeshesInstructions();

      Assert.Empty(subComponentGoo.DrawInstructionsList);
    }

    [Fact]
    public void ShouldNotDrawOnViewportWiresWhenSectionIsNull() {
      subComponentGoo.section = null; // set to null to simulate no brep
      Assert.Empty(subComponentGoo.DrawInstructionsList);

      subComponentGoo.TryPrepareDrawWiresInstructions(Color.Red);

      Assert.Empty(subComponentGoo.DrawInstructionsList);
    }

    [Fact]
    public void BrepShadedShouldBeAddedToInstructionList() {
      var displayMaterial = new DisplayMaterial(Color.Red);

      Assert.Empty(subComponentGoo.DrawInstructionsList);

      var brep = new Brep();
      subComponentGoo.AddBrepShaded(brep, displayMaterial);

      Assert.Single(subComponentGoo.DrawInstructionsList);
      Assert.IsType<DrawBrepShaded>(subComponentGoo.DrawInstructionsList[0]);
      Assert.Equal(brep, ((DrawBrepShaded)subComponentGoo.DrawInstructionsList[0]).Brep);
      Assert.Equal(displayMaterial, ((DrawBrepShaded)subComponentGoo.DrawInstructionsList[0]).DisplayMaterial);
    }

    [Fact]
    public void BrepsShadedShouldNotBeAddedWhenNullBreps() {
      Assert.Empty(subComponentGoo.DrawInstructionsList);
      subComponentGoo.AddBrepsShaded(null, new List<DisplayMaterial>());

      Assert.Empty(subComponentGoo.DrawInstructionsList);
    }

    [Fact]
    public void BrepsShadedShouldNotBeAddedWhenNullMaterials() {
      Assert.Empty(subComponentGoo.DrawInstructionsList);
      subComponentGoo.AddBrepsShaded(new List<Brep>(), null);

      Assert.Empty(subComponentGoo.DrawInstructionsList);
    }

    [Fact]
    public void BrepsShadedShouldNotBeAddedWhenEmptyBreps() {
      Assert.Empty(subComponentGoo.DrawInstructionsList);
      subComponentGoo.AddBrepsShaded(new List<Brep>(), new List<DisplayMaterial>());

      Assert.Empty(subComponentGoo.DrawInstructionsList);
    }

    [Fact]
    public void BrepsShadedShouldBeAdded() {
      Assert.Empty(subComponentGoo.DrawInstructionsList);
      subComponentGoo.AddBrepsShaded(new List<Brep>() { new Brep(), },
        new List<DisplayMaterial>() { new DisplayMaterial(Color.Red), });

      Assert.Single(subComponentGoo.DrawInstructionsList);
    }

    [Fact]
    public void EdgesShouldBeAddedToInstructionList() {
      var edges = new List<Polyline>() { new Polyline(new Point3d[] { Point3d.Origin, }), };
      var color = Color.Red;
      int width = 2;

      Assert.Empty(subComponentGoo.DrawInstructionsList);

      subComponentGoo.AddEdges(edges, color, width);

      Assert.Single(subComponentGoo.DrawInstructionsList);
      Assert.IsType<DrawPolyline>(subComponentGoo.DrawInstructionsList[0]);
      Assert.Equal(edges[0], ((DrawPolyline)subComponentGoo.DrawInstructionsList[0]).Polyline);
      Assert.Equal(color, ((DrawPolyline)subComponentGoo.DrawInstructionsList[0]).Color);
      Assert.Equal(width, ((DrawPolyline)subComponentGoo.DrawInstructionsList[0]).Thickness);
    }

    [Fact]
    public void EdgesShouldNotBeAddedWhenNullEdges() {
      Assert.Empty(subComponentGoo.DrawInstructionsList);
      subComponentGoo.AddEdges(null, Color.Red, 2);

      Assert.Empty(subComponentGoo.DrawInstructionsList);
    }

    [Fact]
    public void NestedEdgesShouldBeAddedToInstructionList() {
      var nestedEdges = new List<IEnumerable<Polyline>>() {
        new List<Polyline>() { new Polyline(new Point3d[] { Point3d.Origin, }), },
      };
      var color = Color.Red;
      int width = 2;

      Assert.Empty(subComponentGoo.DrawInstructionsList);

      subComponentGoo.AddNestedEdges(nestedEdges, color, width);

      Assert.Single(subComponentGoo.DrawInstructionsList);
      Assert.IsType<DrawPolyline>(subComponentGoo.DrawInstructionsList[0]);
      Assert.Equal(nestedEdges[0].First(), ((DrawPolyline)subComponentGoo.DrawInstructionsList[0]).Polyline);
      Assert.Equal(color, ((DrawPolyline)subComponentGoo.DrawInstructionsList[0]).Color);
      Assert.Equal(width, ((DrawPolyline)subComponentGoo.DrawInstructionsList[0]).Thickness);
    }

    [Fact]
    public void NestedEdgesShouldNotBeAddedWhenNullNestedEdges() {
      Assert.Empty(subComponentGoo.DrawInstructionsList);
      subComponentGoo.AddNestedEdges(null, Color.Red, 2);

      Assert.Empty(subComponentGoo.DrawInstructionsList);
    }

    [Fact]
    public void NestedEdgesShouldNotBeAddedWhenEmptyNestedEdges() {
      Assert.Empty(subComponentGoo.DrawInstructionsList);
      subComponentGoo.AddNestedEdges(new List<IEnumerable<Polyline>>(), Color.Red, 2);

      Assert.Empty(subComponentGoo.DrawInstructionsList);
    }

    [Fact]
    public void CirclesShouldBeAddedToInstructionList() {
      var circles = new List<Circle>() { new Circle(Point3d.Origin, 1), };
      var color = Color.Red;
      int width = 2;

      Assert.Empty(subComponentGoo.DrawInstructionsList);

      subComponentGoo.AddCircles(circles, color, width);

      Assert.Single(subComponentGoo.DrawInstructionsList);
      Assert.IsType<DrawCircle>(subComponentGoo.DrawInstructionsList[0]);
      Assert.Equal(circles[0], ((DrawCircle)subComponentGoo.DrawInstructionsList[0]).Circle);
      Assert.Equal(color, ((DrawCircle)subComponentGoo.DrawInstructionsList[0]).Color);
      Assert.Equal(width, ((DrawCircle)subComponentGoo.DrawInstructionsList[0]).Thickness);
    }

    [Fact]
    public void CirclesShouldNotBeAddedWhenNullCircles() {
      Assert.Empty(subComponentGoo.DrawInstructionsList);
      subComponentGoo.AddCircles(null, Color.Red, 2);

      Assert.Empty(subComponentGoo.DrawInstructionsList);
    }

    [Fact]
    public void CirclesShouldNotBeAddedWhenEmptyCircles() {
      Assert.Empty(subComponentGoo.DrawInstructionsList);
      subComponentGoo.AddCircles(new List<Circle>(), Color.Red, 2);

      Assert.Empty(subComponentGoo.DrawInstructionsList);
    }

    private static AdSecSectionGoo GetAdSecSectionGoo() {
      var length = new Length(1, LengthUnit.Meter);
      var profile = AdSecProfiles.CreateProfile(new AngleProfile(length, new Flange(length, length),
        new WebConstant(length)));
      var material = Steel.ASTM.A242_46;
      var section = ISection.Create(profile, material);
      var designCode = IS456.Edition_2000;
      return new AdSecSectionGoo(new AdSecSection(section, designCode, string.Empty, string.Empty, Plane.WorldXY));
    }
  }
}
