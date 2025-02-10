using AdSecGH.Components;
using AdSecGH.Parameters;

using AdSecGHTests.Helpers;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Properties {
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

    private static void SetPlotBoundary(Rectangle3d rectangle) {
      //set boundary in such a way that neutralize translation
      ComponentTestHelper.SetInput(_components, rectangle, 2);
    }

    private static void SetMMCurve() {
      _components.SetSelected(0, 1);
    }

    private static void SetAngle(bool radian = true) {
      if (radian) {
        _components.SetSelected(1, 0);
        ComponentTestHelper.SetInput(_components, 0.785398, 1);
      } else {
        _components.SetSelected(1, 1);
        ComponentTestHelper.SetInput(_components, 45, 1);
      }
    }

    private static void SetAxialForce(double force) {
      ComponentTestHelper.SetInput(_components, force, 1);
    }

    [Fact]
    public void NMCurveIsReportingCorrectPeakValue() {
      //give
      SetPlotBoundary(new Rectangle3d(Plane.WorldXY, new Point3d(200, 1400, 0), new Point3d(-200, -600, 0)));
      Assert.True(AdSecUtility.IsBoundingBoxEqual(NMCurve().Value.BoundingBox, new BoundingBox(new Point3d(-184.84, -453.48, 0), new Point3d(184.84, 1251.86, 0))));
    }

    [Fact]
    public void NMCurveIsReportingCorrectPeakValueAtAngleInRadian() {
      SetAngle();
      SetPlotBoundary(new Rectangle3d(Plane.WorldXY, new Point3d(100, 1400, 0), new Point3d(-100, -600, 0)));
      Assert.True(AdSecUtility.IsBoundingBoxEqual(NMCurve().Value.BoundingBox, new BoundingBox(new Point3d(-90.73, -453.48, 0), new Point3d(90.73, 1251.86, 0))));
    }

    [Fact]
    public void NMCurveIsReportingCorrectPeakValueAtAngleInDegree() {
      SetAngle(false);
      SetPlotBoundary(new Rectangle3d(Plane.WorldXY, new Point3d(100, 1400, 0), new Point3d(-100, -600, 0)));
      Assert.True(AdSecUtility.IsBoundingBoxEqual(NMCurve().Value.BoundingBox, new BoundingBox(new Point3d(-90.73, -453.48, 0), new Point3d(90.73, 1251.86, 0))));
    }

    [Fact]
    public void MMCurveIsReportingCorrectPeakValue() {
      SetMMCurve();
      SetPlotBoundary(new Rectangle3d(Plane.WorldXY, new Point3d(150, 60, 0), new Point3d(-150, -60, 0)));
      Assert.True(AdSecUtility.IsBoundingBoxEqual(NMCurve().Value.BoundingBox, new BoundingBox(new Point3d(-127.03, -59.28, 0), new Point3d(127.03, 59.28, 0))));
    }

    [Fact]
    public void MMCurveIsReportingCorrectPeakValueAtGivenAxialForce() {
      SetAxialForce(200);
      SetMMCurve();
      SetPlotBoundary(new Rectangle3d(Plane.WorldXY, new Point3d(80, 40, 0), new Point3d(-80, -40, 0)));
      Assert.True(AdSecUtility.IsBoundingBoxEqual(NMCurve().Value.BoundingBox, new BoundingBox(new Point3d(-71.17, -33.19, 0), new Point3d(71.17, 33.19, 0))));
    }


  }
}
