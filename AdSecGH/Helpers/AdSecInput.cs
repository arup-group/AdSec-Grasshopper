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
using OasysGH.Units;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

namespace AdSecGH.Helpers {
  internal static class AdSecInput {

    internal static AdSecDesignCode AdSecDesignCode(
      GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      var designCode = new AdSecDesignCode();
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ)) {
        if (gh_typ.Value is AdSecDesignCodeGoo) {
          gh_typ.CastTo(ref designCode);
        } else {
          owner.AddRuntimeError("Unable to convert " + owner.Params.Input[inputid].NickName + " to DesignCode");
          return null;
        }

        return designCode;
      }

      if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    internal static AdSecMaterial AdSecMaterial(
      GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      var material = new AdSecMaterial();
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ)) {
        if (gh_typ.Value is AdSecMaterialGoo) {
          gh_typ.CastTo(ref material);
        } else {
          owner.AddRuntimeError("Unable to convert " + owner.Params.Input[inputid].NickName + " to an AdSec Material");
        }

        return material;
      }

      if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    internal static AdSecPointGoo AdSecPointGoo(
      GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      AdSecPointGoo pt = null;
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ)) {
        var ghpt = new Point3d();
        if (gh_typ.Value is AdSecPointGoo) {
          gh_typ.CastTo(ref pt);
        } else if (GH_Convert.ToPoint3d(gh_typ.Value, ref ghpt, GH_Conversion.Both)) {
          pt = new AdSecPointGoo(ghpt);
        } else {
          owner.AddRuntimeError("Unable to convert " + owner.Params.Input[inputid].NickName + " to an Vertex Point");
        }

        return pt;
      }

      if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      } else if (isOptional) {
        return new AdSecPointGoo(Oasys.Profiles.IPoint.Create(new Length(0, DefaultUnits.LengthUnitGeometry),
          new Length(0, DefaultUnits.LengthUnitGeometry)));
      }

      return null;
    }

    internal static AdSecProfileGoo AdSecProfileGoo(
      GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      AdSecProfileGoo profileGoo = null;
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ)) {
        if (gh_typ.Value is AdSecProfileGoo adsecGoo) {
          profileGoo = adsecGoo;
        } else if (gh_typ.Value is OasysProfileGoo oasysGoo) {
          var profile = AdSecProfiles.CreateProfile(oasysGoo.Value);
          profileGoo = new AdSecProfileGoo(profile, Plane.WorldYZ);
        } else {
          return null;
        }

        return profileGoo;
      }

      if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    internal static AdSecRebarBundleGoo AdSecRebarBundleGoo(
      GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      AdSecRebarBundleGoo rebar = null;
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ)) {
        if (gh_typ.Value is AdSecRebarBundleGoo goo) {
          rebar = goo;
        } else if (gh_typ.Value is AdSecRebarLayerGoo spacing) {
          rebar = new AdSecRebarBundleGoo(spacing.Value.BarBundle);
          owner.AddRuntimeRemark("Converted " + owner.Params.Input[inputid].NickName
            + " from RebarSpacing to an AdSec Rebar. All spacing information has been lost!");
        } else {
          owner.AddRuntimeError("Unable to convert " + owner.Params.Input[inputid].NickName + " to Rebar");
        }

        return rebar;
      }

      if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    internal static AdSecRebarLayerGoo AdSecRebarLayerGoo(
      GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      AdSecRebarLayerGoo spacing = null;
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ)) {
        if (gh_typ.Value is AdSecRebarLayerGoo goo) {
          spacing = goo;
        } else {
          owner.AddRuntimeError("Unable to convert " + owner.Params.Input[inputid].NickName + " to RebarSpacing");
        }

        return spacing;
      }

      if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    internal static AdSecSection AdSecSection(
      GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ)) {
        if (gh_typ.Value is AdSecSectionGoo a1) {
          return a1.Value;
        }

        if (gh_typ.Value is AdSecSubComponentGoo a2) {
          return a2._section;
        }

        owner.AddRuntimeError("Unable to convert " + owner.Params.Input[inputid].NickName + " to Section");
        return null;
      }

      if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    internal static List<AdSecSection> AdSecSections(
      GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      var subs = new List<AdSecSection>();
      var gh_typs = new List<GH_ObjectWrapper>();
      if (DA.GetDataList(inputid, gh_typs)) {
        for (int i = 0; i < gh_typs.Count; i++) {
          if (gh_typs[i].Value is AdSecSection section) {
            subs.Add(section);
          } else if (gh_typs[i].Value is AdSecSectionGoo subcomp) {
            subs.Add(subcomp.Value);
          } else {
            owner.AddRuntimeWarning("Unable to convert " + owner.Params.Input[inputid].NickName + " (item " + i
              + ") to Section");
          }
        }

        return subs;
      }

      if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    internal static IConcreteCrackCalculationParameters ConcreteCrackCalculationParameters(
      GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      IConcreteCrackCalculationParameters concreteCrack = null;

      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ)) {
        if (gh_typ.Value is IConcreteCrackCalculationParameters parameters) {
          concreteCrack = parameters;
        } else if (gh_typ.Value is AdSecConcreteCrackCalculationParametersGoo adsecccp) {
          concreteCrack = adsecccp.Value;
        } else {
          owner.AddRuntimeError("Unable to convert " + owner.Params.Input[inputid].NickName
            + " to ConcreteCrackCalculationParameters");
          return null;
        }

        return concreteCrack;
      }

      if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    internal static List<ICover> Covers(GH_Component owner, IGH_DataAccess DA, int inputid, LengthUnit docLengthUnit) {
      var covers = new List<ICover>();
      var lengths = Input.UnitNumberList(owner, DA, inputid, docLengthUnit);

      foreach (var length in lengths.Select(v => (Length)v)) {
        covers.Add(ICover.Create(length));
      }

      return covers;
    }

    internal static IFlange Flange(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      AdSecProfileFlangeGoo flange = null;
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ)) {
        if (gh_typ.Value is AdSecProfileFlangeGoo goo) {
          flange = goo;
          return flange.Value;
        }

        if (gh_typ.Value is AdSecProfileWebGoo) {
          owner.AddRuntimeError("Unable to convert " + owner.Params.Input[inputid].NickName + " from Web to Flange");
        } else {
          owner.AddRuntimeError("Unable to convert " + owner.Params.Input[inputid].NickName + " to Flange");
          return null;
        }
      } else if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    internal static IBarBundle IBarBundle(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      return AdSecRebarBundleGoo(owner, DA, inputid, isOptional).Value;
    }

    internal static ILayer ILayer(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      return AdSecRebarLayerGoo(owner, DA, inputid, isOptional).Value;
    }

    internal static Oasys.Collections.IList<ILayer> ILayers(
      GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      var grps = Oasys.Collections.IList<ILayer>.Create();
      var gh_typs = new List<GH_ObjectWrapper>();
      if (DA.GetDataList(inputid, gh_typs)) {
        for (int i = 0; i < gh_typs.Count; i++) {
          if (gh_typs[i].Value is ILayer layer) {
            grps.Add(layer);
          } else if (gh_typs[i].Value is AdSecRebarLayerGoo rebarGoo) {
            grps.Add(rebarGoo.Value);
          } else {
            owner.AddRuntimeWarning("Unable to convert " + owner.Params.Input[inputid].NickName + " (item " + i
              + ") to RebarLayer");
          }
        }

        return grps;
      }

      if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    internal static IPoint IPoint(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      var pt = AdSecPointGoo(owner, DA, inputid, isOptional);
      if (pt == null) {
        return null;
      }

      return pt.AdSecPoint;
    }

    internal static Oasys.Collections.IList<IPoint> IPoints(
      GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      var pts = Oasys.Collections.IList<IPoint>.Create();
      var gh_typs = new List<GH_ObjectWrapper>();
      if (DA.GetDataList(inputid, gh_typs)) {
        var tempPts = new List<Point3d>();
        for (int i = 0; i < gh_typs.Count; i++) {
          Curve polycurve = null;
          var ghpt = new Point3d();

          if (gh_typs[i].Value is IPoint point) {
            pts.Add(point);
          } else if (gh_typs[i].Value is AdSecPointGoo sspt) {
            pts.Add(sspt.AdSecPoint);
          } else if (GH_Convert.ToPoint3d(gh_typs[i].Value, ref ghpt, GH_Conversion.Both)) {
            tempPts.Add(ghpt);
          } else if (GH_Convert.ToCurve(gh_typs[i].Value, ref polycurve, GH_Conversion.Both)) {
            var curve = (PolylineCurve)polycurve;
            pts = Parameters.AdSecPointGoo.PtsFromPolylineCurve(curve);
          } else {
            owner.AddRuntimeWarning("Unable to convert " + owner.Params.Input[inputid].NickName + " (item " + i
              + ") to StressStrainPoint or Polyline");
          }
        }

        if (tempPts.Count > 0) {
          if (tempPts.Count == 1) {
            pts.Add(Parameters.AdSecPointGoo.CreateFromPoint3d(tempPts[0], Plane.WorldYZ));
            owner.AddRuntimeRemark(
              "Single Point converted to local point. Assumed that local coordinate system is in a YZ-Plane");
          } else {
            Plane.FitPlaneToPoints(tempPts, out var plane);
            foreach (var pt in tempPts) {
              pts.Add(Parameters.AdSecPointGoo.CreateFromPoint3d(pt, plane));
            }

            owner.AddRuntimeRemark(
              "List of Points have been converted to local points. Assumed that local coordinate system is matching best-fit plane through points");
          }
        }

        return pts;
      }

      if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    internal static AdSecRebarGroupGoo ReinforcementGroup(
      GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ)) {
        if (gh_typ.Value is AdSecRebarGroupGoo goo) {
          return goo;
        }

        owner.AddRuntimeError("Unable to convert " + owner.Params.Input[inputid].NickName + " to RebarLayout");
        return null;
      }

      if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    internal static List<AdSecRebarGroup> ReinforcementGroups(
      GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      var grps = new List<AdSecRebarGroup>();
      var gh_typs = new List<GH_ObjectWrapper>();
      if (DA.GetDataList(inputid, gh_typs)) {
        for (int i = 0; i < gh_typs.Count; i++) {
          if (gh_typs[i].Value is IGroup group) {
            grps.Add(new AdSecRebarGroup(group));
          } else if (gh_typs[i].Value is AdSecRebarGroupGoo rebarGoo) {
            grps.Add(rebarGoo.Value);
          } else {
            owner.AddRuntimeWarning("Unable to convert " + owner.Params.Input[inputid].NickName + " (item " + i
              + ") to RebarGroup");
          }
        }

        return grps;
      }

      if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    internal static AdSecSolutionGoo Solution(
      GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ)) {
        if (gh_typ.Value is AdSecSolutionGoo goo) {
          return goo;
        }

        owner.AddRuntimeError("Unable to convert " + owner.Params.Input[inputid].NickName + " to AdSec Results");
        return null;
      }

      if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    internal static AdSecStressStrainCurveGoo StressStrainCurveGoo(
      GH_Component owner, IGH_DataAccess DA, int inputid, bool compression, bool isOptional = false) {
      AdSecStressStrainCurveGoo ssCrv = null;
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ)) {
        return TryCastToStressStrainCurve(owner, inputid, compression, gh_typ);
      }

      if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    public static AdSecStressStrainCurveGoo TryCastToStressStrainCurve(
      GH_Component owner, int inputid, bool compression, GH_ObjectWrapper gh_typ) {
      AdSecStressStrainCurveGoo ssCrv;
      Curve polycurve = null;
      if (gh_typ.Value is AdSecStressStrainCurveGoo goo) {
        ssCrv = goo;
      } else if (GH_Convert.ToCurve(gh_typ.Value, ref polycurve, GH_Conversion.Both)) {
        var curve = (PolylineCurve)polycurve;
        var pts = AdSecStressStrainCurveGoo.StressStrainPtsFromPolyline(curve);
        var exCrv = IExplicitStressStrainCurve.Create();
        exCrv.Points = pts;
        var tuple = AdSecStressStrainCurveGoo.Create(exCrv, AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit,
          compression);
        ssCrv = new AdSecStressStrainCurveGoo(tuple.Item1, exCrv,
          AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit, tuple.Item2);
      } else {
        owner.AddRuntimeError("Unable to convert " + owner.Params.Input[inputid].NickName + " to StressStrainCurve");
        return null;
      }

      return ssCrv;
    }

    internal static IStressStrainPoint StressStrainPoint(
      GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      IStressStrainPoint pt1 = null;
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ)) {
        var ghpt = new Point3d();
        if (gh_typ.Value is IStressStrainPoint point) {
          pt1 = point;
        } else if (gh_typ.Value is AdSecStressStrainPointGoo sspt) {
          pt1 = sspt.StressStrainPoint;
        } else if (GH_Convert.ToPoint3d(gh_typ.Value, ref ghpt, GH_Conversion.Both)) {
          pt1 = AdSecStressStrainPointGoo.CreateFromPoint3d(ghpt);
        } else {
          owner.AddRuntimeError("Unable to convert " + owner.Params.Input[inputid].NickName + " to StressStrainPoint");
          return null;
        }

        return pt1;
      }

      if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    internal static Oasys.Collections.IList<IStressStrainPoint> StressStrainPoints(
      GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      var pts = Oasys.Collections.IList<IStressStrainPoint>.Create();
      var gh_typs = new List<GH_ObjectWrapper>();
      if (DA.GetDataList(inputid, gh_typs)) {
        for (int i = 0; i < gh_typs.Count; i++) {
          Curve polycurve = null;
          var ghpt = new Point3d();
          if (gh_typs[i].Value is IStressStrainPoint point) {
            pts.Add(point);
          } else if (gh_typs[i].Value is AdSecStressStrainPointGoo sspt) {
            pts.Add(sspt.StressStrainPoint);
          } else if (GH_Convert.ToPoint3d(gh_typs[i].Value, ref ghpt, GH_Conversion.Both)) {
            pts.Add(AdSecStressStrainPointGoo.CreateFromPoint3d(ghpt));
          } else if (GH_Convert.ToCurve(gh_typs[i].Value, ref polycurve, GH_Conversion.Both)) {
            var curve = (PolylineCurve)polycurve;
            pts = AdSecStressStrainCurveGoo.StressStrainPtsFromPolyline(curve);
          } else {
            owner.AddRuntimeWarning("Unable to convert " + owner.Params.Input[inputid].NickName
              + " to StressStrainPoint or a Polyline");
          }
        }

        if (pts.Count < 2) {
          owner.AddRuntimeWarning("Input must contain at least 2 points to create an Explicit Stress Strain Curve");
          return null;
        }

        return pts;
      }

      if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    internal static Oasys.Collections.IList<ISubComponent> SubComponents(
      GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      var subs = Oasys.Collections.IList<ISubComponent>.Create();
      var gh_typs = new List<GH_ObjectWrapper>();
      if (DA.GetDataList(inputid, gh_typs)) {
        for (int i = 0; i < gh_typs.Count; i++) {
          if (gh_typs[i].Value is ISubComponent component) {
            subs.Add(component);
          } else if (gh_typs[i].Value is AdSecSubComponentGoo subcomp) {
            subs.Add(subcomp.Value);
          } else if (gh_typs[i].Value is AdSecSectionGoo section) {
            var offset = Oasys.Profiles.IPoint.Create(Length.Zero, Length.Zero);
            var sub = ISubComponent.Create(section.Value.Section, offset);
            subs.Add(sub);
          } else {
            owner.AddRuntimeWarning("Unable to convert " + owner.Params.Input[inputid].NickName + " (item " + i
              + ") to SubComponent or Section");
          }
        }

        return subs;
      }

      if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    internal static IWeb Web(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false) {
      AdSecProfileWebGoo web = null;
      var gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ)) {
        if (gh_typ.Value is AdSecProfileWebGoo goo) {
          web = goo;
          return web.Value;
        }

        if (gh_typ.Value is AdSecProfileFlangeGoo) {
          owner.AddRuntimeError("Unable to convert " + owner.Params.Input[inputid].NickName + " from Flange to Web");
        } else {
          owner.AddRuntimeError("Unable to convert " + owner.Params.Input[inputid].NickName + " to Web");
          return null;
        }
      } else if (!isOptional) {
        ShowWarningForOptionalParam(owner, inputid);
      }

      return null;
    }

    private static void ShowWarningForOptionalParam(IGH_Component owner, int inputid) {
      owner.AddRuntimeWarning($"Input parameter {owner.Params.Input[inputid].NickName} failed to collect data!");
    }

    private static void ShowConvertToError(string desiredType, IGH_Component owner, int inputid) {
      ShowConvertFromToError(string.Empty, desiredType, owner, inputid);
    }

    private static void ShowConvertFromToError(string fromType, string desiredType, IGH_Component owner, int inputid) {
      string fromObjectString = string.IsNullOrEmpty(fromType) ? $"from {fromType} " : string.Empty;
      owner.AddRuntimeError(
        $"Unable to convert {owner.Params.Input[inputid].NickName} {fromObjectString}to {desiredType}");
    }
  }
}
