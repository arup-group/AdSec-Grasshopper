using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Oasys.Profiles;
using OasysGH.Units;
using OasysUnits;
using OasysUnits.Units;
using Rhino.Geometry;

namespace AdSecGH.Parameters
{
  public class AdSecProfileGoo : GH_GeometricGoo<Polyline>, IGH_PreviewData
  {
    private IProfile m_profile;
    private Plane m_plane = Plane.WorldYZ;
    private List<Polyline> m_voidEdges;
    private Line previewXaxis;
    private Line previewYaxis;
    private Line previewZaxis;
    public override string TypeName => "Profile";
    public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";
    public override BoundingBox Boundingbox
    {
      get
      {
        if (this.Value == null)
          return BoundingBox.Empty;
        return this.Value.BoundingBox;
      }
    }
    public BoundingBox ClippingBox
    {
      get
      {
        return this.Boundingbox;
      }
    }
    public Plane LocalPlane
    {
      get
      {
        return this.m_plane;
      }
    }
    public IProfile Profile
    {
      get
      {
        return this.m_profile;
      }
    }
    public List<Polyline> VoidEdges
    {
      get
      {
        return this.m_voidEdges;
      }
    }
    public bool IsReflectedY
    {
      get
      {
        return this.m_profile.IsReflectedY;
      }
      set
      {
        this.m_profile = this.Clone();
        this.m_profile.IsReflectedY = value;
      }
    }
    public bool IsReflectedZ
    {
      get
      {
        return this.m_profile.IsReflectedZ;
      }
      set
      {
        this.m_profile = this.Clone();
        this.m_profile.IsReflectedZ = value;
      }
    }
    public Angle Rotation
    {
      get
      {
        return this.m_profile.Rotation;
      }
      set
      {
        this.m_profile = this.Clone();
        this.m_profile.Rotation = value;
      }
    }

    public AdSecProfileGoo(Polyline polygon, LengthUnit lengthUnit) : base(polygon)
    {
      // Create from polygon
      IPerimeterProfile perimprofile = IPerimeterProfile.Create();

      // Get local plane
      Plane.FitPlaneToPoints(polygon.ToList(), out Plane plane);

      perimprofile.SolidPolygon = PolygonFromRhinoPolyline(polygon, lengthUnit, plane);
      // create Profile
      this.m_profile = perimprofile;
      this.m_voidEdges = null;
      this.m_plane = plane;
      this.m_profile = perimprofile;
      UpdatePreview();
    }

    public AdSecProfileGoo(IProfile profile, Plane local)
    {
      this.m_profile = profile;
      Tuple<Polyline, List<Polyline>> edges = PolylinesFromAdSecProfile(profile, local);
      this.m_value = edges.Item1;
      this.m_voidEdges = edges.Item2;
      this.m_plane = local;
      this.UpdatePreview();
    }

    public AdSecProfileGoo(Brep brep, LengthUnit lengthUnit)
    {
      IPerimeterProfile profile = IPerimeterProfile.Create();
      // get edge curves from Brep
      Curve[] edgeSegments = brep.DuplicateEdgeCurves();
      Curve[] edges = Curve.JoinCurves(edgeSegments);

      // find the best fit plane
      List<Point3d> ctrl_pts = new List<Point3d>();
      if (edges[0].TryGetPolyline(out Polyline tempCrv))
        ctrl_pts = tempCrv.ToList();
      else
        throw new Exception("Cannot convert edge to Polyline");

      Plane.FitPlaneToPoints(ctrl_pts, out Plane plane);
      plane.Origin = tempCrv.CenterPoint();
      //Rhino.Geometry.Transform xform = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldYZ, plane);

      List<Point3d> solidpts = new List<Point3d>();
      foreach (Point3d pt3d in ctrl_pts)
      {
        //pt3d.Transform(xform);
        solidpts.Add(pt3d);
      }
      Polyline solid = new Polyline(solidpts);
      profile.SolidPolygon = PolygonFromRhinoPolyline(solid, lengthUnit, plane);

      if (edges.Length > 1)
      {
        List<Polyline> voidEdges = new List<Polyline>();
        Oasys.Collections.IList<IPolygon> voids = Oasys.Collections.IList<IPolygon>.Create();
        for (int i = 1; i < edges.Length; i++)
        {
          ctrl_pts.Clear();
          List<Point3d> voidpts = new List<Point3d>();
          if (!edges[i].IsPlanar())
            for (int j = 0; j < edges.Length; j++)
              edges[j] = Curve.ProjectToPlane(edges[j], plane);

          if (edges[i].TryGetPolyline(out tempCrv))
          {
            ctrl_pts = tempCrv.ToList();

            foreach (Point3d pt3d in ctrl_pts)
              voidpts.Add(pt3d);
            //pt3d.Transform(xform);
          }
          else
            throw new Exception("Cannot convert internal edge  to Polyline");

          Polyline voidCrv = new Polyline(voidpts);
          voids.Add(PolygonFromRhinoPolyline(voidCrv, lengthUnit, plane));
          voidEdges.Add(voidCrv);
        }
        profile.VoidPolygons = voids;
      }
      m_plane = plane;
      m_value = solid;
      m_profile = (IProfile)profile;
      UpdatePreview();
    }

