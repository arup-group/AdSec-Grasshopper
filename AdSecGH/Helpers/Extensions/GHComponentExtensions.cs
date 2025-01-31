using System.Collections.Generic;
using System.Linq;

using AdSecGH.Parameters;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.Profiles;

using OasysGH.Units;

using OasysUnits;

namespace AdSecGH.Helpers {
  public static class GHComponentExtensions {
    public static AdSecDesignCode GetAdSecDesignCode(
      this GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      AdSecDesignCode designCode = null;
      GH_ObjectWrapper inputData = null;

      bool isDataAvailable = DA.GetData(inputId, ref inputData);

      if (!isDataAvailable && !isOptional) {
        owner.Params.Input[inputId].FailedToCollectDataWarning();
      } else if (isDataAvailable && !AdSecInput.TryCastToDesignCode(inputData, ref designCode)) {
        owner.Params.Input[inputId].ConvertToError("DesignCode");
      }

      return designCode;
    }

    public static AdSecMaterial GetAdSecMaterial(
      this GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      AdSecMaterial material = null;
      GH_ObjectWrapper inputData = null;

      bool isDataAvailable = DA.GetData(inputId, ref inputData);

      if (!isDataAvailable && !isOptional) {
        owner.Params.Input[inputId].FailedToCollectDataWarning();
      } else if (isDataAvailable && !AdSecInput.TryCastToAdSecMaterial(inputData, ref material)) {
        owner.Params.Input[inputId].ConvertToError("an AdSec Material");
      }

      return material;
    }

    public static AdSecPointGoo GetAdSecPointGoo(
      this GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      AdSecPointGoo pointGoo = null;
      GH_ObjectWrapper inputData = null;

      bool isDataAvailable = DA.GetData(inputId, ref inputData);

      if (!isDataAvailable && !isOptional) {
        owner.Params.Input[inputId].FailedToCollectDataWarning();
      } else if (isDataAvailable && !AdSecInput.TryCastToAdSecPointGoo(inputData, ref pointGoo)) {
        owner.Params.Input[inputId].ConvertToError("a Vertex Point");
      } else if (!isDataAvailable) {
        pointGoo = new AdSecPointGoo(IPoint.Create(new Length(0, DefaultUnits.LengthUnitGeometry),
          new Length(0, DefaultUnits.LengthUnitGeometry)));
      }

      return pointGoo;
    }

    public static AdSecProfileGoo GetAdSecProfileGoo(
      this GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      GH_ObjectWrapper inputData = null;
      AdSecProfileGoo profileGoo = null;
      bool isDataAvailable = DA.GetData(inputId, ref inputData);

      if (!isDataAvailable && !isOptional) {
        owner.Params.Input[inputId].FailedToCollectDataWarning();
      } else if (isDataAvailable && !AdSecInput.TryCastToAdSecProfileGoo(inputData, ref profileGoo)) {
        owner.Params.Input[inputId].ConvertToError("an AdSec Profile");
      }

      return profileGoo;
    }

    public static AdSecRebarBundleGoo GetAdSecRebarBundleGoo(
      this GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      AdSecRebarBundleGoo rebar = null;
      GH_ObjectWrapper inputData = null;
      bool showRemark = false;

      bool isDataAvailable = DA.GetData(inputId, ref inputData);

      if (!isDataAvailable && !isOptional) {
        owner.Params.Input[inputId].FailedToCollectDataWarning();
      } else if (isDataAvailable && !AdSecInput.TryCastToAdSecRebarBundleGoo(inputData, ref rebar, ref showRemark)) {
        owner.Params.Input[inputId].ConvertToError("Rebar");
      } else if (showRemark) {
        owner.AddRuntimeRemark(
          $"Converted {owner.Params.Input[inputId].NickName} from RebarSpacing to an AdSec Rebar. All spacing information has been lost!");
      }

      return rebar;
    }

    public static AdSecRebarLayerGoo GetAdSecRebarLayerGoo(
      this GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      AdSecRebarLayerGoo spacing = null;
      GH_ObjectWrapper inputData = null;

      bool isDataAvailable = DA.GetData(inputId, ref inputData);
      if (!isDataAvailable && !isOptional) {
        owner.Params.Input[inputId].FailedToCollectDataWarning();
      } else if (isDataAvailable && !AdSecInput.TryCastToAdSecRebarLayerGoo(inputData, ref spacing)) {
        owner.Params.Input[inputId].ConvertToError("RebarSpacing");
      }

      return spacing;
    }

