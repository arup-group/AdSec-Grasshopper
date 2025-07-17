using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecCore;
using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.UI;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.Profiles;

using OasysGH.Units;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecProfileGoo : GH_GeometricGoo<ProfileDesign>, IGH_PreviewData {
    public override BoundingBox Boundingbox => Polyline.BoundingBox;
    public BoundingBox ClippingBox => Boundingbox;
    public bool IsReflectedY {
      get => Profile.IsReflectedY;
      set {
        Profile = Clone();
        Profile.IsReflectedY = value;
      }
    }
    public bool IsReflectedZ {
      get => Profile.IsReflectedZ;
      set {
        Profile = Clone();
        Profile.IsReflectedZ = value;
      }
    }
    public Plane LocalPlane => _plane;
    public IProfile Profile { get; private set; }
    public Angle Rotation {
      get => Profile.Rotation;
      set {
        Profile = Clone();
        Profile.Rotation = value;
      }
    }
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "Profile";
    public List<Polyline> VoidEdges { get; private set; }
    public Polyline Polyline { get; private set; }
    private Plane _plane;
    private Line previewXaxis;
    private Line previewYaxis;
    private Line previewZaxis;

    public AdSecProfileGoo(ProfileDesign profileDesign) : base(profileDesign) {
      if (profileDesign == null) {
        const string error = "ProfileDesign cannot be null";
        throw new ArgumentNullException(nameof(profileDesign), error);
      }

      Profile = profileDesign.Profile;
      _plane = profileDesign.LocalPlane.ToGh();
      var edges = PolylinesFromAdSecProfile(Profile, _plane);
      Polyline = edges.Item1;
      VoidEdges = edges.Item2;
      UpdatePreview();
    }

    public AdSecProfileGoo(IProfile profile, Plane local) {
      if (profile == null) {
        const string error = "Profile cannot be null";
        throw new ArgumentNullException(nameof(profile), error);
      }

      Value = new ProfileDesign() {
        Profile = profile,
        LocalPlane = local.ToOasys(),
      };
      Profile = profile;
      var edges = PolylinesFromAdSecProfile(profile, local);
      Polyline = edges.Item1;
      VoidEdges = edges.Item2;
      _plane = local;
      UpdatePreview();
    }

    public AdSecProfileGoo(Polyline polygon, LengthUnit lengthUnit) {
      if (polygon == null) {
        const string error = "Polygon cannot be null";
        throw new ArgumentNullException(nameof(polygon), error);
      }

      var perimprofile = IPerimeterProfile.Create();
      Plane.FitPlaneToPoints(polygon.ToList(), out var plane);

      perimprofile.SolidPolygon = PolygonFromRhinoPolyline(polygon, lengthUnit, plane);
      Profile = perimprofile;
      VoidEdges = null;
      _plane = plane;
      Profile = perimprofile;

      Value = new ProfileDesign() {
        Profile = Profile,
        LocalPlane = plane.ToOasys(),
      };
      UpdatePreview();
    }

    public void DrawViewportMeshes(GH_PreviewMeshArgs args) { }

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      if (Value == null) {
        return;
      }

      var defaultColor = Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
      if (args.Color.IsRgbEqualTo(defaultColor)) {
        args.Pipeline.DrawPolyline(Polyline, Colour.OasysBlue, 2);
        if (VoidEdges != null) {
          foreach (var polyline in VoidEdges) {
            args.Pipeline.DrawPolyline(polyline, Colour.OasysBlue, 1);
          }
        }
      } else {
        args.Pipeline.DrawPolyline(Polyline, Colour.OasysYellow, 3);
        if (VoidEdges != null) {
          foreach (var polyline in VoidEdges) {
            args.Pipeline.DrawPolyline(polyline, Colour.OasysYellow, 2);
          }
        }
      }

      DrawingHelper.DrawLocalAxis(args, previewZaxis, previewXaxis, previewYaxis);
    }

    public override bool CastFrom(object source) {
      if (source == null) {
        return false;
      }

      // try cast using GH_Convert, if that doesnt work we are doomed
      Curve curve = null;
      if (!GH_Convert.ToCurve(source, ref curve, GH_Conversion.Both) || !curve.TryGetPolyline(out var poly)) {
        return false;
      }

      var adSecProfileGoo = new AdSecProfileGoo(poly, DefaultUnits.LengthUnitGeometry);
      m_value = adSecProfileGoo.m_value;
      Profile = adSecProfileGoo.Profile;
      VoidEdges = adSecProfileGoo.VoidEdges;
      return true;
    }

    public override bool CastTo<Q>(out Q target) {
      if (typeof(Q).IsAssignableFrom(typeof(AdSecProfileGoo))) {
        target = (Q)(object)this;
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(Line))) {
        target = (Q)(object)Value;
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Curve))) {
        target = (Q)(object)new GH_Curve(new PolylineCurve(Polyline));
        return true;
      }

      target = default;
      return false;
    }

    public IProfile Clone() {
      IProfile duplicated = null;

      if (Profile.GetType().ToString().Equals($"{typeof(IAngleProfile)}_Implementation")) {
        var angle = (IAngleProfile)Profile;
        duplicated = IAngleProfile.Create(angle.Depth, angle.Flange, angle.Web);
      } else if (Profile.GetType().ToString().Equals($"{typeof(ICatalogueProfile)}_Implementation")) {
        duplicated = ICatalogueProfile.Create(Profile.Description());
      } else if (Profile.GetType().ToString().Equals($"{typeof(IChannelProfile)}_Implementation")) {
        var channel = (IChannelProfile)Profile;
        duplicated = IChannelProfile.Create(channel.Depth, channel.Flanges, channel.Web);
      } else if (Profile.GetType().ToString().Equals($"{typeof(ICircleHollowProfile)}_Implementation")) {
        var circleHollow = (ICircleHollowProfile)Profile;
        duplicated = ICircleHollowProfile.Create(circleHollow.Diameter, circleHollow.WallThickness);
      } else if (Profile.GetType().ToString().Equals($"{typeof(ICircleProfile)}_Implementation")) {
        var circle = (ICircleProfile)Profile;
        duplicated = ICircleProfile.Create(circle.Diameter);
      } else if (Profile.GetType().ToString().Equals($"{typeof(ICruciformSymmetricalProfile)}_Implementation")) {
        var cruciformSymmetrical = (ICruciformSymmetricalProfile)Profile;
        duplicated = ICruciformSymmetricalProfile.Create(cruciformSymmetrical.Depth, cruciformSymmetrical.Flange,
          cruciformSymmetrical.Web);
      } else if (Profile.GetType().ToString().Equals($"{typeof(IEllipseHollowProfile)}_Implementation")) {
        var ellipseHollow = (IEllipseHollowProfile)Profile;
        duplicated = IEllipseHollowProfile.Create(ellipseHollow.Depth, ellipseHollow.Width,
          ellipseHollow.WallThickness);
      } else if (Profile.GetType().ToString().Equals($"{typeof(IEllipseProfile)}_Implementation")) {
        var ellipse = (IEllipseProfile)Profile;
        duplicated = IEllipseProfile.Create(ellipse.Depth, ellipse.Width);
      } else if (Profile.GetType().ToString().Equals($"{typeof(IGeneralCProfile)}_Implementation")) {
        var generalC = (IGeneralCProfile)Profile;
        duplicated = IGeneralCProfile.Create(generalC.Depth, generalC.FlangeWidth, generalC.Lip, generalC.Thickness);
      } else if (Profile.GetType().ToString().Equals($"{typeof(IGeneralZProfile)}_Implementation")) {
        var generalZ = (IGeneralZProfile)Profile;
        duplicated = IGeneralZProfile.Create(generalZ.Depth, generalZ.TopFlangeWidth, generalZ.BottomFlangeWidth,
          generalZ.TopLip, generalZ.BottomLip, generalZ.Thickness);
      } else if (Profile.GetType().ToString().Equals($"{typeof(IIBeamAsymmetricalProfile)}_Implementation")) {
        var iBeamAsymmetrical = (IIBeamAsymmetricalProfile)Profile;
        duplicated = IIBeamAsymmetricalProfile.Create(iBeamAsymmetrical.Depth, iBeamAsymmetrical.TopFlange,
          iBeamAsymmetrical.BottomFlange, iBeamAsymmetrical.Web);
      } else if (Profile.GetType().ToString().Equals($"{typeof(IIBeamCellularProfile)}_Implementation")) {
        var iBeamCellular = (IIBeamCellularProfile)Profile;
        duplicated = IIBeamCellularProfile.Create(iBeamCellular.Depth, iBeamCellular.Flanges, iBeamCellular.Web,
          iBeamCellular.WebOpening);
      } else if (Profile.GetType().ToString().Equals($"{typeof(IIBeamSymmetricalProfile)}_Implementation")) {
        var iBeamSymmetrical = (IIBeamSymmetricalProfile)Profile;
        duplicated = IIBeamSymmetricalProfile.Create(iBeamSymmetrical.Depth, iBeamSymmetrical.Flanges,
          iBeamSymmetrical.Web);
      } else if (Profile.GetType().ToString().Equals($"{typeof(IRectangleHollowProfile)}_Implementation")) {
        var rectangleHollow = (IRectangleHollowProfile)Profile;
        duplicated = IRectangleHollowProfile.Create(rectangleHollow.Depth, rectangleHollow.Flanges,
          rectangleHollow.Webs);
      } else if (Profile.GetType().ToString().Equals($"{typeof(IRectangleProfile)}_Implementation")) {
        var rectangle = (IRectangleProfile)Profile;
        duplicated = IRectangleProfile.Create(rectangle.Depth, rectangle.Width);
      } else if (Profile.GetType().ToString().Equals($"{typeof(IRectoEllipseProfile)}_Implementation")) {
        var rectoEllipse = (IRectoEllipseProfile)Profile;
        duplicated = IRectoEllipseProfile.Create(rectoEllipse.Depth, rectoEllipse.DepthFlat, rectoEllipse.Width,
          rectoEllipse.WidthFlat);
      } else if (Profile.GetType().ToString().Equals($"{typeof(ISecantPileProfile)}_Implementation")) {
        var secantPile = (ISecantPileProfile)Profile;
        duplicated = ISecantPileProfile.Create(secantPile.Diameter, secantPile.PileCentres, secantPile.PileCount,
          secantPile.IsWallNotSection);
      } else if (Profile.GetType().ToString().Equals($"{typeof(ISheetPileProfile)}_Implementation")) {
        var sheetPile = (ISheetPileProfile)Profile;
        duplicated = ISheetPileProfile.Create(sheetPile.Depth, sheetPile.Width, sheetPile.TopFlangeWidth,
          sheetPile.BottomFlangeWidth, sheetPile.FlangeThickness, sheetPile.WebThickness);
      } else if (Profile.GetType().ToString().Equals($"{typeof(IStadiumProfile)}_Implementation")) {
        var stadium = (IStadiumProfile)Profile;
        duplicated = IStadiumProfile.Create(stadium.Depth, stadium.Width);
      } else if (Profile.GetType().ToString().Equals($"{typeof(ITrapezoidProfile)}_Implementation")) {
        var trapezoid = (ITrapezoidProfile)Profile;
        duplicated = ITrapezoidProfile.Create(trapezoid.Depth, trapezoid.TopWidth, trapezoid.BottomWidth);
      } else if (Profile.GetType().ToString().Equals($"{typeof(ITSectionProfile)}_Implementation")) {
        var tSection = (ITSectionProfile)Profile;
        duplicated = ITSectionProfile.Create(tSection.Depth, tSection.Flange, tSection.Web);
      } else {
        duplicated = IPerimeterProfile.Create(Profile);
      }

      duplicated.IsReflectedY = Profile.IsReflectedY;
      duplicated.IsReflectedZ = Profile.IsReflectedZ;
      duplicated.Rotation = Profile.Rotation;

      return duplicated;
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return new AdSecProfileGoo(Clone(), new Plane(_plane));
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      return Polyline.BoundingBox;
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      return null;
    }

    public override object ScriptVariable() {
      return Profile;
    }

    public override string ToString() {
      return Profile.Description();
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      return null;
    }

    internal static Tuple<List<Point3d>, List<List<Point3d>>> PointsFromAdSecPermiter(
      IPerimeterProfile perimeterProfile, Plane local) {
      if (perimeterProfile == null) {
        return null;
      }

      var solid = perimeterProfile.SolidPolygon;
      var rhinoEdgePoints = PtsFromAdSecPolygon(solid, local);

      var rhinoVoidPoints
        = perimeterProfile.VoidPolygons.Select(polygon => PtsFromAdSecPolygon(polygon, local)).ToList();
      return new Tuple<List<Point3d>, List<List<Point3d>>>(rhinoEdgePoints, rhinoVoidPoints);
    }

    internal static IPolygon PolygonFromRhinoPolyline(Polyline polyline, LengthUnit lengthUnit, Plane local) {
      var polygon = IPolygon.Create();
      polygon.Points = PtsFromRhinoPolyline(polyline, lengthUnit, local);
      return polygon;
    }

    internal static Tuple<Polyline, List<Polyline>> PolylinesFromAdSecProfile(IProfile profile, Plane local) {
      var perimeter = profile is IPerimeterProfile perimeterProfile ? perimeterProfile : IPerimeterProfile.Create(profile);
      var pointsFromAdSecPermiter = PointsFromAdSecPermiter(perimeter, local);

      var solid = new Polyline(pointsFromAdSecPermiter.Item1);
      var voids = pointsFromAdSecPermiter.Item2.Select(point3ds => new Polyline(point3ds)).ToList();
      return new Tuple<Polyline, List<Polyline>>(solid, voids);
    }

    internal static List<Point3d> PtsFromAdSecPolygon(IPolygon polygon, Plane local) {
      if (polygon == null) {
        return null;
      }

      var maptToLocal = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldYZ, local);
      var rhinoPoints = new List<Point3d>();

      foreach (var point in polygon.Points) {
        var point3d = new Point3d(0, point.Y.As(DefaultUnits.LengthUnitGeometry),
          point.Z.As(DefaultUnits.LengthUnitGeometry));
        point3d.Transform(maptToLocal);
        rhinoPoints.Add(point3d);
      }

      // add first point to end of list for closed polyline
      rhinoPoints.Add(rhinoPoints[0]);

      return rhinoPoints;
    }

    internal static Oasys.Collections.IList<IPoint> PtsFromRhinoPolyline(
      Polyline polyline, LengthUnit lengthUnit, Plane local) {
      if (polyline == null) {
        return null;
      }

      if (polyline[0] != polyline[polyline.Count - 1]) {
        polyline.Add(polyline[0]);
      }

      var points = Oasys.Collections.IList<IPoint>.Create();

      // map points to XY plane so we can create local points from x and y coordinates
      var xform = Rhino.Geometry.Transform.PlaneToPlane(local, Plane.WorldXY);

      for (int i = 0; i < polyline.Count - 1; i++)
      // -1 on count because the profile is always closed and thus doesnt
      // need the last point being equal to first as a rhino polyline needs
      {
        var point3d = polyline[i];
        point3d.Transform(xform);
        var point = IPoint.Create(new Length(point3d.X, lengthUnit), new Length(point3d.Y, lengthUnit));
        points.Add(point);
      }

      return points;
    }

    private void UpdatePreview() {
      if (!PlaneHelper.IsNotParallelToWorldXYZ(_plane)) {
        return;
      }

      (previewXaxis, previewYaxis, previewZaxis) = AxisHelper.GetLocalAxisLines(Profile, _plane);
    }
  }
}