    public AdSecProfileGoo(Polyline solid, List<Polyline> voids, LengthUnit lengthUnit)
    {
      // Create from polygon
      IPerimeterProfile perimprofile = IPerimeterProfile.Create();

      // Get local axis from solid polyline
      Plane.FitPlaneToPoints(solid.ToList(), out Plane plane);

      perimprofile.SolidPolygon = PolygonFromRhinoPolyline(solid, lengthUnit, plane);

      Oasys.Collections.IList<IPolygon> adsecVoids = Oasys.Collections.IList<IPolygon>.Create();
      foreach (Polyline vdCrv in voids)
        adsecVoids.Add(PolygonFromRhinoPolyline(vdCrv, lengthUnit, plane));

      perimprofile.VoidPolygons = adsecVoids;

      // create Profile
      m_profile = (IProfile)perimprofile;
      m_voidEdges = voids;
      m_plane = plane;
      UpdatePreview();
    }

    private void UpdatePreview()
    {
      // local axis
      if (m_plane != null)
      {
        if (m_plane != Plane.WorldXY & m_plane != Plane.WorldYZ & m_plane != Plane.WorldZX)
        {
          Area area = m_profile.Area();
          double pythogoras = Math.Sqrt(area.As(AreaUnit.SquareMeter));
          Length length = new Length(pythogoras * 0.15, LengthUnit.Meter);
          previewXaxis = new Line(m_plane.Origin, m_plane.XAxis, length.As(DefaultUnits.LengthUnitGeometry));
          previewYaxis = new Line(m_plane.Origin, m_plane.YAxis, length.As(DefaultUnits.LengthUnitGeometry));
          previewZaxis = new Line(m_plane.Origin, m_plane.ZAxis, length.As(DefaultUnits.LengthUnitGeometry));
        }
      }
    }

    internal static Oasys.Collections.IList<IPoint> PtsFromRhinoPolyline(Polyline polyline, LengthUnit lengthUnit, Plane local)
    {
      if (polyline == null)
        return null;

      if (polyline.First() != polyline.Last())
        polyline.Add(polyline.First());

      Oasys.Collections.IList<IPoint> pts = Oasys.Collections.IList<IPoint>.Create();

      // map points to XY plane so we can create local points from x and y coordinates
      Transform xform = Rhino.Geometry.Transform.PlaneToPlane(local, Plane.WorldXY);

      for (int i = 0; i < polyline.Count - 1; i++)
      // -1 on count because the profile is always closed and thus doesnt
      // need the last point being equal to first as a rhino polyline needs
      {
        Point3d point3d = polyline[i];
        point3d.Transform(xform);
        IPoint pt = IPoint.Create(
          new Length(point3d.X, lengthUnit),
          new Length(point3d.Y, lengthUnit));
        pts.Add(pt);
      }

      return pts;
    }

    internal static IPolygon PolygonFromRhinoPolyline(Polyline polyline, LengthUnit lengthUnit, Plane local)
    {
      IPolygon polygon = IPolygon.Create();
      polygon.Points = PtsFromRhinoPolyline(polyline, lengthUnit, local);
      return polygon;
    }

    internal static List<Point3d> PtsFromAdSecPolygon(IPolygon polygon, Plane local)
    {
      if (polygon == null) { return null; }

      Plane adsecLocal = new Plane(new Point3d(0, 0, 0), new Vector3d(1, 0, 0));

      // transform to local plane
      Transform maptToLocal = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldYZ, local);

      Oasys.Collections.IList<IPoint> apts = polygon.Points;
      List<Point3d> rhPts = new List<Point3d>();

      foreach (IPoint apt in apts)
      {
        Point3d pt = new Point3d(0,
          apt.Y.As(DefaultUnits.LengthUnitGeometry),
          apt.Z.As(DefaultUnits.LengthUnitGeometry));
        pt.Transform(maptToLocal);
        rhPts.Add(pt);
      }

