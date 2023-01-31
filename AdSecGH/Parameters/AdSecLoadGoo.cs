using System;
using System.Drawing;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Oasys.AdSec;
using OasysGH.Units;
using OasysUnits;
using Rhino.Display;
using Rhino.Geometry;

namespace AdSecGH.Parameters
{
  public class AdSecLoadGoo : GH_GeometricGoo<ILoad>, IGH_PreviewData
  {
    public static string Name => "Load";
    public static string NickName => "Ld";
    public static string Description => "AdSec Load";
    public override bool IsValid => true;
    public override string TypeName => "Load";
    public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

    private Point3d m_point = Point3d.Unset;

    public AdSecLoadGoo(ILoad load) : base(load) { }

    public AdSecLoadGoo(ILoad load, Plane local)
    {
      this.m_value = load;
      Point3d point = new Point3d(
          load.ZZ.As(DefaultUnits.MomentUnit),
          load.YY.As(DefaultUnits.MomentUnit),
          load.X.As(DefaultUnits.ForceUnit));
      Transform mapFromLocal = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldXY, local);
      point.Transform(mapFromLocal);
      m_point = point;
    }

    public BoundingBox ClippingBox
    {
      get { return Boundingbox; }
    }

    public override BoundingBox Boundingbox
    {
      get
      {
        if (Value == null) { return BoundingBox.Empty; }
        if (m_point == null) { return BoundingBox.Empty; }
        Point3d pt1 = new Point3d(m_point);
        pt1.Z += 0.25;
        Point3d pt2 = new Point3d(m_point);
        pt2.Z += -0.25;
        Line ln = new Line(pt1, pt2);
        LineCurve crv = new LineCurve(ln);
        return crv.GetBoundingBox(false);
      }
    }

    public override IGH_Goo Duplicate() => new AdSecLoadGoo(this.Value);

    public override string ToString()
    {
      IQuantity quantityMoment = new Moment(0, DefaultUnits.MomentUnit);
      string unitMomentAbbreviation = string.Concat(quantityMoment.ToString().Where(char.IsLetter));
      IQuantity quantityForce = new Force(0, DefaultUnits.ForceUnit);
      string unitforceAbbreviation = string.Concat(quantityForce.ToString().Where(char.IsLetter));
      return "AdSec " + TypeName + " {"
          + Math.Round(this.Value.X.As(DefaultUnits.ForceUnit), 4) + unitforceAbbreviation + ", "
          + Math.Round(this.Value.YY.As(DefaultUnits.MomentUnit), 4) + unitMomentAbbreviation + ", "
          + Math.Round(this.Value.ZZ.As(DefaultUnits.MomentUnit), 4) + unitMomentAbbreviation + "}";
    }

    public override bool CastTo<TQ>(out TQ target)
    {
      if (typeof(TQ).IsAssignableFrom(typeof(AdSecLoadGoo)))
      {
        target = (TQ)(object)new AdSecLoadGoo(this.Value);
        return true;
      }

      if (typeof(TQ).IsAssignableFrom(typeof(Point3d)))
      {
        target = (TQ)(object)m_point;
        return true;
      }

      if (typeof(TQ).IsAssignableFrom(typeof(GH_Point)))
      {
        target = (TQ)(object)new GH_Point(m_point);
        return true;
      }

      if (typeof(TQ).IsAssignableFrom(typeof(ILoad)))
      {
        target = (TQ)(object)ILoad.Create(Value.X, Value.YY, Value.ZZ);
        return true;
      }

      target = default(TQ);
      return false;
    }
    public override bool CastFrom(object source)
    {
      if (source == null) return false;

      if (source is Point3d)
      {
        Point3d point = (Point3d)source;
        ILoad load = ILoad.Create(
            new Force(point.X, DefaultUnits.ForceUnit),
            new Moment(point.Y, DefaultUnits.MomentUnit),
            new Moment(point.Z, DefaultUnits.MomentUnit));
        AdSecLoadGoo temp = new AdSecLoadGoo(load);
        this.Value = temp.Value;
        return true;
      }

      GH_Point ptGoo = source as GH_Point;
      if (ptGoo != null)
      {
        Point3d point = ptGoo.Value;
        ILoad load = ILoad.Create(
            new Force(point.X, DefaultUnits.ForceUnit),
            new Moment(point.Y, DefaultUnits.MomentUnit),
            new Moment(point.Z, DefaultUnits.MomentUnit));
        AdSecLoadGoo temp = new AdSecLoadGoo(load);
        this.Value = temp.Value;
        return true;
      }

      Point3d pt = new Point3d();
      if (GH_Convert.ToPoint3d(source, ref pt, GH_Conversion.Both))
      {
        Point3d point = pt;
        ILoad load = ILoad.Create(
            new Force(point.X, DefaultUnits.ForceUnit),
            new Moment(point.Y, DefaultUnits.MomentUnit),
            new Moment(point.Z, DefaultUnits.MomentUnit));
        AdSecLoadGoo temp = new AdSecLoadGoo(load);
        this.Value = temp.Value;
        return true;
      }

      return false;
    }
    public void DrawViewportWires(GH_PreviewWireArgs args)
    {
      if (m_point.IsValid)
      {
        Color defaultCol = Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
        if (args.Color.R == defaultCol.R && args.Color.G == defaultCol.G && args.Color.B == defaultCol.B) // not selected
          args.Pipeline.DrawPoint(m_point, PointStyle.X, 7, UI.Colour.ArupRed);
        else
          args.Pipeline.DrawPoint(m_point, PointStyle.X, 8, UI.Colour.UILightGrey);
      }
    }

    public void DrawViewportMeshes(GH_PreviewMeshArgs args)
    {
    }

    public override IGH_GeometricGoo DuplicateGeometry()
    {
      AdSecLoadGoo dup = new AdSecLoadGoo(Value);
      dup.m_point = new Point3d(m_point);
      return dup;
    }

    public override BoundingBox GetBoundingBox(Transform xform)
    {
      if (Value == null) { return BoundingBox.Empty; }
      if (m_point == null) { return BoundingBox.Empty; }
      Point3d pt1 = new Point3d(m_point);
      pt1.Z += 0.25;
      Point3d pt2 = new Point3d(m_point);
      pt2.Z += -0.25;
      Line ln = new Line(pt1, pt2);
      LineCurve crv = new LineCurve(ln);
      return crv.GetBoundingBox(xform);
    }

    public override IGH_GeometricGoo Transform(Transform xform)
    {
      return null;
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
      return null;
    }
  }
}
