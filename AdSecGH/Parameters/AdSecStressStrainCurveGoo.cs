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
using OasysUnits.Units;

using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecStressStrainCurveGoo : GH_GeometricGoo<StressStrainCurve>, IGH_PreviewData {

    public Curve Curve { get; private set; }

    public List<AdSecStressStrainPointGoo> AdSecStressStrainPoints {
      get {
        var outPts = new List<AdSecStressStrainPointGoo>();
        var points = GetIStressStrainPoints();
        outPts.AddRange(points.Select(pt => new AdSecStressStrainPointGoo(pt)));
        return outPts;
      }
    }
    public override BoundingBox Boundingbox => Value == null ? BoundingBox.Empty : Curve.GetBoundingBox(false);
    public BoundingBox ClippingBox => Boundingbox;
    public List<Point3d> ControlPoints { get; }

    public Oasys.Collections.IList<IStressStrainPoint> GetIStressStrainPoints() {
      try {
        var explicitStressStrainCurve = (IExplicitStressStrainCurve)Value.IStressStrainCurve;
        return explicitStressStrainCurve.Points;
      } catch (Exception) {
        throw new InvalidCastException("Unable to cast to internal IStressStrainCurve to IExplicitStressStrainCurve");
      }
    }

    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "StressStrainCurve";

    public AdSecStressStrainCurveGoo(Curve curve, StressStrainCurve stressStrainCurve, List<Point3d> points) : base(stressStrainCurve) {
      Curve = curve;
      ControlPoints = points;
    }

    public override bool CastFrom(object source) {
      switch (source) {
        case null: return false;
        case StressStrainCurve stressStrainCurve:
          Value = stressStrainCurve;
          Curve = Create(stressStrainCurve.IStressStrainCurve, stressStrainCurve.IsCompression).Curve;
          return true;
      }

      return false;
    }

    public override bool CastTo<Q>(out Q target) {
      if (typeof(Q).IsAssignableFrom(typeof(AdSecStressStrainCurveGoo))) {
        target = (Q)(object)new AdSecStressStrainCurveGoo(Curve.DuplicateCurve(), Value, ControlPoints.ToList());
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(Line))) {
        target = (Q)(object)Value;
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Curve))) {
        target = (Q)(object)new GH_Curve(Curve);
        return true;
      }

      target = default;
      return false;
    }

    public void DrawViewportMeshes(GH_PreviewMeshArgs args) { }

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      if (Value == null) {
        return;
      }

      args.Pipeline.DrawCurve(Curve, Colour.OasysBlue, 2);
      foreach (var point3d in ControlPoints) {
        args.Pipeline.DrawCircle(new Circle(point3d, 0.5), Colour.OasysYellow, 1);
      }
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return new AdSecStressStrainCurveGoo(Curve.DuplicateCurve(), Value, ControlPoints);
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      return Curve.GetBoundingBox(false);
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      return null;
    }

    public override string ToString() {
      var type = StressStrainCurveFunction.GetCurveTypeFromInterface(m_value.IStressStrainCurve);
      return $"AdSec {TypeName} {{{type}}}";
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      return null;
    }

    internal static AdSecStressStrainCurveGoo Create(IStressStrainCurve stressStrainCurve, bool isCompression) {
      int direction = isCompression ? 1 : -1;
      Curve curveOut;
      var point3ds = new List<Point3d>();

      switch (stressStrainCurve) {
        case IBilinearStressStrainCurve bilinear:
          point3ds.Add(new Point3d(0, 0, 0));
          point3ds.Add(new Point3d(bilinear.YieldPoint.Strain.As(DefaultUnits.StrainUnitResult) * direction,
            bilinear.YieldPoint.Stress.As(DefaultUnits.StressUnitResult) * direction, 0));
          point3ds.Add(new Point3d(bilinear.FailurePoint.Strain.As(DefaultUnits.StrainUnitResult) * direction,
            bilinear.FailurePoint.Stress.As(DefaultUnits.StressUnitResult) * direction, 0));
          curveOut = new Polyline(point3ds).ToPolylineCurve();
          break;

        case IExplicitStressStrainCurve explicitCurve:
          point3ds.AddRange(explicitCurve.Points.Select(point => new Point3d(
            point.Strain.As(DefaultUnits.StrainUnitResult) * direction,
            point.Stress.As(DefaultUnits.StressUnitResult) * direction, 0)));
          curveOut = new Polyline(point3ds).ToPolylineCurve();
          break;

        case ILinearStressStrainCurve linear:
          point3ds.Add(new Point3d(0, 0, 0));
          point3ds.Add(new Point3d(linear.FailurePoint.Strain.As(DefaultUnits.StrainUnitResult) * direction,
            linear.FailurePoint.Stress.As(DefaultUnits.StressUnitResult) * direction, 0));
          curveOut = new Polyline(point3ds).ToPolylineCurve();
          break;

        default:
          double maxStrain = stressStrainCurve.FailureStrain.As(StrainUnit.MilliStrain);
          //limit this to 2.5% strain(probably no tension material)
          maxStrain = maxStrain > 99 ? 2.5 : maxStrain;
          var polypoints = new List<Point3d>();
          for (int i = 0; i <= 100; i++) {
            var strain = new Strain(Math.Min(i / 100.0 * maxStrain, maxStrain), StrainUnit.MilliStrain);
            Pressure stress;
            try {
              stress = stressStrainCurve.StressAt(strain);
            } catch (Exception) {
              stress = Pressure.Zero;
            }

            polypoints.Add(new Point3d(strain.As(DefaultUnits.StrainUnitResult) * direction,
              stress.As(DefaultUnits.StressUnitResult) * direction, 0));
          }

          curveOut = new Polyline(polypoints).ToPolylineCurve();
          break;
      }
      return new AdSecStressStrainCurveGoo(curveOut, new StressStrainCurve() { IStressStrainCurve = stressStrainCurve, IsCompression = isCompression }, point3ds);
    }

    internal static AdSecStressStrainCurveGoo CreateFromCode(
      IStressStrainCurve stressStrainCurve, bool isCompression) {
      int direction = isCompression ? 1 : -1;
      var point3ds = new List<Point3d>();

      double maxStrain = stressStrainCurve.FailureStrain.As(StrainUnit.MilliStrain);
      if (maxStrain > 99) {
        //limit this to 2.5% strain
        maxStrain = 2.5;
      }
      var polypoints = new List<Point3d>();
      for (int i = 0; i < 100; i++) {
        var strain = new Strain(i / 100.0 * maxStrain, StrainUnit.MilliStrain);
        var stress = stressStrainCurve.StressAt(strain);
        polypoints.Add(new Point3d(strain.As(DefaultUnits.StrainUnitResult) * direction,
          stress.As(DefaultUnits.StressUnitResult) * direction, 0));
      }

      var curveOut = new Polyline(polypoints).ToPolylineCurve();
      point3ds.Add(polypoints[0]);
      point3ds.Add(polypoints[polypoints.Count - 1]);

      return new AdSecStressStrainCurveGoo(curveOut, new StressStrainCurve() { IStressStrainCurve = stressStrainCurve, IsCompression = isCompression }, point3ds);
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
