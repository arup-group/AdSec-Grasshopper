using AdSecGH.Parameters;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

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
  }
}
