using System;
using System.Collections.Generic;

using AdSecGH.Components;
using AdSecGH.Parameters;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel.Types;

using OasysUnits;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class NMDiagramTest {
    private static NMDiagram _components;
    public NMDiagramTest() {
      _components = ComponentMother();
    }

    public static AdSecNMMCurveGoo NMCurve() {
      return (AdSecNMMCurveGoo)ComponentTestHelper.GetOutput(_components);
    }

    public static NMDiagram ComponentMother() {
      var component = new NMDiagram();
      component.CreateAttributes();
      component.SetSelected(0, 0);
      ComponentTestHelper.SetInput(component, AdSecUtility.GetResult(), 0);
      return component;
    }

    private static BoundingBox LoadBoundingBox() {
      return NMCurve().CurveToPolyline().BoundingBox;
    }

    private static void SetPlotBoundary() {
      //set boundary in such a way that neutralize translation
      Rectangle3d rectangle = new Rectangle3d(Plane.WorldXY, new Point3d(200, 1400, 0), new Point3d(-200, -600, 0));
      ComponentTestHelper.SetInput(_components, rectangle, 2);
    }

    private static void SetMMCurve() {
      _components.SetSelected(0, 1);
    }

    private static void SetAngle(bool radian = true) {
      if (radian) {
        _components.SetSelected(1, 0);
        ComponentTestHelper.SetInput(_components, Angle.FromRadians(0.785398).Value, 1);
      } else {
        _components.SetSelected(1, 1);
        ComponentTestHelper.SetInput(_components, Angle.FromDegrees(45.0).Value, 1);
      }
    }

    private static void SetAxialForce(double force) {
      ComponentTestHelper.SetInput(_components, force, 1);
    }

    [Fact]
    public void NMCurveIsReportingCorrectPeakValueAtBoundary() {
      //expected output is from post processing of input data
      SetPlotBoundary();
      var expectedMinPoint = new Point3d(-184.84, -453.48, 0);
      var expectedMaxPoint = new Point3d(184.84, 1251.86, 0);
      var expectedBoundingBox = new BoundingBox(expectedMinPoint, expectedMaxPoint);
      var actualBoundingBox = LoadBoundingBox();
      bool areEqual = AdSecUtility.IsBoundingBoxEqual(expectedBoundingBox, actualBoundingBox);
      Assert.True(areEqual);
    }

    [Fact]
    public void NMCurveIsReportingCorrectPeakValue() {
      //expected output is from post processing of input data
      var expectedMinPoint = new Point3d(-184.84, -453.48, 0);
      var expectedMaxPoint = new Point3d(184.84, 1251.86, 0);
      var expectedBoundingBox = new BoundingBox(expectedMinPoint, expectedMaxPoint);
      var actualBoundingBox = LoadBoundingBox();
      bool areEqual = AdSecUtility.IsBoundingBoxEqual(expectedBoundingBox, actualBoundingBox);
      Assert.True(areEqual);
    }

    [Fact]
    public void NMCurveIsReportingCorrectPeakValueAtAngleInRadian() {
      //expected output is from post processing of input data
      SetAngle();
      var expectedMinPoint = new Point3d(-90.73, -453.48, 0);
      var expectedMaxPoint = new Point3d(90.73, 1251.86, 0);
      var expectedBoundingBox = new BoundingBox(expectedMinPoint, expectedMaxPoint);
      var actualBoundingBox = LoadBoundingBox();
      bool areEqual = AdSecUtility.IsBoundingBoxEqual(expectedBoundingBox, actualBoundingBox);
      Assert.True(areEqual);
    }

    [Fact]
    public void NMCurveIsReportingCorrectPeakValueAtAngleInDegree() {
      //expected output is from post processing of input data
      SetAngle(false);
      var expectedMinPoint = new Point3d(-90.73, -453.48, 0);
      var expectedMaxPoint = new Point3d(90.73, 1251.86, 0);
      var expectedBoundingBox = new BoundingBox(expectedMinPoint, expectedMaxPoint);
      var actualBoundingBox = LoadBoundingBox();
      bool areEqual = AdSecUtility.IsBoundingBoxEqual(expectedBoundingBox, actualBoundingBox);
      Assert.True(areEqual);
    }

    [Fact]
    public void MMCurveIsReportingCorrectPeakValue() {
      //expected output is from post processing of input data
      SetMMCurve();
      var expectedMinPoint = new Point3d(-127.03, -59.28, 0);
      var expectedMaxPoint = new Point3d(127.03, 59.28, 0);
      var expectedBoundingBox = new BoundingBox(expectedMinPoint, expectedMaxPoint);
      var actualBoundingBox = LoadBoundingBox();
      bool areEqual = AdSecUtility.IsBoundingBoxEqual(expectedBoundingBox, actualBoundingBox);
      Assert.True(areEqual);
    }

    [Fact]
    public void MMCurveIsReportingNullWhenAxialForceisOutOfBound() {
      SetAxialForce(1000);
      SetMMCurve();
      Assert.Null(NMCurve());
    }

    [Fact]
    public void VolumeIsZeroWhenLoadCurveIsNull() {
      var curveGoo = new AdSecNMMCurveGoo(null, Angle.FromRadians(0), new Rectangle3d());
      Assert.Equal(0, curveGoo.Boundingbox.Volume, 5);
    }

    [Fact]
    public void VolumeIsZeroWhenLoadCurveIsNullInOverLoadMethod() {
      var curveGoo = new AdSecNMMCurveGoo(null, new Angle(), new Rectangle3d(), AdSecNMMCurveGoo.InteractionCurveType.NM);
      Assert.Equal(0, curveGoo.Boundingbox.Volume, 5);
    }

    [Fact]
    public void CastToPolyLine() {
      var curveGoo = NMCurve();
      GH_Curve curve = null;
      Assert.True(curveGoo.CastTo(ref curve));
      Assert.NotNull(curve);
    }

    [Fact]
    public void CastToAdSecNMMCurveGoo() {
      var curveGoo = NMCurve();
      AdSecNMMCurveGoo castedCurve = null;
      Assert.True(curveGoo.CastTo(ref castedCurve));
      Assert.NotNull(castedCurve);
    }

    [Fact]
    public void NegativeCastIsNull() {
      var curveGoo = NMCurve();
      GH_Point point = null;
      Assert.False(curveGoo.CastTo(ref point));
      Assert.Null(point);
    }

    [Fact]
    public void ToStringIsExpectedForNMChart() {
      var curveGoo = NMCurve();
      var output = curveGoo.ToString();
      Assert.Equal("AdSec N-M {N-M (Force-Moment Interaction)}", output);
    }

    [Fact]
    public void ToStringIsExpectedForMMChart() {
      SetMMCurve();
      var curveGoo = NMCurve();
      var output = curveGoo.ToString();
      Assert.Equal("AdSec M-M {M-M (Moment-Moment Interaction)}", output);
    }

    [Fact]
    public void TransformedBoundingBoxIsCorrect() {
      var curveGoo = NMCurve();
      var actualBoundingBox = curveGoo.Boundingbox;
      var expectedBoundingBox = curveGoo.GetBoundingBox(Transform.Translation(new Vector3d(1, 1, 1)));
      Assert.Equal(actualBoundingBox.Center.X + 1, expectedBoundingBox.Center.X, 5);
      Assert.Equal(actualBoundingBox.Center.Y + 1, expectedBoundingBox.Center.Y, 5);
      Assert.Equal(actualBoundingBox.Center.Z + 1, expectedBoundingBox.Center.Z, 5);
    }

    [Fact]
    public void DupliCateGeometryIsCorrect() {
      var curveGoo = NMCurve();
      var duplicateGeometry = curveGoo.DuplicateGeometry();
      Assert.True(AdSecUtility.IsBoundingBoxEqual(curveGoo.Boundingbox, duplicateGeometry.Boundingbox));
      Assert.Equal("AdSec N-M Parameter", duplicateGeometry.TypeDescription);
      Assert.Equal("N-M", duplicateGeometry.TypeName);
    }
  }
}
