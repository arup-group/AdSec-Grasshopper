﻿using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Oasys.AdSec.Materials.StressStrainCurves;
using OasysGH.Units;
using OasysUnits;
using Rhino.Geometry;

namespace AdSecGH.Parameters
{
  public class AdSecStressStrainCurveGoo : GH_GeometricGoo<Curve>, IGH_PreviewData
  {
    public enum StressStrainCurveType
    {
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

    public override string TypeName => "StressStrainCurve";
    public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";
    public IStressStrainCurve StressStrainCurve
    {
      get
      {
        return this.m_SScurve;
      }
    }
    public List<Point3d> ControlPoints
    {
      get
      {
        return this.m_pts;
      }
    }
    public override BoundingBox Boundingbox
    {
      get
      {
        if (this.Value == null)
          return BoundingBox.Empty;
        return this.Value.GetBoundingBox(false);
      }
    }
    public BoundingBox ClippingBox
    {
      get
      {
        return this.Boundingbox;
      }
    }
    private List<Point3d> m_pts = new List<Point3d>();
    private StressStrainCurveType m_type;
    private IStressStrainCurve m_SScurve;

    public AdSecStressStrainCurveGoo(Curve curve, IStressStrainCurve stressStrainCurve, StressStrainCurveType type, List<Point3d> points) : base(curve)
    {
      this.m_pts = points;
      this.m_type = type;
      this.m_SScurve = stressStrainCurve;
    }

    #region methods
    public override bool CastTo<Q>(out Q target)
    {
      if (typeof(Q).IsAssignableFrom(typeof(AdSecStressStrainCurveGoo)))
      {
        target = (Q)(object)new AdSecStressStrainCurveGoo(m_value.DuplicateCurve(), m_SScurve, m_type, m_pts.ToList());
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(Line)))
      {
        target = (Q)(object)this.Value;
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Curve)))
      {
        target = (Q)(object)new GH_Curve(this.Value);
        return true;
      }

