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
using Oasys.Profiles;
using UnitsNet;

namespace GhAdSec.Parameters
{
    public class AdSecPointGoo : GH_GeometricGoo<Point3d>, IGH_PreviewData
    {
        public AdSecPointGoo(Point3d point)
        : base(point)
        {
            m_value = point;
            this.m_AdSecPoint = IPoint.Create(
                new UnitsNet.Length(m_value.Y, GhAdSec.DocumentUnits.LengthUnit), 
                new UnitsNet.Length(m_value.Z, GhAdSec.DocumentUnits.LengthUnit));
        }
        public AdSecPointGoo(AdSecPointGoo adsecPoint)
        {
            m_AdSecPoint = adsecPoint.AdSecPoint;
            this.m_value = new Point3d(Value);
        }
        public AdSecPointGoo(IPoint adsecPoint)
        {
            m_AdSecPoint = adsecPoint;
            this.m_value = new Point3d(
                0,
                m_AdSecPoint.Y.As(GhAdSec.DocumentUnits.LengthUnit),
                m_AdSecPoint.Z.As(GhAdSec.DocumentUnits.LengthUnit));
        }
        public AdSecPointGoo(UnitsNet.Length y, UnitsNet.Length z)
        {
            m_AdSecPoint = IPoint.Create(y, z);
            m_value = new Point3d(
                0,
                m_AdSecPoint.Y.As(GhAdSec.DocumentUnits.LengthUnit),
                m_AdSecPoint.Z.As(GhAdSec.DocumentUnits.LengthUnit));
        }

        public static IPoint CreateFromPoint3d(Point3d point, Plane plane)
        {
            // transform to local plane
            Rhino.Geometry.Transform mapToLocal = Rhino.Geometry.Transform.ChangeBasis(Plane.WorldYZ, plane);
            Point3d trans = new Point3d(point);
            trans.Transform(mapToLocal);
            return IPoint.Create(
                new UnitsNet.Length(trans.Y, GhAdSec.DocumentUnits.LengthUnit),
                new UnitsNet.Length(trans.Z, GhAdSec.DocumentUnits.LengthUnit));
        }
        internal static Oasys.Collections.IList<IPoint> PtsFromPolylineCurve(PolylineCurve curve)
        {
            curve.TryGetPolyline(out Polyline temp_crv);
            Plane.FitPlaneToPoints(temp_crv.ToList(), out Plane plane);
            Rhino.Geometry.Transform mapToLocal = Rhino.Geometry.Transform.ChangeBasis(Plane.WorldXY, plane);

            Oasys.Collections.IList<IPoint> pts = Oasys.Collections.IList<IPoint>.Create();
            IPoint pt = null;
            for (int j = 0; j < curve.PointCount; j++)
            {
                Point3d point3d = curve.Point(j);
                point3d.Transform(mapToLocal);
                pt = IPoint.Create(
                    new UnitsNet.Length(point3d.X, GhAdSec.DocumentUnits.LengthUnit),
                    new UnitsNet.Length(point3d.Y, GhAdSec.DocumentUnits.LengthUnit));
                pts.Add(pt);
            }
            return pts;
        }
        internal static Oasys.Collections.IList<IPoint> PtsFromPolyline(Polyline curve)
        {
            Plane.FitPlaneToPoints(curve.ToList(), out Plane plane);
            Rhino.Geometry.Transform mapToLocal = Rhino.Geometry.Transform.ChangeBasis(Plane.WorldXY, plane);

            Oasys.Collections.IList<IPoint> pts = Oasys.Collections.IList<IPoint>.Create();
            IPoint pt = null;
            for (int j = 0; j < curve.Count; j++)
            {
                Point3d point3d = curve[j];
                point3d.Transform(mapToLocal);
                pt = IPoint.Create(
                    new UnitsNet.Length(point3d.X, GhAdSec.DocumentUnits.LengthUnit),
                    new UnitsNet.Length(point3d.Y, GhAdSec.DocumentUnits.LengthUnit));
                pts.Add(pt);
            }
            return pts;
        }
        private IPoint m_AdSecPoint;
        public IPoint AdSecPoint
        {
            get { return m_AdSecPoint; }
        }

        public override string ToString()
        {
            IQuantity quantity = new UnitsNet.Length(0, GhAdSec.DocumentUnits.LengthUnit);
            string unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));
            return "AdSec " + TypeName + " {"
                + Math.Round(AdSecPoint.Y.As(GhAdSec.DocumentUnits.LengthUnit), 4) + unitAbbreviation + ", "
                + Math.Round(AdSecPoint.Z.As(GhAdSec.DocumentUnits.LengthUnit), 4) + unitAbbreviation + "}";
        }
        public override string TypeName => "Vertex";

        public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return new AdSecPointGoo(new Point3d(this.Value));
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
            if (typeof(TQ).IsAssignableFrom(typeof(AdSecPointGoo)))
            {
                target = (TQ)(object)new AdSecPointGoo(this.Value);
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

            if (typeof(TQ).IsAssignableFrom(typeof(IPoint)))
            {
                target = (TQ)(object)IPoint.Create(
                    new UnitsNet.Length(Value.X, GhAdSec.DocumentUnits.LengthUnit),
                    new UnitsNet.Length(Value.Y, GhAdSec.DocumentUnits.LengthUnit));
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
                AdSecPointGoo temp = new AdSecPointGoo((Point3d)source);
                this.m_value = temp.Value;
                this.m_AdSecPoint = temp.AdSecPoint;
                return true;
            }

            if (source is IPoint)
            {
                AdSecPointGoo temp = new AdSecPointGoo((IPoint)source);
                this.m_value = temp.Value;
                this.m_AdSecPoint = temp.AdSecPoint;
                return true;
            }

            GH_Point ptGoo = source as GH_Point;
            if (ptGoo != null)
            {
                AdSecPointGoo temp = new AdSecPointGoo(ptGoo.Value);
                this.m_value = temp.Value;
                this.m_AdSecPoint = temp.AdSecPoint;
                return true;
            }

            Point3d pt = new Point3d();
            if (GH_Convert.ToPoint3d(source, ref pt, GH_Conversion.Both))
            {
                AdSecPointGoo temp = new AdSecPointGoo(pt);
                this.m_value = temp.Value;
                this.m_AdSecPoint = temp.AdSecPoint;
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
            if (args.Color == System.Drawing.Color.FromArgb(255, 150, 0, 0)) // not selected
                args.Pipeline.DrawPoint(Value, PointStyle.RoundControlPoint, 3, GhAdSec.UI.Colour.OasysBlue);
            else
                args.Pipeline.DrawPoint(Value, PointStyle.RoundControlPoint, 5, GhAdSec.UI.Colour.OasysYellow);
        }
        public void DrawViewportMeshes(GH_PreviewMeshArgs args) { }
    }
}