      // add first point to end of list for closed polyline
      rhPts.Add(rhPts[0]);

      return rhPts;
    }

    internal static Tuple<List<Point3d>, List<List<Point3d>>> PtsFromAdSecPermiter(IPerimeterProfile perimeterProfile, Plane local)
    {
      if (perimeterProfile == null)
        return null;

      IPolygon solid = perimeterProfile.SolidPolygon;
      List<Point3d> rhEdgePts = PtsFromAdSecPolygon(solid, local);

      Oasys.Collections.IList<IPolygon> voids = perimeterProfile.VoidPolygons;
      List<List<Point3d>> rhVoidPts = new List<List<Point3d>>();
      foreach (IPolygon vpol in voids)
        rhVoidPts.Add(PtsFromAdSecPolygon(vpol, local));

      return new Tuple<List<Point3d>, List<List<Point3d>>>(rhEdgePts, rhVoidPts);
    }

    internal static Tuple<Polyline, List<Polyline>> PolylinesFromAdSecProfile(IProfile profile, Plane local)
    {
      IPerimeterProfile perimeter = IPerimeterProfile.Create(profile);

      Tuple<List<Point3d>, List<List<Point3d>>> pts = PtsFromAdSecPermiter(perimeter, local);

      Polyline solid = new Polyline(pts.Item1);
      List<Polyline> voids = new List<Polyline>();
      foreach (List<Point3d> plvoid in pts.Item2)
        voids.Add(new Polyline(plvoid));

      return new Tuple<Polyline, List<Polyline>>(solid, voids);
    }

