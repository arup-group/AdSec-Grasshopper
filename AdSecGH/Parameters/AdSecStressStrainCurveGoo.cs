using System;
using System.Collections.Generic;
using System.Linq;

using AdSecCore.Functions;

using AdSecGH.UI;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec.Materials.StressStrainCurves;

using OasysGH.Units;

using OasysUnits;

using Rhino.Geometry;



namespace AdSecGH.Parameters {
  public class AdSecStressStrainCurveGoo : GH_GeometricGoo<Curve>, IGH_PreviewData {

    public List<AdSecStressStrainPointGoo> AdSecStressStrainPoints {
      get {
        var outPts = new List<AdSecStressStrainPointGoo>();
        var points = GetIStressStrainPoints();
        outPts.AddRange(points.Select(pt => new AdSecStressStrainPointGoo(pt)));
        return outPts;
      }
    }
    public override BoundingBox Boundingbox => Value == null ? BoundingBox.Empty : Value.GetBoundingBox(false);
    public BoundingBox ClippingBox => Boundingbox;
    public List<Point3d> ControlPoints { get; }

    public Oasys.Collections.IList<IStressStrainPoint> GetIStressStrainPoints() {
      try {
        var explicitStressStrainCurve = (IExplicitStressStrainCurve)StressStrainCurve;
        return explicitStressStrainCurve.Points;
      } catch (Exception) {
        throw new InvalidCastException("Unable to cast to internal IStressStrainCurve to IExplicitStressStrainCurve");
      }
    }
    public IStressStrainCurve StressStrainCurve { get; }
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "StressStrainCurve";

    private readonly StressStrainCurveType _type;

    public AdSecStressStrainCurveGoo(Curve curve, IStressStrainCurve stressStrainCurve, StressStrainCurveType type, List<Point3d> points) : base(curve) {
      ControlPoints = points;
      _type = type;
      StressStrainCurve = stressStrainCurve;
    }

    public override bool CastFrom(object source) {
      switch (source) {
        case null: return false;
        case Curve curve:
          Value = curve;
          return true;
        case GH_Curve lineGoo:
          Value = lineGoo.Value;
          return true;
      }

      Curve line = null;
      if (!GH_Convert.ToCurve(source, ref line, GH_Conversion.Both)) {
        return false;
      }

      Value = line;
      return true;
    }

