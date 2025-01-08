using System.Linq;

using AdSecGH.Components;
using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Moq;

using Oasys.AdSec.Materials.StressStrainCurves;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecInputTests {
    private CreateStressStrainPoint component;
    private Mock<IGH_DataAccess> dataAccess;

    //StressStrainPoint
    //StressStrainPoints
    //StressStrainCurveGoo
    //IStressStrainPoint StressStrainPoint(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
    //Oasys.Collections.IList<IStressStrainPoint> StressStrainPoints(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
    public AdSecInputTests() {
      component = new CreateStressStrainPoint();
      dataAccess = new Mock<IGH_DataAccess>();
    }
    //internal static AdSecStressStrainCurveGoo StressStrainCurveGoo(
    //  GH_Component owner, IGH_DataAccess DA, int inputid, bool compression, bool isOptional = false) {
    //  AdSecStressStrainCurveGoo ssCrv = null;
    //  var gh_typ = new GH_ObjectWrapper();
    //  if (DA.GetData(inputid, ref gh_typ)) {
    //    Curve polycurve = null;
    //    if (gh_typ.Value is AdSecStressStrainCurveGoo goo) {
    //      // try direct cast
    //      ssCrv = goo;
    //    } else if (GH_Convert.ToCurve(gh_typ.Value, ref polycurve, GH_Conversion.Both)) {
    //      // try convert to polylinecurve
    //      var curve = (PolylineCurve)polycurve;
    //      var pts = AdSecStressStrainCurveGoo.StressStrainPtsFromPolyline(curve);
    //      var exCrv = IExplicitStressStrainCurve.Create();
    //      exCrv.Points = pts;
    //      var tuple = AdSecStressStrainCurveGoo.Create(exCrv, AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit,
    //        compression);
    //      ssCrv = new AdSecStressStrainCurveGoo(tuple.Item1, exCrv,
    //        AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit, tuple.Item2);
    //    } else {
    //      owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
    //        "Unable to convert " + owner.Params.Input[inputid].NickName + " to StressStrainCurve");
    //      return null;
    //    }

    //    return ssCrv;
    //  }

    //  if (!isOptional) {
    //    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
    //      "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
    //  }

    //  return null;
    //}

    [Fact]
    public void StressStrainCurveGooReturnsNull() {
      var result = AdSecInput.StressStrainCurveGoo(null, dataAccess.Object, 0, false, true);
      Assert.Null(result);
    }

    [Fact]
    public void StressStrainCurveGooReturnsWarningWhenInputIsNonOptional() {
      var result = AdSecInput.StressStrainCurveGoo(component, dataAccess.Object, 0, false);
      Assert.Null(result);

      var runtimeWarnings = component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Single(runtimeWarnings);
      Assert.Contains("failed", runtimeWarnings.First());
    }

    [Fact]
    public void StressStrainCurveGooReturnsErrorWhenCantConvert() {
      var objwrap = new GH_ObjectWrapper();
      var result = AdSecInput.TryCastToStressStrainCurve(component, 0, false, objwrap);
      Assert.Null(result);

      var runtimeErrors = component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(runtimeErrors);
      Assert.Equal("Unable to convert ε to StressStrainCurve", runtimeErrors.First());
    }

    [Fact]
    public void StressStrainCurveGooReturnsGooFromPolyline() {
      IStressStrainCurve crv = ILinearStressStrainCurve.Create(
        IStressStrainPoint.Create(new Pressure(0, PressureUnit.Pascal), new Strain(1, StrainUnit.Ratio)));
      var tuple = AdSecStressStrainCurveGoo.Create(crv, AdSecStressStrainCurveGoo.StressStrainCurveType.Linear, false);

      var objwrap = new GH_ObjectWrapper(tuple.Item1);
      var result = AdSecInput.TryCastToStressStrainCurve(component, 0, false, objwrap);

      Assert.NotNull(result);
      Assert.True(result.Value.IsPolyline());
      Assert.Equal(2, result.ControlPoints.Count);
    }

    [Fact]
    public void StressStrainCurveGooReturnsGooFromAnotherGoo() {
      var exCrv = IExplicitStressStrainCurve.Create();
      var tuple = AdSecStressStrainCurveGoo.Create(exCrv, AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit,
        false);
      var goo = new AdSecStressStrainCurveGoo(tuple.Item1, exCrv,
        AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit, tuple.Item2);

      var objwrap = new GH_ObjectWrapper(goo);
      var result = AdSecInput.TryCastToStressStrainCurve(component, 0, false, objwrap);

      Assert.NotNull(result);
      Assert.Null(result.Value);
      Assert.Empty(result.ControlPoints);
    }
  }
}