    public IProfile Clone()
    {
      IProfile dup = null;

      // angle
      if (this.m_profile.GetType().ToString().Equals(typeof(IAngleProfile).ToString() + "_Implementation"))
      {
        IAngleProfile angle = (IAngleProfile)this.m_profile;
        dup = IAngleProfile.Create(angle.Depth, angle.Flange, angle.Web);
      }

      // catalogue
      else if (this.m_profile.GetType().ToString().Equals(typeof(ICatalogueProfile).ToString() + "_Implementation"))
      {
        dup = ICatalogueProfile.Create(this.m_profile.Description());
      }

      // channel
      else if (this.m_profile.GetType().ToString().Equals(typeof(IChannelProfile).ToString() + "_Implementation"))
      {
        IChannelProfile channel = (IChannelProfile)this.m_profile;
        dup = IChannelProfile.Create(channel.Depth, channel.Flanges, channel.Web);
      }

      // circle hollow
      else if (this.m_profile.GetType().ToString().Equals(typeof(ICircleHollowProfile).ToString() + "_Implementation"))
      {
        ICircleHollowProfile circleHollow = (ICircleHollowProfile)this.m_profile;
        dup = ICircleHollowProfile.Create(circleHollow.Diameter, circleHollow.WallThickness);
      }

      // circle
      else if (this.m_profile.GetType().ToString().Equals(typeof(ICircleProfile).ToString() + "_Implementation"))
      {
        ICircleProfile circle = (ICircleProfile)this.m_profile;
        dup = ICircleProfile.Create(circle.Diameter);
      }

      // ICruciformSymmetricalProfile
      else if (this.m_profile.GetType().ToString().Equals(typeof(ICruciformSymmetricalProfile).ToString() + "_Implementation"))
      {
        ICruciformSymmetricalProfile cruciformSymmetrical = (ICruciformSymmetricalProfile)this.m_profile;
        dup = ICruciformSymmetricalProfile.Create(cruciformSymmetrical.Depth, cruciformSymmetrical.Flange, cruciformSymmetrical.Web);
      }

      // IEllipseHollowProfile
      else if (this.m_profile.GetType().ToString().Equals(typeof(IEllipseHollowProfile).ToString() + "_Implementation"))
      {
        IEllipseHollowProfile ellipseHollow = (IEllipseHollowProfile)this.m_profile;
        dup = IEllipseHollowProfile.Create(ellipseHollow.Depth, ellipseHollow.Width, ellipseHollow.WallThickness);
      }

      // IEllipseProfile
      else if (this.m_profile.GetType().ToString().Equals(typeof(IEllipseProfile).ToString() + "_Implementation"))
      {
        IEllipseProfile ellipse = (IEllipseProfile)this.m_profile;
        dup = IEllipseProfile.Create(ellipse.Depth, ellipse.Width);
      }

      // IGeneralCProfile
      else if (this.m_profile.GetType().ToString().Equals(typeof(IGeneralCProfile).ToString() + "_Implementation"))
      {
        IGeneralCProfile generalC = (IGeneralCProfile)this.m_profile;
        dup = IGeneralCProfile.Create(generalC.Depth, generalC.FlangeWidth, generalC.Lip, generalC.Thickness);
      }

      // IGeneralZProfile
      else if (this.m_profile.GetType().ToString().Equals(typeof(IGeneralZProfile).ToString() + "_Implementation"))
      {
        IGeneralZProfile generalZ = (IGeneralZProfile)this.m_profile;
        dup = IGeneralZProfile.Create(generalZ.Depth, generalZ.TopFlangeWidth, generalZ.BottomFlangeWidth, generalZ.TopLip, generalZ.BottomLip, generalZ.Thickness);
      }

      // IIBeamAsymmetricalProfile
      else if (this.m_profile.GetType().ToString().Equals(typeof(IIBeamAsymmetricalProfile).ToString() + "_Implementation"))
      {
        IIBeamAsymmetricalProfile iBeamAsymmetrical = (IIBeamAsymmetricalProfile)this.m_profile;
        dup = IIBeamAsymmetricalProfile.Create(iBeamAsymmetrical.Depth, iBeamAsymmetrical.TopFlange, iBeamAsymmetrical.BottomFlange, iBeamAsymmetrical.Web);
      }

      // IIBeamCellularProfile
      else if (this.m_profile.GetType().ToString().Equals(typeof(IIBeamCellularProfile).ToString() + "_Implementation"))
      {
        IIBeamCellularProfile iBeamCellular = (IIBeamCellularProfile)this.m_profile;
        dup = IIBeamCellularProfile.Create(iBeamCellular.Depth, iBeamCellular.Flanges, iBeamCellular.Web, iBeamCellular.WebOpening);
      }

      // IIBeamSymmetricalProfile
      else if (this.m_profile.GetType().ToString().Equals(typeof(IIBeamSymmetricalProfile).ToString() + "_Implementation"))
      {
        IIBeamSymmetricalProfile iBeamSymmetrical = (IIBeamSymmetricalProfile)this.m_profile;
        dup = IIBeamSymmetricalProfile.Create(iBeamSymmetrical.Depth, iBeamSymmetrical.Flanges, iBeamSymmetrical.Web);
      }

      // IRectangleHollowProfile
      else if (this.m_profile.GetType().ToString().Equals(typeof(IRectangleHollowProfile).ToString() + "_Implementation"))
      {
        IRectangleHollowProfile rectangleHollow = (IRectangleHollowProfile)this.m_profile;
        dup = IRectangleHollowProfile.Create(rectangleHollow.Depth, rectangleHollow.Flanges, rectangleHollow.Webs);
      }

      // IRectangleProfile
      else if (this.m_profile.GetType().ToString().Equals(typeof(IRectangleProfile).ToString() + "_Implementation"))
      {
        IRectangleProfile rectangle = (IRectangleProfile)this.m_profile;
        dup = IRectangleProfile.Create(rectangle.Depth, rectangle.Width);
      }

      // IRectoEllipseProfile
      else if (this.m_profile.GetType().ToString().Equals(typeof(IRectoEllipseProfile).ToString() + "_Implementation"))
      {
        IRectoEllipseProfile rectoEllipse = (IRectoEllipseProfile)this.m_profile;
        dup = IRectoEllipseProfile.Create(rectoEllipse.Depth, rectoEllipse.DepthFlat, rectoEllipse.Width, rectoEllipse.WidthFlat);
      }

      // ISecantPileProfile
      else if (this.m_profile.GetType().ToString().Equals(typeof(ISecantPileProfile).ToString() + "_Implementation"))
      {
        ISecantPileProfile secantPile = (ISecantPileProfile)this.m_profile;
        dup = ISecantPileProfile.Create(secantPile.Diameter, secantPile.PileCentres, secantPile.PileCount, secantPile.IsWallNotSection);
      }

      // ISheetPileProfile
      else if (this.m_profile.GetType().ToString().Equals(typeof(ISheetPileProfile).ToString() + "_Implementation"))
      {
        ISheetPileProfile sheetPile = (ISheetPileProfile)this.m_profile;
        dup = ISheetPileProfile.Create(sheetPile.Depth, sheetPile.Width, sheetPile.TopFlangeWidth, sheetPile.BottomFlangeWidth, sheetPile.FlangeThickness, sheetPile.WebThickness);
      }

      // IStadiumProfile
      else if (this.m_profile.GetType().ToString().Equals(typeof(IStadiumProfile).ToString() + "_Implementation"))
      {
        IStadiumProfile stadium = (IStadiumProfile)this.m_profile;
        dup = IStadiumProfile.Create(stadium.Depth, stadium.Width);
      }

      // ITrapezoidProfile
      else if (this.m_profile.GetType().ToString().Equals(typeof(ITrapezoidProfile).ToString() + "_Implementation"))
      {
        ITrapezoidProfile trapezoid = (ITrapezoidProfile)this.m_profile;
        dup = ITrapezoidProfile.Create(trapezoid.Depth, trapezoid.TopWidth, trapezoid.BottomWidth);
      }

      // ITSectionProfile
      else if (this.m_profile.GetType().ToString().Equals(typeof(ITSectionProfile).ToString() + "_Implementation"))
      {
        ITSectionProfile tSection = (ITSectionProfile)this.m_profile;
        dup = ITSectionProfile.Create(tSection.Depth, tSection.Flange, tSection.Web);
      }

      // IPerimeterProfile (last chance...)
      else
      {
        dup = IPerimeterProfile.Create(this.m_profile);
      }

      // modifications
      dup.IsReflectedY = this.m_profile.IsReflectedY;
      dup.IsReflectedZ = this.m_profile.IsReflectedZ;
      dup.Rotation = this.m_profile.Rotation;

      return dup;
    }

