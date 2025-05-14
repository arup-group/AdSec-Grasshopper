using System;
using System.Collections.Generic;
using System.Linq;

using AdSecGH.Helpers;
using AdSecGH.UI;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec.Materials.StressStrainCurves;

using OasysGH.Units;

using OasysUnits;

using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecStressStrainPointGoo : GH_GeometricGoo<Point3d>, IGH_PreviewData {
    public override BoundingBox Boundingbox {
      get {
        const double offset = 0.5d;
        return PointHelper.GetPointBoundingBox(Value, offset, true);
      }
    }

    public BoundingBox ClippingBox => Boundingbox;
    public IStressStrainPoint StressStrainPoint { get; private set; }
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "StressStrainPoint";
    public List<DrawInstructions> DrawInstructionsList { get; private set; } = new List<DrawInstructions>();

    public AdSecStressStrainPointGoo(Point3d point) : base(point) {
      m_value = point;
      StressStrainPoint = IStressStrainPoint.Create(
        new Pressure(m_value.Y, DefaultUnits.StressUnitResult),
        new Strain(m_value.X, DefaultUnits.StrainUnitResult));
    }

    public AdSecStressStrainPointGoo(AdSecStressStrainPointGoo stressstrainPoint) {
      StressStrainPoint = stressstrainPoint.StressStrainPoint;
      m_value = new Point3d(Value);
    }

    public AdSecStressStrainPointGoo(IStressStrainPoint stressstrainPoint) {
      StressStrainPoint = stressstrainPoint;
      m_value = new Point3d(
        StressStrainPoint.Strain.As(DefaultUnits.StrainUnitResult),
        StressStrainPoint.Stress.As(DefaultUnits.StressUnitResult),
        0);
    }

    public AdSecStressStrainPointGoo(Pressure stress, Strain strain) {
      StressStrainPoint = IStressStrainPoint.Create(stress, strain);
      m_value = new Point3d(
        StressStrainPoint.Strain.As(DefaultUnits.StrainUnitResult),
        StressStrainPoint.Stress.As(DefaultUnits.StressUnitResult),
        0);
    }

    public static IStressStrainPoint CreateFromPoint3d(Point3d point) {
      return IStressStrainPoint.Create(
        new Pressure(point.Y, DefaultUnits.StressUnitResult),
        new Strain(point.X, DefaultUnits.StrainUnitResult));
    }

    public override bool CastFrom(object source) {
      switch (source) {
        case null: return false;
        case Point3d d: {
            var temp = new AdSecStressStrainPointGoo(d);
            m_value = temp.Value;
            StressStrainPoint = temp.StressStrainPoint;
            return true;
          }

        case IStressStrainPoint point: {
            var temp = new AdSecStressStrainPointGoo(point);
            m_value = temp.Value;
            StressStrainPoint = temp.StressStrainPoint;
            return true;
          }

        case GH_Point ptGoo: {
            var temp = new AdSecStressStrainPointGoo(ptGoo.Value);
            m_value = temp.Value;
            StressStrainPoint = temp.StressStrainPoint;
            return true;
          }
      }

      var point3d = new Point3d();
      if (!GH_Convert.ToPoint3d(source, ref point3d, GH_Conversion.Both)) {
        return false;
      }

      var adSecStressStrainPointGoo = new AdSecStressStrainPointGoo(point3d);
      m_value = adSecStressStrainPointGoo.Value;
      StressStrainPoint = adSecStressStrainPointGoo.StressStrainPoint;
      return true;
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
      target = default;
      return false;
    }

    public void DrawViewportMeshes(GH_PreviewMeshArgs args) {
    }

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      DrawInstructionsList.Clear();
      DrawInstructionsList.Add(new DrawCircle() { Circle = new Circle(Value, 0.5), Color = Colour.OasysYellow, Thickness = 1, });

      DrawingHelper.Draw(args.Pipeline, DrawInstructionsList[0]);
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return new AdSecStressStrainPointGoo(new Point3d(Value));
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      return PointHelper.GetPointBoundingBox(Value, xform, 0.001, false);
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      return null;
    }

    public override string ToString() {
      var quantityStrain = new Strain(0, DefaultUnits.StrainUnitResult);
      string unitStrainAbbreviation = string.Concat(quantityStrain.ToString().Where(char.IsLetter));
      var quantityStress = new Pressure(0, DefaultUnits.StressUnitResult);
      string unitStressAbbreviation = string.Concat(quantityStress.ToString().Where(char.IsLetter));
      return
        $"AdSec {TypeName} {{{Math.Round(StressStrainPoint.Strain.As(DefaultUnits.StrainUnitResult), 4)}{unitStrainAbbreviation}, {Math.Round(StressStrainPoint.Stress.As(DefaultUnits.StressUnitResult), 4)}{unitStressAbbreviation}}}";
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      return null;
    }
  }
}
