using System.Collections.Generic;
using System.Linq;

using AdSecGH.Parameters;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.Profiles;

using OasysGH.Helpers;
using OasysGH.Parameters;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

namespace AdSecGH.Helpers {
  internal static class AdSecInput {
    internal static AdSecSection AdSecSection(
      GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputId, ref gh_typ)) {
        if (gh_typ.Value is AdSecSectionGoo a1) {
          return a1.Value;
        }

        if (gh_typ.Value is AdSecSubComponentGoo a2) {
          return a2._section;
        }

        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
          $"Unable to convert {owner.Params.Input[inputId].NickName} to Section");
        return null;
      }

      if (!isOptional) {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
          $"Input parameter {owner.Params.Input[inputId].NickName} failed to collect data!");
      }

      return null;
    }

    internal static List<AdSecSection> AdSecSections(
      GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      var subs = new List<AdSecSection>();
      var gh_typs = new List<GH_ObjectWrapper>();
      if (DA.GetDataList(inputId, gh_typs)) {
        for (int i = 0; i < gh_typs.Count; i++) {
          if (gh_typs[i].Value is AdSecSection section) {
            subs.Add(section);
          } else if (gh_typs[i].Value is AdSecSectionGoo subcomp) {
            subs.Add(subcomp.Value);
          } else {
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
              $"Unable to convert {owner.Params.Input[inputId].NickName} (item {i}) to Section");
          }
        }

        return subs;
      }

      if (!isOptional) {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
          $"Input parameter {owner.Params.Input[inputId].NickName} failed to collect data!");
      }

      return null;
    }

    internal static IConcreteCrackCalculationParameters ConcreteCrackCalculationParameters(
      GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      IConcreteCrackCalculationParameters concreteCrack = null;

      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputId, ref gh_typ)) {
        if (gh_typ.Value is IConcreteCrackCalculationParameters parameters) {
          concreteCrack = parameters;
        } else if (gh_typ.Value is AdSecConcreteCrackCalculationParametersGoo adsecccp) {
          concreteCrack = adsecccp.Value;
        } else {
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
            $"Unable to convert {owner.Params.Input[inputId].NickName} to ConcreteCrackCalculationParameters");
          return null;
        }

        return concreteCrack;
      }

      if (!isOptional) {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
          $"Input parameter {owner.Params.Input[inputId].NickName} failed to collect data!");
      }

      return null;
    }

    internal static List<ICover> Covers(GH_Component owner, IGH_DataAccess DA, int inputId, LengthUnit docLengthUnit) {
      var covers = new List<ICover>();
      var lengths = Input.UnitNumberList(owner, DA, inputId, docLengthUnit);

      foreach (var length in lengths.Select(v => (Length)v)) {
        covers.Add(ICover.Create(length));
      }

      return covers;
    }

    internal static AdSecRebarGroupGoo ReinforcementGroup(
      GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputId, ref gh_typ)) {
        if (gh_typ.Value is AdSecRebarGroupGoo goo) {
          return goo;
        }

        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
          $"Unable to convert {owner.Params.Input[inputId].NickName} to RebarLayout");
        return null;
      }

      if (!isOptional) {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
          $"Input parameter {owner.Params.Input[inputId].NickName} failed to collect data!");
      }

      return null;
    }

    internal static List<AdSecRebarGroup> ReinforcementGroups(
      GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      var grps = new List<AdSecRebarGroup>();
      var gh_typs = new List<GH_ObjectWrapper>();
      if (DA.GetDataList(inputId, gh_typs)) {
        for (int i = 0; i < gh_typs.Count; i++) {
          if (gh_typs[i].Value is IGroup group) {
            grps.Add(new AdSecRebarGroup(group));
          } else if (gh_typs[i].Value is AdSecRebarGroupGoo rebarGoo) {
            grps.Add(rebarGoo.Value);
          } else {
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
              $"Unable to convert {owner.Params.Input[inputId].NickName} (item {i}) to RebarGroup");
          }
        }

        return grps;
      }

      if (!isOptional) {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
          $"Input parameter {owner.Params.Input[inputId].NickName} failed to collect data!");
      }

      return null;
    }

    internal static AdSecSolutionGoo Solution(
      GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputId, ref gh_typ)) {
        if (gh_typ.Value is AdSecSolutionGoo goo) {
          return goo;
        }

        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
          $"Unable to convert {owner.Params.Input[inputId].NickName} to AdSec Results");
        return null;
      }

      if (!isOptional) {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
          $"Input parameter {owner.Params.Input[inputId].NickName} failed to collect data!");
      }

      return null;
    }

    internal static Oasys.Collections.IList<ISubComponent> SubComponents(
      GH_Component owner, IGH_DataAccess DA, int inputId, bool isOptional = false) {
      var subs = Oasys.Collections.IList<ISubComponent>.Create();
      var gh_typs = new List<GH_ObjectWrapper>();
      if (DA.GetDataList(inputId, gh_typs)) {
        for (int i = 0; i < gh_typs.Count; i++) {
          if (gh_typs[i].Value is ISubComponent component) {
            subs.Add(component);
          } else if (gh_typs[i].Value is AdSecSubComponentGoo subcomp) {
            subs.Add(subcomp.Value);
          } else if (gh_typs[i].Value is AdSecSectionGoo section) {
            var offset = IPoint.Create(Length.Zero, Length.Zero);
            var sub = ISubComponent.Create(section.Value.Section, offset);
            subs.Add(sub);
          } else {
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
              $"Unable to convert {owner.Params.Input[inputId].NickName} (item {i}) to SubComponent or Section");
          }
        }

        return subs;
      }

      if (!isOptional) {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
          $"Input parameter {owner.Params.Input[inputId].NickName} failed to collect data!");
      }

      return null;
    }

    public static bool TryCastToDesignCode(GH_ObjectWrapper ghType, ref AdSecDesignCode designCode) {
      bool castSuccessful = true;
      if (ghType.Value is AdSecDesignCodeGoo) {
        ghType.CastTo(ref designCode);
      } else {
        castSuccessful = false;
      }

      return castSuccessful;
    }

    public static bool TryCastToAdSecMaterial(GH_ObjectWrapper ghType, ref AdSecMaterial material) {
      bool castSuccessful = true;
      if (ghType.Value is AdSecMaterialGoo) {
        ghType.CastTo(ref material);
      } else {
        castSuccessful = false;
      }

      return castSuccessful;
    }

    public static bool TryCastToAdSecPointGoo(GH_ObjectWrapper ghType, ref AdSecPointGoo pointGoo) {
      bool castSuccessful = true;
      var ghpt = new Point3d();
      if (ghType.Value is AdSecPointGoo) {
        ghType.CastTo(ref pointGoo);
      } else if (GH_Convert.ToPoint3d(ghType.Value, ref ghpt, GH_Conversion.Both)) {
        pointGoo = new AdSecPointGoo(ghpt);
      } else {
        castSuccessful = false;
      }

      return castSuccessful;
    }

    public static bool TryCastToAdSecProfileGoo(GH_ObjectWrapper ghType, ref AdSecProfileGoo profileGoo) {
      bool castSuccessful = true;
      if (ghType.Value is AdSecProfileGoo adsecGoo) {
        profileGoo = adsecGoo;
      } else if (ghType.Value is OasysProfileGoo oasysGoo) {
        var profile = AdSecProfiles.CreateProfile(oasysGoo.Value);
        profileGoo = new AdSecProfileGoo(profile, Plane.WorldYZ);
      } else {
        castSuccessful = false;
      }

      return castSuccessful;
    }

    public static bool TryCastToAdSecRebarBundleGoo(
      GH_ObjectWrapper ghType, ref AdSecRebarBundleGoo rebarBundleGoo, ref bool showRemark) {
      bool castSuccessful = true;
      if (ghType.Value is AdSecRebarBundleGoo goo) {
        rebarBundleGoo = goo;
      } else if (ghType.Value is AdSecRebarLayerGoo spacing) {
        rebarBundleGoo = new AdSecRebarBundleGoo(spacing.Value.BarBundle);
        showRemark = true;
      } else {
        castSuccessful = false;
      }

      return castSuccessful;
    }

    public static bool TryCastToAdSecRebarLayerGoo(GH_ObjectWrapper ghType, ref AdSecRebarLayerGoo spacing) {
      bool castSuccessful = true;
      if (ghType.Value is AdSecRebarLayerGoo goo) {
        spacing = goo;
      } else {
        castSuccessful = false;
      }

      return castSuccessful;
    }

    public static bool TryCastToIPoints(
      List<GH_ObjectWrapper> ghTypes, IList<IPoint> iPoints, List<int> invalidIds, ref int pointsConverted) {
      invalidIds = invalidIds ?? new List<int>();
      IPoint point = null;
      if (ghTypes == null || ghTypes.Count == 0) {
        return false;
      }

      var temporaryPoints = new List<Point3d>();

      for (int i = 0; i < ghTypes.Count; i++) {
        Curve curve = null;
        var ghpt = new Point3d();

        if (TryCastToIPoint(ghTypes[i], ref point)) {
          iPoints.Add(point);
        } else if (TryCastToPoint3d(ghTypes[i], ref ghpt)) {
          temporaryPoints.Add(ghpt);
        } else if (TryCastToCurve(ghTypes[i], ref curve)) {
          iPoints = AdSecPointGoo.PtsFromPolylineCurve((PolylineCurve)curve);
        } else {
          invalidIds.Add(i);
        }
      }

      ProcessTemporaryPoints(iPoints, temporaryPoints);

      pointsConverted = temporaryPoints.Count;
      return !invalidIds.Any();
    }

    public static void ProcessTemporaryPoints(IList<IPoint> iPoints, List<Point3d> temporaryPoints) {
      if (!temporaryPoints.Any()) {
        return;
      }

      if (temporaryPoints.Count == 1) {
        iPoints.Add(AdSecPointGoo.CreateFromPoint3d(temporaryPoints[0], Plane.WorldYZ));
      } else if (temporaryPoints.Count > 1) {
        Plane.FitPlaneToPoints(temporaryPoints, out var plane);
        foreach (var point in temporaryPoints) {
          iPoints.Add(AdSecPointGoo.CreateFromPoint3d(point, plane));
        }
      }
    }

    public static bool TryCastToPoint3d(GH_ObjectWrapper ghType, ref Point3d point) {
      return GH_Convert.ToPoint3d(ghType.Value, ref point, GH_Conversion.Both);
    }

    public static bool TryCastToCurve(GH_ObjectWrapper ghType, ref Curve curve) {
      return GH_Convert.ToCurve(ghType.Value, ref curve, GH_Conversion.Both);
    }

    public static bool TryCastToIPoint(GH_ObjectWrapper objectWrapper, ref IPoint iPoint) {
      switch (objectWrapper.Value) {
        case IPoint point:
          iPoint = point;
          break;
        case AdSecPointGoo adSecPointGoo:
          iPoint = adSecPointGoo.AdSecPoint;
          break;
        default: return false;
      }

      return true;
    }

    public static bool TryCastToILayers(List<GH_ObjectWrapper> ghTypes, IList<ILayer> iLayers, List<int> invalidIds) {
      invalidIds = invalidIds ?? new List<int>();
      ILayer layer = null;
      if (ghTypes == null || ghTypes.Count == 0) {
        return false;
      }

      for (int i = 0; i < ghTypes.Count; i++) {
        if (TryCastToILayer(ghTypes[i], ref layer)) {
          iLayers.Add(layer);
        } else {
          invalidIds.Add(i);
        }
      }

      return !invalidIds.Any();
    }

    public static bool TryCastToILayer(GH_ObjectWrapper objectWrapper, ref ILayer iLayer) {
      switch (objectWrapper.Value) {
        case ILayer layer:
          iLayer = layer;
          break;
        case AdSecRebarLayerGoo rebarGoo:
          iLayer = rebarGoo.Value;
          break;
        default: return false;
      }

      return true;
    }

    public static bool TryCastToStressStrainCurve(
      bool compression, GH_ObjectWrapper gh_typ, ref AdSecStressStrainCurveGoo curveGoo) {
      bool castSuccess = true;
      Curve polycurve = null;
      if (gh_typ.Value is AdSecStressStrainCurveGoo goo) {
        curveGoo = goo;
      } else if (GH_Convert.ToCurve(gh_typ.Value, ref polycurve, GH_Conversion.Both)) {
        var curve = (PolylineCurve)polycurve;
        var pts = AdSecStressStrainCurveGoo.StressStrainPtsFromPolyline(curve);
        var exCrv = IExplicitStressStrainCurve.Create();
        exCrv.Points = pts;
        var tuple = AdSecStressStrainCurveGoo.Create(exCrv, AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit,
          compression);
        curveGoo = new AdSecStressStrainCurveGoo(tuple.Item1, exCrv,
          AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit, tuple.Item2);
      } else {
        castSuccess = false;
      }

      return castSuccess;
    }

    public static bool TryCastToStressStrainPoint(GH_ObjectWrapper ghType, ref IStressStrainPoint stressStrainPoint) {
      bool castSuccessful = true;
      var point3d = new Point3d();
      if (ghType.Value is IStressStrainPoint point) {
        stressStrainPoint = point;
      } else if (ghType.Value is AdSecStressStrainPointGoo pointGoo) {
        stressStrainPoint = pointGoo.StressStrainPoint;
      } else if (GH_Convert.ToPoint3d(ghType.Value, ref point3d, GH_Conversion.Both)) {
        stressStrainPoint = AdSecStressStrainPointGoo.CreateFromPoint3d(point3d);
      } else {
        castSuccessful = false;
      }

      return castSuccessful;
    }

    public static bool TryCastToStressStrainPoints(
      List<GH_ObjectWrapper> ghTypes, ref Oasys.Collections.IList<IStressStrainPoint> points) {
      foreach (var ghType in ghTypes) {
        Curve polycurve = null;
        IStressStrainPoint stressStrainPoint = null;
        if (GH_Convert.ToCurve(ghType.Value, ref polycurve, GH_Conversion.Both)) {
          var curve = (PolylineCurve)polycurve;
          points = AdSecStressStrainCurveGoo.StressStrainPtsFromPolyline(curve);
        } else if (ghType.Value is IExplicitStressStrainCurve iCurve) {
          foreach (var strainPoint in iCurve.Points) {
            points.Add(strainPoint);
          }
        } else if (TryCastToStressStrainPoint(ghType, ref stressStrainPoint)) {
          points.Add(stressStrainPoint);
        } else {
          return false;
        }
      }

      return points?.Count > 0;
    }
  }
}