    public override string ToString()
    {
      return "AdSec " + TypeName + " {" + m_profile.Description() + "}";
    }

    public override IGH_GeometricGoo DuplicateGeometry()
    {
      return new AdSecProfileGoo(this.Clone(), new Plane(m_plane));
    }


    public override BoundingBox GetBoundingBox(Transform xform)
    {
      return Value.BoundingBox;
    }

    public override IGH_GeometricGoo Transform(Transform xform)
    {
      return null;
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
      return null;
    }

    public override object ScriptVariable()
    {
      return m_profile;
    }

    public override bool CastTo<TQ>(out TQ target)
    {
      if (typeof(TQ).IsAssignableFrom(typeof(AdSecProfileGoo)))
      {
        target = (TQ)(object)this;
        return true;
      }

      if (typeof(TQ).IsAssignableFrom(typeof(Line)))
      {
        target = (TQ)(object)Value;
        return true;
      }

      if (typeof(TQ).IsAssignableFrom(typeof(GH_Curve)))
      {
        target = (TQ)(object)new GH_Curve(new PolylineCurve(Value));
        return true;
      }

      target = default(TQ);
      return false;
    }

    public override bool CastFrom(object source)
    {
      if (source == null)
        return false;

      // try cast using GH_Convert, if that doesnt work we are doomed
      Curve crv = null;
      if (GH_Convert.ToCurve(source, ref crv, GH_Conversion.Both))
      {
        Polyline poly;
        if (crv.TryGetPolyline(out poly))
        {
          AdSecProfileGoo temp = new AdSecProfileGoo(poly, DefaultUnits.LengthUnitGeometry);
          this.m_value = temp.m_value;
          this.m_profile = temp.m_profile;
          this.m_voidEdges = temp.m_voidEdges;
          return true;
        }
      }

      return false;
    }

    public void DrawViewportWires(GH_PreviewWireArgs args)
    {
      if (Value != null)
      {
        Color defaultCol = Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
        if (args.Color.R == defaultCol.R && args.Color.G == defaultCol.G && args.Color.B == defaultCol.B) // not selected
        {
          args.Pipeline.DrawPolyline(Value, UI.Colour.OasysBlue, 2);
          if (this.m_voidEdges != null)
          {
            foreach (Polyline crv in m_voidEdges)
            {
              args.Pipeline.DrawPolyline(crv, UI.Colour.OasysBlue, 1);
            }
          }
        }
        else // selected
        {
          args.Pipeline.DrawPolyline(Value, UI.Colour.OasysYellow, 3);
          if (this.m_voidEdges != null)
          {
            foreach (Polyline crv in m_voidEdges)
            {
              args.Pipeline.DrawPolyline(crv, UI.Colour.OasysYellow, 2);
            }
          }
        }
        // local axis
        if (previewXaxis != null)
        {
          args.Pipeline.DrawLine(previewZaxis, Color.FromArgb(255, 244, 96, 96), 1);
          args.Pipeline.DrawLine(previewXaxis, Color.FromArgb(255, 96, 244, 96), 1);
          args.Pipeline.DrawLine(previewYaxis, Color.FromArgb(255, 96, 96, 234), 1);
        }
      }
    }

    public void DrawViewportMeshes(GH_PreviewMeshArgs args) { }
  }
}
