using System;
using System.Collections.Generic;
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
using Oasys.Taxonomy.Geometry;
using Oasys.Taxonomy.Profiles;

using OasysGH;

using OasysUnits;
using OasysUnits.Units;

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

      var local = GetLocalPlane(DA, Plane.WorldYZ);
      Plane globalPlane = Plane.WorldYZ;
      if (_mode == FoldMode.Catalogue) {
        var profiles = SolveInstanceForCatalogueProfile(DA);
        var adSecProfile = AdSecProfiles.CreateProfile(profiles[0]);
        DA.SetData(0, new AdSecProfileGoo(adSecProfile, globalPlane, local));
      } else if (_mode == FoldMode.Other) {
        var profile = SolveInstanceForStandardProfile(DA);
        if (profile.ProfileType == Oasys.Taxonomy.Profiles.ProfileType.Perimeter) {
          local = GetLocalPlane(DA, Plane.Unset);
          var gh_typ = new GH_ObjectWrapper();
          if (DA.GetData(0, ref gh_typ)) {
            Brep brep = null;
            Curve crv = null;
            if (GH_Convert.ToBrep(gh_typ.Value, ref brep, GH_Conversion.Both)) {
              BrepPolylineResult brepInfo = PolyLineFromBrep(brep);
              globalPlane = brepInfo.Plane;
              IPolygon perimeter = PolygonFromRhinoPolyline(brepInfo.Boundary, _lengthUnit, globalPlane);

              IList<IPolygon> voidPolygons = new List<IPolygon>();
              foreach (Polyline voids in brepInfo.Voids) {
                voidPolygons.Add(PolygonFromRhinoPolyline(voids, _lengthUnit, globalPlane));
              }

              profile = new PerimeterProfile(perimeter, voidPolygons);

            } else if (GH_Convert.ToCurve(gh_typ.Value, ref crv, GH_Conversion.Both)) {
              if (crv.TryGetPolyline(out Polyline solid)) {
                // get local plane
                Plane.FitPlaneToPoints(solid.ToList(), out globalPlane);

                IPolygon perimeter = PolygonFromRhinoPolyline(solid, _lengthUnit, globalPlane);
                IList<IPolygon> voidPolygons = new List<IPolygon>();

                profile = new PerimeterProfile(perimeter, voidPolygons);

              }
            }

          }
        }
        var adSecProfile = AdSecProfiles.CreateProfile(profile);
        DA.SetData(0, new AdSecProfileGoo(adSecProfile, globalPlane, local));
      }
    }

    public static List<IPoint2d> PointsFromRhinoPolyline(Polyline polyline, LengthUnit lengthUnit, Plane local) {
      if (polyline.First() != polyline.Last()) {
        polyline.Add(polyline.First());
      }

      var points = new List<IPoint2d>();

      // map points to XY plane so we can create local points from x and y coordinates
      var xform = Transform.PlaneToPlane(local, Plane.WorldXY);

      for (int i = 0; i < polyline.Count - 1; i++)
      // -1 on count because the profile is always closed and thus doesn�t
      // need the last point being equal to first as a rhino polyline needs
      {
        Point3d point3d = polyline[i];
        point3d.Transform(xform);
        IPoint2d point2d = new Oasys.Taxonomy.Geometry.Point2d(
          new Length(point3d.X, lengthUnit),
          new Length(point3d.Y, lengthUnit));
        points.Add(point2d);
      }

      return points;
    }

    public static IPolygon PolygonFromRhinoPolyline(Polyline polyline, LengthUnit lengthUnit, Plane local) {
      var polygon = new Polygon() {
        Points = PointsFromRhinoPolyline(polyline, lengthUnit, local)
      };
      return polygon;
    }


    public static BrepPolylineResult PolyLineFromBrep(Brep brep) {

      BrepFace mainFace = brep.Faces.OrderByDescending(face => {
        BoundingBox bbox = face.GetBoundingBox(true);
        return bbox.Area;
      }).FirstOrDefault();

      if (!mainFace.OuterLoop.To3dCurve().TryGetPolyline(out Polyline polyline)) {
        throw new Exception("Cannot extract polyline from Brep surface.");
      }

      List<Polyline> voids = ExtractInnerVoids(mainFace);

      Plane plane = PlaneFromFace(mainFace);

      return new BrepPolylineResult(polyline, voids, plane);
    }

    private static Plane PlaneFromFace(BrepFace mainFace) {

      var b = mainFace.TryGetPlane(out Plane plane);
      // planer normal should point upwards
      // for consistent profile creation
      if (plane.Normal.Z < 0) {
        plane = new Plane(plane.Origin, -plane.Normal);
      }
      return plane;
    }

    private static List<Polyline> ExtractInnerVoids(BrepFace mainFace) {
      var voids = new List<Polyline>();
      foreach (BrepLoop loop in mainFace.Loops) {
        if (loop.LoopType == BrepLoopType.Inner) {
          Curve voidCurve = loop.To3dCurve();
          if (voidCurve.TryGetPolyline(out Polyline voidPolyline)) {
            voids.Add(voidPolyline);
          } else {
            throw new Exception("Cannot extract polyline from Brep inner loop.");
          }
        }
      }
      return voids;
    }

    public readonly struct BrepPolylineResult {
      public Polyline Boundary { get; }
      public List<Polyline> Voids { get; }
      public Plane Plane { get; }

      public BrepPolylineResult(Polyline boundary, List<Polyline> voids, Plane plane) {
        Boundary = boundary;
        Voids = voids ?? new List<Polyline>();
        Plane = plane;
      }
    }

    private Plane GetLocalPlane(IGH_DataAccess DA, Plane plane) {
      var localPlane = plane;
      if (DA.GetData(Params.Input.Count - 1, ref localPlane)) {
        return localPlane;
      }
      return localPlane;
    }

  }
}