      target = default(Q);
      return false;
    }

    public override bool CastFrom(object source)
    {
      if (source == null) return false;
      if (source is Curve)
      {
        this.Value = (Curve)source;
        return true;
      }
      GH_Curve lineGoo = source as GH_Curve;
      if (lineGoo != null)
      {
        this.Value = lineGoo.Value;
        return true;
      }

      Curve line = null;
      if (GH_Convert.ToCurve(source, ref line, GH_Conversion.Both))
      {
        this.Value = line;
        return true;
      }
      return false;
    }

    public override string ToString()
    {
      return "AdSec " + this.TypeName + " {" + this.m_type.ToString() + "}";
    }

    public override IGH_GeometricGoo DuplicateGeometry()
    {
      return new AdSecStressStrainCurveGoo(this.Value.DuplicateCurve(), this.m_SScurve, this.m_type, this.m_pts);
    }

    public override BoundingBox GetBoundingBox(Transform xform)
    {
      return Value.GetBoundingBox(false);
    }

    public override IGH_GeometricGoo Transform(Transform xform)
    {
      return null;
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
      return null;
    }

    public void DrawViewportMeshes(GH_PreviewMeshArgs args)
    {
    }

    public void DrawViewportWires(GH_PreviewWireArgs args)
    {
      if (Value != null)
      {
        args.Pipeline.DrawCurve(Value, UI.Colour.OasysBlue, 2);
        foreach (Point3d pt in m_pts)
        {
          args.Pipeline.DrawCircle(new Circle(pt, 0.5), UI.Colour.OasysYellow, 1);
        }
      }
    }

    public Oasys.Collections.IList<IStressStrainPoint> IStressStrainPoints
    {
      get
      {
        try
        {
          IExplicitStressStrainCurve crv2 = (IExplicitStressStrainCurve)m_SScurve;
          return crv2.Points;

        }
        catch (Exception)
        {

          throw new Exception("Unable to cast to internal IStressStrainCurve to IExplicitStressStrainCurve");
        }
      }
    }

    public List<AdSecStressStrainPointGoo> AdSecStressStrainPoints
    {
      get
      {
        List<AdSecStressStrainPointGoo> outPts = new List<AdSecStressStrainPointGoo>();
        Oasys.Collections.IList<IStressStrainPoint> pts = this.IStressStrainPoints;
        foreach (IStressStrainPoint pt in pts)
        {
          outPts.Add(new AdSecStressStrainPointGoo(pt));
        }
        return outPts;
      }
    }

    internal static Oasys.Collections.IList<IStressStrainPoint> StressStrainPtsFromPolyline(PolylineCurve curve)
    {
      Oasys.Collections.IList<IStressStrainPoint> pts = Oasys.Collections.IList<IStressStrainPoint>.Create();
      IStressStrainPoint pt = null;
      for (int j = 0; j < curve.PointCount; j++)
      {
        Point3d point3d = curve.Point(j);
        pt = IStressStrainPoint.Create(
            new Pressure(point3d.Y, DefaultUnits.StressUnitResult),
            new Strain(point3d.X, DefaultUnits.StrainUnitResult));
        pts.Add(pt);
      }
      return pts;
    }

    internal static Tuple<Curve, List<Point3d>> Create(IStressStrainCurve stressStrainCurve, StressStrainCurveType type, bool isCompression)
    {
      int direction = isCompression ? 1 : -1;
      Curve crvOut = null;
      List<Point3d> pts = new List<Point3d>();
      if (type == StressStrainCurveType.Bilinear)
      {
        IBilinearStressStrainCurve crv1 = (IBilinearStressStrainCurve)stressStrainCurve;
        pts.Add(new Point3d(0, 0, 0));
        pts.Add(new Point3d(
            crv1.YieldPoint.Strain.As(DefaultUnits.StrainUnitResult) * direction,
            crv1.YieldPoint.Stress.As(DefaultUnits.StressUnitResult) * direction, 0));
        pts.Add(new Point3d(
            crv1.FailurePoint.Strain.As(DefaultUnits.StrainUnitResult) * direction,
            crv1.FailurePoint.Stress.As(DefaultUnits.StressUnitResult) * direction, 0));
        crvOut = new Polyline(pts).ToPolylineCurve();
      }
      else if (type == StressStrainCurveType.Explicit)
      {
        IExplicitStressStrainCurve crv2 = (IExplicitStressStrainCurve)stressStrainCurve;
        foreach (IStressStrainPoint pt in crv2.Points)
        {
          pts.Add(new Point3d(
          pt.Strain.As(DefaultUnits.StrainUnitResult) * direction,
          pt.Stress.As(DefaultUnits.StressUnitResult) * direction, 0));
        }
        crvOut = new Polyline(pts).ToPolylineCurve();
      }
      else if (type == StressStrainCurveType.Linear)
      {
        ILinearStressStrainCurve crv3 = (ILinearStressStrainCurve)stressStrainCurve;
        pts.Add(new Point3d(0, 0, 0));
        pts.Add(new Point3d(
            crv3.FailurePoint.Strain.As(DefaultUnits.StrainUnitResult) * direction,
            crv3.FailurePoint.Stress.As(DefaultUnits.StressUnitResult) * direction, 0));
        crvOut = new Polyline(pts).ToPolylineCurve();
      }
      else
      {
        double maxStrain = stressStrainCurve.FailureStrain.As(DefaultUnits.StrainUnitResult);
        List<Point3d> polypts = new List<Point3d>();
        for (int i = 0; i < 250; i++)
        {
          Strain strain = new Strain((double)i / (double)100.0 * maxStrain, DefaultUnits.StrainUnitResult);

          Pressure stress;
          try
          {
            stress = stressStrainCurve.StressAt(strain); // for some material models the first point returns an NaN
          }
          catch (Exception)
          {
            stress = Pressure.Zero;
          }

          polypts.Add(new Point3d(
          strain.As(DefaultUnits.StrainUnitResult) * direction,
          stress.As(DefaultUnits.StressUnitResult) * direction, 0));

        }
        crvOut = new Polyline(polypts).ToPolylineCurve();

        //if (type == StressStrainCurveType.FibModelCode)
        //{
        //    IFibModelCodeStressStrainCurve crv = (IFibModelCodeStressStrainCurve)stressStrainCurve;
        //    pts.Add(new Point3d(0, 0, 0));
        //    pts.Add(new Point3d(
        //        crv.PeakPoint.Strain.As(Units.StrainUnit) * direction,
        //        crv.PeakPoint.Stress.As(DefaultUnits.StrainUnitResult) * direction, 0));
        //}
        //if (type == StressStrainCurveType.Mander)
        //{
        //    IManderStressStrainCurve crv = (IManderStressStrainCurve)stressStrainCurve;
        //    pts.Add(new Point3d(0, 0, 0));
        //    pts.Add(new Point3d(
        //        crv.PeakPoint.Strain.As(Units.StrainUnit) * direction,
        //        crv.PeakPoint.Stress.As(DefaultUnits.StrainUnitResult) * direction, 0));
        //}
        //if (type == StressStrainCurveType.ParabolaRectangle)
        //{
        //    IParabolaRectangleStressStrainCurve crv = (IParabolaRectangleStressStrainCurve)stressStrainCurve;
        //    pts.Add(new Point3d(0, 0, 0));
        //    pts.Add(new Point3d(
        //        crv.YieldPoint.Strain.As(Units.StrainUnit) * direction,
        //        crv.YieldPoint.Stress.As(DefaultUnits.StrainUnitResult) * direction, 0));
        //}
        //if (type == StressStrainCurveType.Park)
        //{
        //    IParkStressStrainCurve crv = (IParkStressStrainCurve)stressStrainCurve;
        //    pts.Add(new Point3d(0, 0, 0));
        //    pts.Add(new Point3d(
        //        crv.YieldPoint.Strain.As(Units.StrainUnit) * direction,
        //        crv.YieldPoint.Stress.As(DefaultUnits.StrainUnitResult) * direction, 0));
        //}
        //if (type == StressStrainCurveType.Popovics)
        //{
        //    IPopovicsStressStrainCurve crv = (IPopovicsStressStrainCurve)stressStrainCurve;
        //    pts.Add(new Point3d(0, 0, 0));
        //    pts.Add(new Point3d(
        //        crv.PeakPoint.Strain.As(Units.StrainUnit) * direction,
        //        crv.PeakPoint.Stress.As(DefaultUnits.StrainUnitResult) * direction, 0));
        //}
        //if (type == StressStrainCurveType.Rectangular)
        //{
        //    IRectangularStressStrainCurve crv = (IRectangularStressStrainCurve)stressStrainCurve;
        //    pts.Add(new Point3d(0, 0, 0));
        //    pts.Add(new Point3d(
        //        crv.YieldPoint.Strain.As(Units.StrainUnit) * direction,
        //        crv.YieldPoint.Stress.As(DefaultUnits.StrainUnitResult) * direction, 0));
        //}
      }

      return new Tuple<Curve, List<Point3d>>(crvOut, pts);
    }

    internal static Tuple<Curve, List<Point3d>> CreateFromCode(IStressStrainCurve stressStrainCurve, bool isCompression)
    {
      int direction = isCompression ? 1 : -1;
      Curve crvOut = null;
      List<Point3d> pts = new List<Point3d>();

      double maxStrain = stressStrainCurve.FailureStrain.As(DefaultUnits.StrainUnitResult);
      List<Point3d> polypts = new List<Point3d>();
      for (int i = 0; i < 100; i++)
      {
        Strain strain = new Strain((double)i / (double)100.0 * maxStrain, DefaultUnits.StrainUnitResult);
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
    #endregion
  }
}