using System.Collections.Generic;

using AdSecGH.Components;
using AdSecGH.Parameters;

using AdSecGHTests.Helpers;

using OasysUnits;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Properties {
  [Collection("GrasshopperFixture collection")]
  public class NMDiagramTest {
    private static NMDiagram _components;
    private static Angle _angle;
    private static bool _isMMCurve;
    public NMDiagramTest() {
      _components = ComponentMother();
      _isMMCurve = false;
      _angle = Angle.FromRadians(0);
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

    private static void SetPlotBoundary(Rectangle3d rectangle) {
      //set boundary in such a way that neutralize translation
      ComponentTestHelper.SetInput(_components, rectangle, 2);
    }

    private static BoundingBox LoadBoundingBox() {
      var curve = NMCurve().LoadCurve;
      return AdSecNMMCurveGoo.CurveToPolyline(curve, _angle, _isMMCurve).BoundingBox;
    }

    private static void SetMMCurve() {
      _isMMCurve = true;
      _components.SetSelected(0, 1);
    }

    private static void SetAngle(bool radian = true) {
      if (radian) {
        _components.SetSelected(1, 0);
        _angle = Angle.FromRadians(0.785398);
      } else {
        _components.SetSelected(1, 1);
        _angle = Angle.FromDegrees(45.0);
      }
      ComponentTestHelper.SetInput(_components, _angle.Value, 1);
    }

    private static void SetAxialForce(double force) {
      ComponentTestHelper.SetInput(_components, force, 1);
    }

    [Fact]
    public void NMCurveIsReportingCorrectPeakValueAtBoundary() {
      SetPlotBoundary(new Rectangle3d(Plane.WorldXY, new Point3d(200, 1400, 0), new Point3d(-200, -600, 0)));
      Assert.True(AdSecUtility.IsBoundingBoxEqual(NMCurve().Value.BoundingBox, new BoundingBox(new Point3d(-184.84, -453.48, 0), new Point3d(184.84, 1251.86, 0))));
    }

    [Fact]
    public void NMCurveIsReportingCorrectPeakValue() {
      Assert.True(AdSecUtility.IsBoundingBoxEqual(LoadBoundingBox(), new BoundingBox(new Point3d(-184.84, -453.48, 0), new Point3d(184.84, 1251.86, 0))));
    }

    [Fact]
    public void NMCurveIsReportingCorrectPeakValueAtAngleInRadian() {
      SetAngle();
      Assert.True(AdSecUtility.IsBoundingBoxEqual(LoadBoundingBox(), new BoundingBox(new Point3d(-90.73, -453.48, 0), new Point3d(90.73, 1251.86, 0))));
    }

    [Fact]
    public void NMCurveIsReportingCorrectPeakValueAtAngleInDegree() {
      SetAngle(false);
      Assert.True(AdSecUtility.IsBoundingBoxEqual(LoadBoundingBox(), new BoundingBox(new Point3d(-90.73, -453.48, 0), new Point3d(90.73, 1251.86, 0))));
    }

    [Fact]
    public void MMCurveIsReportingCorrectPeakValue() {
      SetMMCurve();
      Assert.True(AdSecUtility.IsBoundingBoxEqual(LoadBoundingBox(), new BoundingBox(new Point3d(-127.03, -59.28, 0), new Point3d(127.03, 59.28, 0))));
    }

    [Fact]
    public void MMCurveIsReportingCorrectPeakValueAtGivenAxialForce() {
      SetAxialForce(200);
      SetMMCurve();
      Assert.True(AdSecUtility.IsBoundingBoxEqual(LoadBoundingBox(), new BoundingBox(new Point3d(-71.17, -33.19, 0), new Point3d(71.17, 33.19, 0))));
    }
  }
}
