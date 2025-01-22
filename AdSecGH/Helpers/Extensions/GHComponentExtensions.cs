using System.Collections.Generic;

using AdSecGH.Parameters;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec.Materials.StressStrainCurves;

namespace AdSecGH.Helpers {
  public static class GHComponentExtensions {
    public static AdSecStressStrainCurveGoo GetStressStrainCurveGoo(
      this GH_Component owner, IGH_DataAccess DA, int inputId, bool compression, bool isOptional = false) {
      AdSecStressStrainCurveGoo curveGoo = null;
      GH_ObjectWrapper inputData = null;

      bool isDataAvailable = DA.GetData(inputId, ref inputData);

      if (!isDataAvailable && !isOptional) {
        owner.Params.Input[inputId].FailedToCollectDataWarning();
      } else if (isDataAvailable && !AdSecInput.TryCastToStressStrainCurve(compression, inputData, ref curveGoo)) {
        owner.Params.Input[inputId].ConvertToError("StressStrainCurve");
      }

      return curveGoo;
    }

    public static IStressStrainPoint GetStressStrainPoint(
      this GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      IStressStrainPoint stressStrainPoint = null;
      GH_ObjectWrapper inputData = null;

      bool isDataAvailable = DA.GetData(inputId, ref inputData);

      if (!isDataAvailable && !isOptional) {
        owner.Params.Input[inputId].FailedToCollectDataWarning();
      } else if (isDataAvailable && !AdSecInput.TryCastToStressStrainPoint(inputData, ref stressStrainPoint)) {
        owner.Params.Input[inputId].ConvertToError("StressStrainPoint");
      }

      return stressStrainPoint;
    }

    public static Oasys.Collections.IList<IStressStrainPoint> GetStressStrainPoints(
      this GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      var inputData = new List<GH_ObjectWrapper>();

      DA.GetDataList(inputId, inputData);
      bool isDataAvailable = inputData.TrueForAll(item => item != null);
      var points = Oasys.Collections.IList<IStressStrainPoint>.Create();

      if (!isDataAvailable && !isOptional) {
        owner.Params.Input[inputId].FailedToCollectDataWarning();
      } else if (isDataAvailable && !AdSecInput.TryCastToStressStrainPoints(inputData, ref points)) {
        owner.Params.Input[inputId].ConvertToError("StressStrainPoint or a Polyline");
      }

      if (points.Count >= 2) {
        return points;
      }

      owner.AddRuntimeWarning("Input must contain at least 2 points to create an Explicit Stress Strain Curve");

      return null;
    }
  }
}