    public static AdSecSection GetAdSecSection(
      this GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      AdSecSection section = null;
      GH_ObjectWrapper inputData = null;

      bool isDataAvailable = DA.GetData(inputId, ref inputData);
      if (!isDataAvailable && !isOptional) {
        owner.Params.Input[inputId].FailedToCollectDataWarning();
      } else if (isDataAvailable && !AdSecInput.TryCastToAdSecSection(inputData, ref section)) {
        owner.Params.Input[inputId].ConvertToError("Section");
      }

      return section;
    }

    public static List<AdSecSection> GetAdSecSections(
      this GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      var sections = new List<AdSecSection>();
      var inputData = new List<GH_ObjectWrapper>();
      var invalidIds = new List<int>();

      DA.GetDataList(inputId, inputData);
      bool isDataAvailable = inputData.TrueForAll(item => item != null);
      if (!isDataAvailable && !isOptional) {
        owner.Params.Input[inputId].FailedToCollectDataWarning();
      } else if (isDataAvailable && !AdSecInput.TryCastToAdSecSections(inputData, sections, invalidIds)) {
        invalidIds.ForEach(id => owner.Params.Input[inputId].ConvertFromToError($"(item {id})", "Section"));
      }

      return sections.Any() ? sections : null;
    }

    public static IConcreteCrackCalculationParameters GetIConcreteCrackCalculationParameters(
      this GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      IConcreteCrackCalculationParameters calculationParameters = null;
      GH_ObjectWrapper inputData = null;

      bool isDataAvailable = DA.GetData(inputId, ref inputData);
      if (!isDataAvailable && !isOptional) {
        owner.Params.Input[inputId].FailedToCollectDataWarning();
      } else if (isDataAvailable
        && !AdSecInput.TryCastToConcreteCrackCalculationParameters(inputData, ref calculationParameters)) {
        owner.Params.Input[inputId].ConvertToError("ConcreteCrackCalculationParameters");
      }

      return calculationParameters;
    }

    public static Oasys.Collections.IList<ILayer> GetILayers(
      this GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      var inputData = new List<GH_ObjectWrapper>();

      DA.GetDataList(inputId, inputData);
      bool isDataAvailable = inputData.TrueForAll(item => item != null);
      var layers = Oasys.Collections.IList<ILayer>.Create();
      var invalidIds = new List<int>();

      if (!isDataAvailable && !isOptional) {
        owner.Params.Input[inputId].FailedToCollectDataWarning();
      } else if (isDataAvailable && !AdSecInput.TryCastToILayers(inputData, layers, invalidIds)) {
        invalidIds.ForEach(id => owner.Params.Input[inputId].ConvertFromToError($"(item {id})", "RebarLayer"));
      }

      return layers.Any() ? layers : null;
    }

    public static Oasys.Collections.IList<IPoint> GetIPoints(
      this GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      var inputData = new List<GH_ObjectWrapper>();
      var points = Oasys.Collections.IList<IPoint>.Create();
      var invalidIds = new List<int>();
      int pointsConverted = 0;

      DA.GetDataList(inputId, inputData);
      bool isDataAvailable = inputData.TrueForAll(item => item != null);

      if (!isDataAvailable && !isOptional) {
        owner.Params.Input[inputId].FailedToCollectDataWarning();
      } else if (isDataAvailable && !AdSecInput.TryCastToIPoints(inputData, points, invalidIds, ref pointsConverted)) {
        invalidIds.ForEach(id
          => owner.Params.Input[inputId].ConvertFromToError($"(item {id})", "StressStrainPoint or Polyline"));
      }

      if (pointsConverted == 1) {
        owner.AddRuntimeRemark(
          "Single Point converted to local point. Assumed that local coordinate system is in a YZ-Plane");
      } else if (pointsConverted > 1) {
        owner.AddRuntimeRemark(
          "List of Points have been converted to local points. Assumed that local coordinate system is matching best fit plane through points");
      }

      return points.Any() ? points : null;
    }

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
