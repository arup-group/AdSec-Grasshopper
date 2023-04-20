using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Oasys.AdSec.Materials.StressStrainCurves;
using OasysGH.Units;
using OasysUnits;
using Rhino.Geometry;
using System;
using System.Linq;

namespace AdSecGH.Parameters {
  public class AdSecStressStrainPointGoo : GH_GeometricGoo<Point3d>, IGH_PreviewData {
    public override BoundingBox Boundingbox {
      get {
        if (Value == null)
          return BoundingBox.Empty;
        Point3d pt1 = new Point3d(Value);
        pt1.Z += 0.25;
        Point3d pt2 = new Point3d(Value);
        pt2.Z += -0.25;
        Line ln = new Line(pt1, pt2);
        LineCurve crv = new LineCurve(ln);
        return crv.GetBoundingBox(false);
      }
    }
    public BoundingBox ClippingBox {
      get { return Boundingbox; }
    }
    public IStressStrainPoint StressStrainPoint {
      get {
        return m_SSpoint;
      }
    }
    public override string TypeDescription => "AdSec " + TypeName + " Parameter";
    public override string TypeName => "StressStrainPoint";
    private IStressStrainPoint m_SSpoint;

    public AdSecStressStrainPointGoo(Point3d point) : base(point) {
      m_value = point;
      m_SSpoint = IStressStrainPoint.Create(
        new Pressure(m_value.Y, DefaultUnits.StressUnitResult),
        new Strain(m_value.X, DefaultUnits.StrainUnitResult));
    }

    public AdSecStressStrainPointGoo(AdSecStressStrainPointGoo stressstrainPoint) {
      m_SSpoint = stressstrainPoint.StressStrainPoint;
      m_value = new Point3d(Value);
    }

    public AdSecStressStrainPointGoo(IStressStrainPoint stressstrainPoint) {
      m_SSpoint = stressstrainPoint;
      m_value = new Point3d(
        m_SSpoint.Strain.As(DefaultUnits.StrainUnitResult),
        m_SSpoint.Stress.As(DefaultUnits.StressUnitResult),
        0);
    }

    public AdSecStressStrainPointGoo(Pressure stress, Strain strain) {
      m_SSpoint = IStressStrainPoint.Create(stress, strain);
      m_value = new Point3d(
        m_SSpoint.Strain.As(DefaultUnits.StrainUnitResult),
        m_SSpoint.Stress.As(DefaultUnits.StressUnitResult),
        0);
    }

    public static IStressStrainPoint CreateFromPoint3d(Point3d point) {
      return IStressStrainPoint.Create(
        new Pressure(point.Y, DefaultUnits.StressUnitResult),
        new Strain(point.X, DefaultUnits.StrainUnitResult));
    }

    public override bool CastFrom(object source) {
      if (source == null)
        return false;

      if (source is Point3d) {
        AdSecStressStrainPointGoo temp = new AdSecStressStrainPointGoo((Point3d)source);
        m_value = temp.Value;
        m_SSpoint = temp.StressStrainPoint;
        return true;
      }

      if (source is IStressStrainPoint) {
        AdSecStressStrainPointGoo temp = new AdSecStressStrainPointGoo((IStressStrainPoint)source);
        m_value = temp.Value;
        m_SSpoint = temp.StressStrainPoint;
        return true;
      }

      GH_Point ptGoo = source as GH_Point;
      if (ptGoo != null) {
        AdSecStressStrainPointGoo temp = new AdSecStressStrainPointGoo(ptGoo.Value);
        m_value = temp.Value;
        m_SSpoint = temp.StressStrainPoint;
        return true;
      }

      Point3d pt = new Point3d();
      if (GH_Convert.ToPoint3d(source, ref pt, GH_Conversion.Both)) {
        AdSecStressStrainPointGoo temp = new AdSecStressStrainPointGoo(pt);
        m_value = temp.Value;
        m_SSpoint = temp.StressStrainPoint;
        return true;
      }
      return false;
    }

    public override bool CastTo<Q>(out Q target) {
      if (typeof(Q).IsAssignableFrom(typeof(AdSecStressStrainPointGoo))) {
        target = (Q)(object)new AdSecStressStrainPointGoo(Value);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(Point3d))) {
        target = (Q)(object)Value;
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Point))) {
        target = (Q)(object)new GH_Point(Value);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(IStressStrainPoint))) {
        target = (Q)(object)IStressStrainPoint.Create(
            new Pressure(Value.Y, DefaultUnits.StressUnitResult),
            new Strain(Value.X, DefaultUnits.StrainUnitResult));
        return true;
      }
      target = default(Q);
      return false;
    }

    public void DrawViewportMeshes(GH_PreviewMeshArgs args) {
    }

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      if (Value != null) {
        args.Pipeline.DrawCircle(new Circle(Value, 0.5), UI.Colour.OasysYellow, 1);
      }
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return new AdSecStressStrainPointGoo(new Point3d(Value));
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      if (Value == null) { return BoundingBox.Empty; }
      Point3d pt = new Point3d(Value);
      pt.Z += 0.001;
      Line ln = new Line(Value, pt);
      LineCurve crv = new LineCurve(ln);
      return crv.GetBoundingBox(xform);
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      return null;
    }

    public override string ToString() {
      IQuantity quantityStrain = new Strain(0, DefaultUnits.StrainUnitResult);
      string unitStrainAbbreviation = string.Concat(quantityStrain.ToString().Where(char.IsLetter));
      IQuantity quantityStress = new Pressure(0, DefaultUnits.StressUnitResult);
      string unitStressAbbreviation = string.Concat(quantityStress.ToString().Where(char.IsLetter));
      return "AdSec " + TypeName + " {"
        + Math.Round(StressStrainPoint.Strain.As(DefaultUnits.StrainUnitResult), 4) + unitStrainAbbreviation + ", "
        + Math.Round(StressStrainPoint.Stress.As(DefaultUnits.StressUnitResult), 4) + unitStressAbbreviation + "}";
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      return null;
    }
  }
}
