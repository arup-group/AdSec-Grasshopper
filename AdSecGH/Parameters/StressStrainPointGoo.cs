using System;
using System.Collections;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.IO;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using Rhino.DocObjects;
using Rhino.Collections;
using GH_IO;
using GH_IO.Serialization;
using Rhino.Display;
using Oasys.AdSec.Materials.StressStrainCurves;
using UnitsNet;

namespace AdSecGH.Parameters
{
  public class AdSecStressStrainPointGoo : GH_GeometricGoo<Point3d>, IGH_PreviewData
  {
    public AdSecStressStrainPointGoo(Point3d point)
    : base(point)
    {
      m_value = point;
      this.m_SSpoint = IStressStrainPoint.Create(
          new Pressure(m_value.Y, Units.StressUnit),
          new Oasys.Units.Strain(m_value.X, Units.StrainUnit));
    }
    public AdSecStressStrainPointGoo(AdSecStressStrainPointGoo stressstrainPoint)
    {
      m_SSpoint = stressstrainPoint.StressStrainPoint;
      this.m_value = new Point3d(Value);
    }
    public AdSecStressStrainPointGoo(IStressStrainPoint stressstrainPoint)
    {
      m_SSpoint = stressstrainPoint;
      this.m_value = new Point3d(
          m_SSpoint.Strain.As(Units.StrainUnit),
          m_SSpoint.Stress.As(Units.StressUnit),
          0);
    }
    public AdSecStressStrainPointGoo(Pressure stress, Oasys.Units.Strain strain)
    {
      m_SSpoint = IStressStrainPoint.Create(stress, strain);
      m_value = new Point3d(
          m_SSpoint.Strain.As(Units.StrainUnit),
          m_SSpoint.Stress.As(Units.StressUnit),
          0);
    }

    public static IStressStrainPoint CreateFromPoint3d(Point3d point)
    {
      return IStressStrainPoint.Create(
          new Pressure(point.Y, Units.StressUnit),
          new Oasys.Units.Strain(point.X, Units.StrainUnit));
    }

    private IStressStrainPoint m_SSpoint;
    public IStressStrainPoint StressStrainPoint
    {
      get { return m_SSpoint; }
    }

    public override string ToString()
    {
      IQuantity quantityStrain = new Oasys.Units.Strain(0, Units.StrainUnit);
      string unitStrainAbbreviation = string.Concat(quantityStrain.ToString().Where(char.IsLetter));
      IQuantity quantityStress = new Pressure(0, Units.StressUnit);
      string unitStressAbbreviation = string.Concat(quantityStress.ToString().Where(char.IsLetter));
      return "AdSec " + TypeName + " {"
          + Math.Round(StressStrainPoint.Strain.As(Units.StrainUnit), 4) + unitStrainAbbreviation + ", "
          + Math.Round(StressStrainPoint.Stress.As(Units.StressUnit), 4) + unitStressAbbreviation + "}";
    }
    public override string TypeName => "StressStrainPoint";

    public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

    public override IGH_GeometricGoo DuplicateGeometry()
    {
      return new AdSecStressStrainPointGoo(new Point3d(this.Value));
    }
    public override BoundingBox Boundingbox
    {
      get
      {
        if (Value == null) { return BoundingBox.Empty; }
        Point3d pt1 = new Point3d(Value);
        pt1.Z += 0.25;
        Point3d pt2 = new Point3d(Value);
        pt2.Z += -0.25;
        Line ln = new Line(pt1, pt2);
        LineCurve crv = new LineCurve(ln);
        return crv.GetBoundingBox(false);
      }
    }
    public override BoundingBox GetBoundingBox(Transform xform)
    {
      if (Value == null) { return BoundingBox.Empty; }
      Point3d pt = new Point3d(Value);
      pt.Z += 0.001;
      Line ln = new Line(Value, pt);
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

    public override object ScriptVariable()
    {
      return Value;
    }
    public override bool CastTo<TQ>(out TQ target)
    {
      if (typeof(TQ).IsAssignableFrom(typeof(AdSecStressStrainPointGoo)))
      {
        target = (TQ)(object)new AdSecStressStrainPointGoo(this.Value);
        return true;
      }

      if (typeof(TQ).IsAssignableFrom(typeof(Point3d)))
      {
        target = (TQ)(object)Value;
        return true;
      }

      if (typeof(TQ).IsAssignableFrom(typeof(GH_Point)))
      {
        target = (TQ)(object)new GH_Point(Value);
        return true;
      }

      if (typeof(TQ).IsAssignableFrom(typeof(IStressStrainPoint)))
      {
        target = (TQ)(object)IStressStrainPoint.Create(
            new Pressure(Value.Y, Units.StressUnit),
            new Oasys.Units.Strain(Value.X, Units.StrainUnit));
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
        AdSecStressStrainPointGoo temp = new AdSecStressStrainPointGoo((Point3d)source);
        this.m_value = temp.Value;
        this.m_SSpoint = temp.StressStrainPoint;
        return true;
      }

      if (source is IStressStrainPoint)
      {
        AdSecStressStrainPointGoo temp = new AdSecStressStrainPointGoo((IStressStrainPoint)source);
        this.m_value = temp.Value;
        this.m_SSpoint = temp.StressStrainPoint;
        return true;
      }

      GH_Point ptGoo = source as GH_Point;
      if (ptGoo != null)
      {
        AdSecStressStrainPointGoo temp = new AdSecStressStrainPointGoo(ptGoo.Value);
        this.m_value = temp.Value;
        this.m_SSpoint = temp.StressStrainPoint;
        return true;
      }

      Point3d pt = new Point3d();
      if (GH_Convert.ToPoint3d(source, ref pt, GH_Conversion.Both))
      {
        AdSecStressStrainPointGoo temp = new AdSecStressStrainPointGoo(pt);
        this.m_value = temp.Value;
        this.m_SSpoint = temp.StressStrainPoint;
        return true;
      }

      return false;
    }

    public BoundingBox ClippingBox
    {
      get { return Boundingbox; }
    }
    public void DrawViewportWires(GH_PreviewWireArgs args)
    {
      if (Value != null)
      {
        args.Pipeline.DrawCircle(new Circle(Value, 0.5), UI.Colour.OasysYellow, 1);
      }
    }
    public void DrawViewportMeshes(GH_PreviewMeshArgs args) { }
  }
}
