using System;
using System.Drawing;
using System.IO;
using System.Linq;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

using Oasys.GH.Helpers;
using Oasys.Profiles;

using OasysGH;
using OasysGH.Units;

using Rhino.Geometry;

namespace AdSecGH.Components {
  /// <summary>
  ///   Component to create AdSec profile
  /// </summary>
  public class CreateProfile : ProfileAdapter<CreateProfileFunction> {

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("ea0741e5-905e-4ecb-8270-a584e3f99aa3");
    public override string DataSource => Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.CreateProfile;

    protected override string HtmlHelp_Source() {
      string help = "GOTO:https://arup-group.github.io/oasys-combined/adsec-api/api/Oasys.Profiles.html";
      return help;
    }

    protected override void Mode1Clicked() {
      // remove plane
      var plane = Params.Input[Params.Input.Count - 1];
      Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1], false);

      // remove input parameters
      while (Params.Input.Count > 0) {
        Params.UnregisterInputParameter(Params.Input[0], true);
      }

      // register input parameter
      Params.RegisterInputParam(new Param_String());
      Params.RegisterInputParam(new Param_Boolean());

      // add plane
      Params.RegisterInputParam(plane);

      _mode = FoldMode.Catalogue;

      base.UpdateUI();
    }

    protected override void Mode2Clicked() {
      var plane = Params.Input[Params.Input.Count - 1];
      // remove plane
      Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1], false);

      // check if mode is correct
      if (_mode != FoldMode.Other) {
        // if we come from catalogue mode remove all input parameters
        while (Params.Input.Count > 0) {
          Params.UnregisterInputParameter(Params.Input[0], true);
        }

        // set mode to other
        _mode = FoldMode.Other;
      }

      UpdateParameters();

      // add plane
      Params.RegisterInputParam(plane);

      (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
      Params.OnParametersChanged();
      ExpireSolution(true);
    }

    protected override void SolveInternal(IGH_DataAccess DA) {
      ClearRuntimeMessages();
      Params.Input.ForEach(input => input.ClearRuntimeMessages());

      var localPlan = GetLocalPlane(DA);

      if (_mode == FoldMode.Catalogue) {
        var profiles = SolveInstanceForCatalogueProfile(DA);
        var adSecProfile = AdSecProfiles.CreateProfile(profiles[0]);
        DA.SetData(0, new AdSecProfileGoo(adSecProfile, localPlan));
      } else if (_mode == FoldMode.Other) {
        var profile = SolveInstanceForStandardProfile(DA);
        var adSecProfile = AdSecProfiles.CreateProfile(profile);
        if (profile.ProfileType == Oasys.Taxonomy.Profiles.ProfileType.Perimeter) {
          //AdSec API makes polyline anticlockwise and gives coordinate relative to its own centre
          //So, we need to shift it back to the original base curve origin
          var polylines = AdSecProfileGoo.PolylinesFromAdSecProfile(AdSecProfiles.CreateProfile(profile), localPlan);
          var outerPolyLine = polylines.Item1;
          if (TryGetCurveCentreFromInput(DA, out Polyline basePolyLine)) {
            if (IsClockwise(basePolyLine)) {
              AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid curve orientation detected. Perimeter profiles expect anti-clockwise curves. Please use the Flip Curve component to correct the orientation.");
              return;
            }
            var baseCurveOrigin = basePolyLine.CenterPoint();
            outerPolyLine = ShiftPolyLineGivenByAdSecApiRelativeToTheOriginOfBasePolyLine(baseCurveOrigin, outerPolyLine);
            for (int i = 0; i < polylines.Item2.Count; i++) {
              var voidPolyLine = polylines.Item2[i];
              voidPolyLine = ShiftPolyLineGivenByAdSecApiRelativeToTheOriginOfBasePolyLine(baseCurveOrigin, voidPolyLine);
              polylines.Item2[i] = voidPolyLine;
            }
            adSecProfile = BuildPerimeterProfile(polylines, outerPolyLine);
          }
        }
        DA.SetData(0, new AdSecProfileGoo(adSecProfile, localPlan));
      }
    }

    public static bool IsClockwise(Polyline polyline) {
      double signedArea = 0;
      for (int i = 0; i < polyline.Count - 1; i++) {
        Point3d p1 = polyline[i];
        Point3d p2 = polyline[i + 1];
        signedArea += (p2.X - p1.X) * (p2.Y + p1.Y);
      }
      return signedArea > 0;
    }

    private Plane GetLocalPlane(IGH_DataAccess DA) {
      var localPlane = Plane.WorldYZ;
      var tempPlane = Plane.Unset;

      if (DA.GetData(Params.Input.Count - 1, ref tempPlane)) {
        localPlane = tempPlane;
      }

      return localPlane;
    }

    private static IProfile BuildPerimeterProfile(Tuple<Polyline, System.Collections.Generic.List<Polyline>> polylines, Polyline outerPolyLine) {
      var adSecProfile = IPerimeterProfile.Create();
      Plane.FitPlaneToPoints(outerPolyLine.ToList(), out var plane);
      adSecProfile.SolidPolygon = AdSecProfileGoo.PolygonFromRhinoPolyline(outerPolyLine, DefaultUnits.LengthUnitGeometry, plane);

      for (int i = 0; i < polylines.Item2.Count; i++) {
        var voidPolyLine = polylines.Item2[i];
        var voidPolygon = AdSecProfileGoo.PolygonFromRhinoPolyline(voidPolyLine, DefaultUnits.LengthUnitGeometry, plane);
        adSecProfile.VoidPolygons.Add(voidPolygon);
      }
      return adSecProfile;
    }

    private static Polyline ShiftPolyLineGivenByAdSecApiRelativeToTheOriginOfBasePolyLine(Point3d baseCurveOrigin, Polyline solid) {
      if (!solid.CenterPoint().Equals(baseCurveOrigin)) {
        Vector3d offset = baseCurveOrigin - solid.CenterPoint();
        for (int i = 0; i < solid.Count; i++) {
          solid[i] = solid[i] + offset;
        }
      }
      return solid;
    }

    private static bool TryGetCurveCentreFromInput(IGH_DataAccess DA, out Polyline basePolyLine) {
      Brep brep = null;
      var gh_typ = new GH_ObjectWrapper();
      basePolyLine = new Polyline();
      if (DA.GetData(0, ref gh_typ) &&
             GH_Convert.ToBrep(gh_typ.Value, ref brep, GH_Conversion.Both)) {
        var edgeSegments = brep.DuplicateEdgeCurves();
        var edges = Curve.JoinCurves(edgeSegments);
        if (edges.Length > 0 && edges[0].TryGetPolyline(out basePolyLine)) {
          return true;
        }
      }
      return false;
    }
  }
}
