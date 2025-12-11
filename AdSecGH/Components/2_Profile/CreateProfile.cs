using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using GH_IO.Types;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

using Oasys.GH.Helpers;
using Oasys.Profiles;
using Oasys.Taxonomy.Geometry;
using Oasys.Taxonomy.Profiles;

using OasysGH;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

using Rhino;
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
          var polylines = AdSecProfileGoo.PolylinesFromAdSecProfile(AdSecProfiles.CreateProfile(profile), localPlan);
          var outerPolyLine = polylines.Item1;

          var gh_typ = new GH_ObjectWrapper();
          if (DA.GetData(0, ref gh_typ)) {
            Brep brep = null;
            if (GH_Convert.ToBrep(gh_typ.Value, ref brep, GH_Conversion.Both)) {
              // get edge curves from brep
              Curve[] edgeSegments = brep.DuplicateEdgeCurves();
              Curve[] edges = Curve.JoinCurves(edgeSegments);

              if (!edges[0].TryGetPolyline(out Polyline tempCrv)) {
                throw new Exception("Data conversion failed to create a polyline from input geometry. Please input a polyline approximation of your Brep/outline.");
              }

              RhinoHelper.TryFitPlaneToPolyline(tempCrv, out Plane plane);

              plane = GetCorrectedPlane(plane);

              if (localPlan == Plane.Unset) {
                localPlan = plane;
              }

              Oasys.Taxonomy.Geometry.IPolygon perimeter = PolygonFromRhinoPolyline(tempCrv, _lengthUnit, plane);
              IList<Oasys.Taxonomy.Geometry.IPolygon> voidPolygons = new List<Oasys.Taxonomy.Geometry.IPolygon>();

              List<Point3d> ctrlPts = new List<Point3d>();
              if (edges.Length > 1) {
                for (int i = 1; i < edges.Length; i++) {
                  ctrlPts.Clear();
                  var voidpts = new List<Point3d>();
                  if (!edges[i].IsPlanar()) {
                    for (int j = 0; j < edges.Length; j++) {
                      edges[j] = Curve.ProjectToPlane(edges[j], plane);
                    }
                  }

                  if (edges[i].TryGetPolyline(out tempCrv)) {
                    voidPolygons.Add(PolygonFromRhinoPolyline(tempCrv, _lengthUnit, plane));
                  } else {
                    throw new Exception("Cannot convert internal edge to polyline.");
                  }
                }
              }
              profile = new PerimeterProfile(perimeter, voidPolygons);
              adSecProfile = AdSecProfiles.CreateProfile(profile);
            }

          }
        }
        DA.SetData(0, new AdSecProfileGoo(adSecProfile, localPlan));
      }
    }

    private Plane GetLocalPlane(IGH_DataAccess DA) {
      var localPlane = Plane.Unset;
      if (DA.GetData(Params.Input.Count - 1, ref localPlane)) {
        return localPlane;
      }
      return localPlane;
    }

    public static Oasys.Taxonomy.Geometry.IPolygon PolygonFromRhinoPolyline(Polyline polyline, LengthUnit lengthUnit, Plane LocalPlane) {
      var polygon = new Polygon() {
        Points = PointsFromRhinoPolyline(polyline, lengthUnit, LocalPlane)
      };
      return polygon;
    }

    public static List<IPoint2d> PointsFromRhinoPolyline(Polyline polyline, LengthUnit lengthUnit, Plane LocalPlane) {
      if (polyline.First() != polyline.Last()) {
        polyline.Add(polyline.First());
      }

      var points = new List<IPoint2d>();

      var xform = Rhino.Geometry.Transform.PlaneToPlane(LocalPlane, Plane.WorldXY);

      for (int i = 0; i < polyline.Count - 1; i++) {
        Point3d point3d = polyline[i];
        point3d.Transform(xform);

        IPoint2d point2d = new Oasys.Taxonomy.Geometry.Point2d(
            new Length(point3d.X, lengthUnit),
            new Length(point3d.Y, lengthUnit));
        points.Add(point2d);
      }

      return points;
    }

    private static Plane GetCorrectedPlane(Plane LocalPlane) {
      Vector3d xAxis = LocalPlane.XAxis;
      Vector3d yAxis = LocalPlane.YAxis;
      Vector3d zAxis = Vector3d.CrossProduct(xAxis, yAxis);

      if (zAxis.Z < 0) {
        // flip Y-axis as well to ensure right-hand rule and positive Z
        yAxis = -yAxis;
        zAxis = -zAxis;
      }

      if (xAxis.X < 0) {
        xAxis = -xAxis;
      }

      if (yAxis.Y < 0) {
        yAxis = -yAxis;
      }

      var correctedPlane = new Plane(LocalPlane.Origin, xAxis, yAxis);
      return correctedPlane;
    }
  }
}
