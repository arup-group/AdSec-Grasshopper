using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecCore.Functions;

using AdSecGH.Helpers;

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
    public override BoundingBox Boundingbox {
      get {
        if (Value == null) {
          return BoundingBox.Empty;
        }

        return Polyline.BoundingBox;
      }
    }
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
    public Plane LocalPlane => m_plane;
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
    public Polyline Polyline;
    private Plane m_plane = Plane.WorldYZ;
    private Line previewXaxis;
    private Line previewYaxis;
    private Line previewZaxis;

    public AdSecProfileGoo(ProfileDesign profileDesign) : base(profileDesign) {
      Profile = profileDesign.Profile;
      m_plane = profileDesign.LocalPlane.ToGh();
      Tuple<Polyline, List<Polyline>> edges = PolylinesFromAdSecProfile(Profile, m_plane);
      Polyline = edges.Item1;
      VoidEdges = edges.Item2;
      UpdatePreview();
    }

    public AdSecProfileGoo(IProfile profile, Plane local) {
      Profile = profile;
      Tuple<Polyline, List<Polyline>> edges = PolylinesFromAdSecProfile(profile, local);
      Polyline = edges.Item1;
      VoidEdges = edges.Item2;
      m_plane = local;
      UpdatePreview();
    }

    public AdSecProfileGoo(Polyline polygon, LengthUnit lengthUnit) {
      // Create from polygon
      var perimprofile = IPerimeterProfile.Create();

      // Get local plane
      Plane.FitPlaneToPoints(polygon.ToList(), out Plane plane);

      perimprofile.SolidPolygon = PolygonFromRhinoPolyline(polygon, lengthUnit, plane);
      // create Profile
      Profile = perimprofile;
      VoidEdges = null;
      m_plane = plane;
      Profile = perimprofile;
      UpdatePreview();
    }

    public override bool CastFrom(object source) {
      if (source == null) {
        return false;
      }

      // try cast using GH_Convert, if that doesnt work we are doomed
      Curve crv = null;
      if (GH_Convert.ToCurve(source, ref crv, GH_Conversion.Both)) {
        if (crv.TryGetPolyline(out Polyline poly)) {
          var temp = new AdSecProfileGoo(poly, DefaultUnits.LengthUnitGeometry);
          m_value = temp.m_value;
          Profile = temp.Profile;
          VoidEdges = temp.VoidEdges;
          return true;
        }
      }

      return false;
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
      IProfile dup = null;

      // angle
      if (Profile.GetType().ToString().Equals($"{typeof(IAngleProfile)}_Implementation")) {
        var angle = (IAngleProfile)Profile;
        dup = IAngleProfile.Create(angle.Depth, angle.Flange, angle.Web);
      }

      // catalogue
      else if (Profile.GetType().ToString().Equals($"{typeof(ICatalogueProfile)}_Implementation")) {
        dup = ICatalogueProfile.Create(Profile.Description());
      }

      // channel
      else if (Profile.GetType().ToString().Equals($"{typeof(IChannelProfile)}_Implementation")) {
        var channel = (IChannelProfile)Profile;
        dup = IChannelProfile.Create(channel.Depth, channel.Flanges, channel.Web);
      }

      // circle hollow
      else if (Profile.GetType().ToString().Equals($"{typeof(ICircleHollowProfile)}_Implementation")) {
        var circleHollow = (ICircleHollowProfile)Profile;
        dup = ICircleHollowProfile.Create(circleHollow.Diameter, circleHollow.WallThickness);
      }

      // circle
      else if (Profile.GetType().ToString().Equals($"{typeof(ICircleProfile)}_Implementation")) {
        var circle = (ICircleProfile)Profile;
        dup = ICircleProfile.Create(circle.Diameter);
      }

      // ICruciformSymmetricalProfile
      else if (Profile.GetType().ToString().Equals($"{typeof(ICruciformSymmetricalProfile)}_Implementation")) {
        var cruciformSymmetrical = (ICruciformSymmetricalProfile)Profile;
        dup = ICruciformSymmetricalProfile.Create(cruciformSymmetrical.Depth, cruciformSymmetrical.Flange,
          cruciformSymmetrical.Web);
      }

      // IEllipseHollowProfile
      else if (Profile.GetType().ToString().Equals($"{typeof(IEllipseHollowProfile)}_Implementation")) {
        var ellipseHollow = (IEllipseHollowProfile)Profile;
        dup = IEllipseHollowProfile.Create(ellipseHollow.Depth, ellipseHollow.Width, ellipseHollow.WallThickness);
      }

      // IEllipseProfile
      else if (Profile.GetType().ToString().Equals($"{typeof(IEllipseProfile)}_Implementation")) {
        var ellipse = (IEllipseProfile)Profile;
        dup = IEllipseProfile.Create(ellipse.Depth, ellipse.Width);
      }

      // IGeneralCProfile
      else if (Profile.GetType().ToString().Equals($"{typeof(IGeneralCProfile)}_Implementation")) {
        var generalC = (IGeneralCProfile)Profile;
        dup = IGeneralCProfile.Create(generalC.Depth, generalC.FlangeWidth, generalC.Lip, generalC.Thickness);
      }

      // IGeneralZProfile
      else if (Profile.GetType().ToString().Equals($"{typeof(IGeneralZProfile)}_Implementation")) {
        var generalZ = (IGeneralZProfile)Profile;
        dup = IGeneralZProfile.Create(generalZ.Depth, generalZ.TopFlangeWidth, generalZ.BottomFlangeWidth,
          generalZ.TopLip, generalZ.BottomLip, generalZ.Thickness);
      }

      // IIBeamAsymmetricalProfile
      else if (Profile.GetType().ToString().Equals($"{typeof(IIBeamAsymmetricalProfile)}_Implementation")) {
        var iBeamAsymmetrical = (IIBeamAsymmetricalProfile)Profile;
        dup = IIBeamAsymmetricalProfile.Create(iBeamAsymmetrical.Depth, iBeamAsymmetrical.TopFlange,
          iBeamAsymmetrical.BottomFlange, iBeamAsymmetrical.Web);
      }

      // IIBeamCellularProfile
      else if (Profile.GetType().ToString().Equals($"{typeof(IIBeamCellularProfile)}_Implementation")) {
        var iBeamCellular = (IIBeamCellularProfile)Profile;
        dup = IIBeamCellularProfile.Create(iBeamCellular.Depth, iBeamCellular.Flanges, iBeamCellular.Web,
          iBeamCellular.WebOpening);
      }

      // IIBeamSymmetricalProfile
      else if (Profile.GetType().ToString().Equals($"{typeof(IIBeamSymmetricalProfile)}_Implementation")) {
        var iBeamSymmetrical = (IIBeamSymmetricalProfile)Profile;
        dup = IIBeamSymmetricalProfile.Create(iBeamSymmetrical.Depth, iBeamSymmetrical.Flanges, iBeamSymmetrical.Web);
      }

      // IRectangleHollowProfile
      else if (Profile.GetType().ToString().Equals($"{typeof(IRectangleHollowProfile)}_Implementation")) {
        var rectangleHollow = (IRectangleHollowProfile)Profile;
        dup = IRectangleHollowProfile.Create(rectangleHollow.Depth, rectangleHollow.Flanges, rectangleHollow.Webs);
      }

      // IRectangleProfile
      else if (Profile.GetType().ToString().Equals($"{typeof(IRectangleProfile)}_Implementation")) {
        var rectangle = (IRectangleProfile)Profile;
        dup = IRectangleProfile.Create(rectangle.Depth, rectangle.Width);
      }

      // IRectoEllipseProfile
      else if (Profile.GetType().ToString().Equals($"{typeof(IRectoEllipseProfile)}_Implementation")) {
        var rectoEllipse = (IRectoEllipseProfile)Profile;
        dup = IRectoEllipseProfile.Create(rectoEllipse.Depth, rectoEllipse.DepthFlat, rectoEllipse.Width,
          rectoEllipse.WidthFlat);
      }

      // ISecantPileProfile
      else if (Profile.GetType().ToString().Equals($"{typeof(ISecantPileProfile)}_Implementation")) {
        var secantPile = (ISecantPileProfile)Profile;
        dup = ISecantPileProfile.Create(secantPile.Diameter, secantPile.PileCentres, secantPile.PileCount,
          secantPile.IsWallNotSection);
      }

      // ISheetPileProfile
      else if (Profile.GetType().ToString().Equals($"{typeof(ISheetPileProfile)}_Implementation")) {
        var sheetPile = (ISheetPileProfile)Profile;
        dup = ISheetPileProfile.Create(sheetPile.Depth, sheetPile.Width, sheetPile.TopFlangeWidth,
          sheetPile.BottomFlangeWidth, sheetPile.FlangeThickness, sheetPile.WebThickness);
      }

      // IStadiumProfile
      else if (Profile.GetType().ToString().Equals($"{typeof(IStadiumProfile)}_Implementation")) {
        var stadium = (IStadiumProfile)Profile;
        dup = IStadiumProfile.Create(stadium.Depth, stadium.Width);
      }

      // ITrapezoidProfile
      else if (Profile.GetType().ToString().Equals($"{typeof(ITrapezoidProfile)}_Implementation")) {
        var trapezoid = (ITrapezoidProfile)Profile;
        dup = ITrapezoidProfile.Create(trapezoid.Depth, trapezoid.TopWidth, trapezoid.BottomWidth);
      }

      // ITSectionProfile
      else if (Profile.GetType().ToString().Equals($"{typeof(ITSectionProfile)}_Implementation")) {
        var tSection = (ITSectionProfile)Profile;
        dup = ITSectionProfile.Create(tSection.Depth, tSection.Flange, tSection.Web);
      }

      // IPerimeterProfile (last chance...)
      else {
        dup = IPerimeterProfile.Create(Profile);
      }

      // modifications
      dup.IsReflectedY = Profile.IsReflectedY;
      dup.IsReflectedZ = Profile.IsReflectedZ;
      dup.Rotation = Profile.Rotation;

      return dup;
    }

    public void DrawViewportMeshes(GH_PreviewMeshArgs args) { }

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      if (Value != null) {
        Color defaultCol = Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
        if (args.Color.R == defaultCol.R && args.Color.G == defaultCol.G
          && args.Color.B == defaultCol.B) // not selected
        {
          args.Pipeline.DrawPolyline(Polyline, UI.Colour.OasysBlue, 2);
          if (VoidEdges != null) {
            foreach (Polyline crv in VoidEdges) {
              args.Pipeline.DrawPolyline(crv, UI.Colour.OasysBlue, 1);
            }
          }
        } else // selected
        {
          args.Pipeline.DrawPolyline(Polyline, UI.Colour.OasysYellow, 3);
          if (VoidEdges != null) {
            foreach (Polyline crv in VoidEdges) {
              args.Pipeline.DrawPolyline(crv, UI.Colour.OasysYellow, 2);
            }
          }
        }

        // local axis
        if (previewXaxis != null) {
          args.Pipeline.DrawLine(previewZaxis, Color.FromArgb(255, 244, 96, 96), 1);
          args.Pipeline.DrawLine(previewXaxis, Color.FromArgb(255, 96, 244, 96), 1);
          args.Pipeline.DrawLine(previewYaxis, Color.FromArgb(255, 96, 96, 234), 1);
        }
      }
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return new AdSecProfileGoo(Clone(), new Plane(m_plane));
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

      IPolygon solid = perimeterProfile.SolidPolygon;
      List<Point3d> rhEdgePts = PtsFromAdSecPolygon(solid, local);

      var rhVoidPts = new List<List<Point3d>>();
      foreach (IPolygon vpol in perimeterProfile.VoidPolygons) {
        rhVoidPts.Add(PtsFromAdSecPolygon(vpol, local));
      }

      return new Tuple<List<Point3d>, List<List<Point3d>>>(rhEdgePts, rhVoidPts);
    }

    internal static IPolygon PolygonFromRhinoPolyline(Polyline polyline, LengthUnit lengthUnit, Plane local) {
      var polygon = IPolygon.Create();
      polygon.Points = PtsFromRhinoPolyline(polyline, lengthUnit, local);
      return polygon;
    }

    internal static Tuple<Polyline, List<Polyline>> PolylinesFromAdSecProfile(IProfile profile, Plane local) {
      var perimeter = IPerimeterProfile.Create(profile);

      Tuple<List<Point3d>, List<List<Point3d>>> pts = PointsFromAdSecPermiter(perimeter, local);

      var solid = new Polyline(pts.Item1);
      var voids = new List<Polyline>();
      foreach (List<Point3d> plvoid in pts.Item2) {
        voids.Add(new Polyline(plvoid));
      }

      return new Tuple<Polyline, List<Polyline>>(solid, voids);
    }

    internal static List<Point3d> PtsFromAdSecPolygon(IPolygon polygon, Plane local) {
      if (polygon == null) {
        return null;
      }

      // transform to local plane
      var maptToLocal = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldYZ, local);

      var rhPts = new List<Point3d>();

      foreach (IPoint apt in polygon.Points) {
        var pt = new Point3d(0, apt.Y.As(DefaultUnits.LengthUnitGeometry), apt.Z.As(DefaultUnits.LengthUnitGeometry));
        pt.Transform(maptToLocal);
        rhPts.Add(pt);
      }

      // add first point to end of list for closed polyline
      rhPts.Add(rhPts[0]);

      return rhPts;
    }

    internal static Oasys.Collections.IList<IPoint> PtsFromRhinoPolyline(
      Polyline polyline, LengthUnit lengthUnit, Plane local) {
      if (polyline == null) {
        return null;
      }

      if (polyline.First() != polyline.Last()) {
        polyline.Add(polyline.First());
      }

      var pts = Oasys.Collections.IList<IPoint>.Create();

      // map points to XY plane so we can create local points from x and y coordinates
      var xform = Rhino.Geometry.Transform.PlaneToPlane(local, Plane.WorldXY);

      for (int i = 0; i < polyline.Count - 1; i++)
      // -1 on count because the profile is always closed and thus doesnt
      // need the last point being equal to first as a rhino polyline needs
      {
        Point3d point3d = polyline[i];
        point3d.Transform(xform);
        var pt = IPoint.Create(new Length(point3d.X, lengthUnit), new Length(point3d.Y, lengthUnit));
        pts.Add(pt);
      }

      return pts;
    }

    private void UpdatePreview() {
      // local axis
      if (m_plane != null) {
        if (m_plane != Plane.WorldXY && m_plane != Plane.WorldYZ && m_plane != Plane.WorldZX) {
          Area area = Profile.Area();
          double pythogoras = Math.Sqrt(area.As(AreaUnit.SquareMeter));
          var length = new Length(pythogoras * 0.15, LengthUnit.Meter);
          previewXaxis = new Line(m_plane.Origin, m_plane.XAxis, length.As(DefaultUnits.LengthUnitGeometry));
          previewYaxis = new Line(m_plane.Origin, m_plane.YAxis, length.As(DefaultUnits.LengthUnitGeometry));
          previewZaxis = new Line(m_plane.Origin, m_plane.ZAxis, length.As(DefaultUnits.LengthUnitGeometry));
        }
      }
    }
  }
}
