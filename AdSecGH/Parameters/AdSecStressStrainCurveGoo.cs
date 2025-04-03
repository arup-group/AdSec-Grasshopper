using System;
using System.Collections.Generic;
using System.Linq;

using AdSecGH.UI;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec.Materials.StressStrainCurves;

using OasysGH.Units;

using OasysUnits;

using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecStressStrainCurveGoo : GH_GeometricGoo<Curve>, IGH_PreviewData {
    public enum StressStrainCurveType {
      Bilinear,
      Explicit,
      FibModelCode,
      Linear,
      ManderConfined,
      Mander,
      ParabolaRectangle,
      Park,
      Popovics,
      Rectangular,
      StressStrainDefault
    }

    public List<AdSecStressStrainPointGoo> AdSecStressStrainPoints {
      get {
        var outPts = new List<AdSecStressStrainPointGoo>();
        Oasys.Collections.IList<IStressStrainPoint> pts = IStressStrainPoints;
        foreach (IStressStrainPoint pt in pts) {
          outPts.Add(new AdSecStressStrainPointGoo(pt));
        }
        return outPts;
      }
    }
    public override BoundingBox Boundingbox {
      get {
        if (Value == null) {
          return BoundingBox.Empty;
        }
        return Value.GetBoundingBox(false);
      }
    }
    public BoundingBox ClippingBox => Boundingbox;
    public List<Point3d> ControlPoints { get; } = new List<Point3d>();
    public Oasys.Collections.IList<IStressStrainPoint> IStressStrainPoints {
      get {
        try {
          var crv2 = (IExplicitStressStrainCurve)StressStrainCurve;
          return crv2.Points;
        } catch (Exception) {
          throw new InvalidCastException("Unable to cast to internal IStressStrainCurve to IExplicitStressStrainCurve");
        }
      }
    }
    public IStressStrainCurve StressStrainCurve { get; }
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "StressStrainCurve";

    private StressStrainCurveType m_type;

    public AdSecStressStrainCurveGoo(Curve curve, IStressStrainCurve stressStrainCurve, StressStrainCurveType type, List<Point3d> points) : base(curve) {
      ControlPoints = points;
      m_type = type;
      StressStrainCurve = stressStrainCurve;
    }

    public override bool CastFrom(object source) {
      if (source == null) {
        return false;
      }
      if (source is Curve curve) {
        Value = curve;
        return true;
      }
      if (source is GH_Curve lineGoo) {
        Value = lineGoo.Value;
        return true;
      }

      Curve line = null;
      if (GH_Convert.ToCurve(source, ref line, GH_Conversion.Both)) {
        Value = line;
        return true;
      }
      return false;
    }

    public override bool CastTo<Q>(out Q target) {
      if (typeof(Q).IsAssignableFrom(typeof(AdSecStressStrainCurveGoo))) {
        target = (Q)(object)new AdSecStressStrainCurveGoo(m_value.DuplicateCurve(), StressStrainCurve, m_type, ControlPoints.ToList());
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(Line))) {
        target = (Q)(object)Value;
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Curve))) {
        target = (Q)(object)new GH_Curve(Value);
        return true;
      }

      target = default;
      return false;
    }

    public void DrawViewportMeshes(GH_PreviewMeshArgs args) {
    }

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      if (Value != null) {
        args.Pipeline.DrawCurve(Value, Colour.OasysBlue, 2);
        foreach (Point3d pt in ControlPoints) {
          args.Pipeline.DrawCircle(new Circle(pt, 0.5), Colour.OasysYellow, 1);
        }
      }
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return new AdSecStressStrainCurveGoo(Value.DuplicateCurve(), StressStrainCurve, m_type, ControlPoints);
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      return Value.GetBoundingBox(false);
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      return null;
    }

    public override string ToString() {
      return $"AdSec {TypeName} {{{m_type}}}";
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      return null;
    }

    internal static Tuple<Curve, List<Point3d>> Create(IStressStrainCurve stressStrainCurve, StressStrainCurveType type, bool isCompression) {
      int direction = isCompression ? 1 : -1;
      Curve crvOut = null;
      var pts = new List<Point3d>();
      if (type == StressStrainCurveType.Bilinear) {
        var crv1 = (IBilinearStressStrainCurve)stressStrainCurve;
        pts.Add(new Point3d(0, 0, 0));
        pts.Add(new Point3d(
            crv1.YieldPoint.Strain.As(DefaultUnits.StrainUnitResult) * direction,
            crv1.YieldPoint.Stress.As(DefaultUnits.StressUnitResult) * direction, 0));
        pts.Add(new Point3d(
            crv1.FailurePoint.Strain.As(DefaultUnits.StrainUnitResult) * direction,
            crv1.FailurePoint.Stress.As(DefaultUnits.StressUnitResult) * direction, 0));
        crvOut = new Polyline(pts).ToPolylineCurve();
      } else if (type == StressStrainCurveType.Explicit) {
        var crv2 = (IExplicitStressStrainCurve)stressStrainCurve;
        foreach (IStressStrainPoint pt in crv2.Points) {
          pts.Add(new Point3d(
          pt.Strain.As(DefaultUnits.StrainUnitResult) * direction,
          pt.Stress.As(DefaultUnits.StressUnitResult) * direction, 0));
        }
        crvOut = new Polyline(pts).ToPolylineCurve();
      } else if (type == StressStrainCurveType.Linear) {
        var crv3 = (ILinearStressStrainCurve)stressStrainCurve;
        pts.Add(new Point3d(0, 0, 0));
        pts.Add(new Point3d(
            crv3.FailurePoint.Strain.As(DefaultUnits.StrainUnitResult) * direction,
            crv3.FailurePoint.Stress.As(DefaultUnits.StressUnitResult) * direction, 0));
        crvOut = new Polyline(pts).ToPolylineCurve();
      } else {
        double maxStrain = stressStrainCurve.FailureStrain.As(DefaultUnits.StrainUnitResult);
        var polypts = new List<Point3d>();
        for (int i = 0; i < 250; i++) {
          var strain = new Strain((double)i / (double)100.0 * maxStrain, DefaultUnits.StrainUnitResult);

          Pressure stress;
          try {
            stress = stressStrainCurve.StressAt(strain); // for some material models the first point returns an NaN
          } catch (Exception) {
            stress = Pressure.Zero;
          }

          polypts.Add(new Point3d(
          strain.As(DefaultUnits.StrainUnitResult) * direction,
          stress.As(DefaultUnits.StressUnitResult) * direction, 0));
        }
        crvOut = new Polyline(polypts).ToPolylineCurve();
      }

      return new Tuple<Curve, List<Point3d>>(crvOut, pts);
    }

    internal static Tuple<Curve, List<Point3d>> CreateFromCode(IStressStrainCurve stressStrainCurve, bool isCompression) {
      int direction = isCompression ? 1 : -1;
      Curve crvOut = null;
      var pts = new List<Point3d>();

      double maxStrain = stressStrainCurve.FailureStrain.As(DefaultUnits.StrainUnitResult);
      var polypts = new List<Point3d>();
      for (int i = 0; i < 100; i++) {
        var strain = new Strain((double)i / (double)100.0 * maxStrain, DefaultUnits.StrainUnitResult);
        Pressure stress = stressStrainCurve.StressAt(strain);
        polypts.Add(new Point3d(
        strain.As(DefaultUnits.StrainUnitResult) * direction,
        stress.As(DefaultUnits.StressUnitResult) * direction, 0));
      }
      crvOut = new Polyline(polypts).ToPolylineCurve();
      pts.Add(polypts.First());
      pts.Add(polypts.Last());

      return new Tuple<Curve, List<Point3d>>(crvOut, pts);
    }

    internal static Oasys.Collections.IList<IStressStrainPoint> StressStrainPtsFromPolyline(PolylineCurve curve) {
      var pts = Oasys.Collections.IList<IStressStrainPoint>.Create();
      IStressStrainPoint pt = null;
      for (int j = 0; j < curve.PointCount; j++) {
        Point3d point3d = curve.Point(j);
        pt = IStressStrainPoint.Create(
            new Pressure(point3d.Y, DefaultUnits.StressUnitResult),
            new Strain(point3d.X, DefaultUnits.StrainUnitResult));
        pts.Add(pt);
      }
      return pts;
    }
  }
}