    public override bool CastTo<Q>(out Q target) {
      if (typeof(Q).IsAssignableFrom(typeof(AdSecStressStrainCurveGoo))) {
        target = (Q)(object)new AdSecStressStrainCurveGoo(m_value.DuplicateCurve(), StressStrainCurve, _type,
          ControlPoints.ToList());
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
      if (Value == null) {
        return;
      }

      args.Pipeline.DrawCurve(Value, Colour.OasysBlue, 2);
      foreach (var point3d in ControlPoints) {
        args.Pipeline.DrawCircle(new Circle(point3d, 0.5), Colour.OasysYellow, 1);
      }
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return new AdSecStressStrainCurveGoo(Value.DuplicateCurve(), StressStrainCurve, _type, ControlPoints);
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      return Value.GetBoundingBox(false);
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      return null;
    }

    public override string ToString() {
      return $"AdSec {TypeName} {{{_type}}}";
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      return null;
    }

    internal static Tuple<Curve, List<Point3d>> Create(IStressStrainCurve stressStrainCurve, StressStrainCurveType type, bool isCompression) {
      int direction = isCompression ? 1 : -1;
      Curve curveOut;
      var point3ds = new List<Point3d>();
      switch (type) {
        case StressStrainCurveType.Bilinear: {
            var strainCurve = (IBilinearStressStrainCurve)stressStrainCurve;
            point3ds.Add(new Point3d(0, 0, 0));
            point3ds.Add(new Point3d(strainCurve.YieldPoint.Strain.As(DefaultUnits.StrainUnitResult) * direction,
              strainCurve.YieldPoint.Stress.As(DefaultUnits.StressUnitResult) * direction, 0));
            point3ds.Add(new Point3d(strainCurve.FailurePoint.Strain.As(DefaultUnits.StrainUnitResult) * direction,
              strainCurve.FailurePoint.Stress.As(DefaultUnits.StressUnitResult) * direction, 0));
            curveOut = new Polyline(point3ds).ToPolylineCurve();
            break;
          }

        case StressStrainCurveType.Explicit: {
            var strainCurve = (IExplicitStressStrainCurve)stressStrainCurve;
            point3ds.AddRange(strainCurve.Points.Select(point => new Point3d(
              point.Strain.As(DefaultUnits.StrainUnitResult) * direction,
              point.Stress.As(DefaultUnits.StressUnitResult) * direction, 0)));
            curveOut = new Polyline(point3ds).ToPolylineCurve();
            break;
          }

        case StressStrainCurveType.Linear: {
            var strainCurve = (ILinearStressStrainCurve)stressStrainCurve;
            point3ds.Add(new Point3d(0, 0, 0));
            point3ds.Add(new Point3d(strainCurve.FailurePoint.Strain.As(DefaultUnits.StrainUnitResult) * direction,
              strainCurve.FailurePoint.Stress.As(DefaultUnits.StressUnitResult) * direction, 0));
            curveOut = new Polyline(point3ds).ToPolylineCurve();
            break;
          }

        default: {
            double maxStrain = stressStrainCurve.FailureStrain.As(DefaultUnits.StrainUnitResult);
            var polypoints = new List<Point3d>();
            for (int i = 0; i < 250; i++) {
              var strain = new Strain(i / 100.0 * maxStrain, DefaultUnits.StrainUnitResult);

              Pressure stress;
              try {
                stress = stressStrainCurve.StressAt(strain); // for some material models the first point returns an NaN
              } catch (Exception) {
                stress = Pressure.Zero;
              }

              polypoints.Add(new Point3d(strain.As(DefaultUnits.StrainUnitResult) * direction,
                stress.As(DefaultUnits.StressUnitResult) * direction, 0));
            }

            curveOut = new Polyline(polypoints).ToPolylineCurve();
            break;
          }
      }

      return new Tuple<Curve, List<Point3d>>(curveOut, point3ds);
    }

    internal static Tuple<Curve, List<Point3d>> CreateFromCode(IStressStrainCurve stressStrainCurve, bool isCompression) {
      int direction = isCompression ? 1 : -1;
      var point3ds = new List<Point3d>();

      double maxStrain = stressStrainCurve.FailureStrain.As(DefaultUnits.StrainUnitResult);
      var polypoints = new List<Point3d>();
      for (int i = 0; i < 100; i++) {
        var strain = new Strain(i / 100.0 * maxStrain, DefaultUnits.StrainUnitResult);
        var stress = stressStrainCurve.StressAt(strain);
        polypoints.Add(new Point3d(strain.As(DefaultUnits.StrainUnitResult) * direction,
          stress.As(DefaultUnits.StressUnitResult) * direction, 0));
      }

      var curveOut = new Polyline(polypoints).ToPolylineCurve();
      point3ds.Add(polypoints[0]);
      point3ds.Add(polypoints[polypoints.Count - 1]);

      return new Tuple<Curve, List<Point3d>>(curveOut, point3ds);
    }

    internal static Oasys.Collections.IList<IStressStrainPoint> StressStrainPtsFromPolyline(PolylineCurve curve) {
      var strainPoints = Oasys.Collections.IList<IStressStrainPoint>.Create();
      for (int j = 0; j < curve.PointCount; j++) {
        var point3d = curve.Point(j);
        var point = IStressStrainPoint.Create(new Pressure(point3d.Y, DefaultUnits.StressUnitResult),
          new Strain(point3d.X, DefaultUnits.StrainUnitResult));
        strainPoints.Add(point);
      }

      return strainPoints;
    }
  }
}
