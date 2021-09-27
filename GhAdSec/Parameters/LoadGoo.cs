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
using Oasys.AdSec.Materials;
using Oasys.Profiles;
using Oasys.AdSec;
using UnitsNet;

namespace AdSecGH.Parameters
{
    public class AdSecLoadGoo : GH_GeometricGoo<ILoad>, IGH_PreviewData
    {
        public AdSecLoadGoo(ILoad load)
        : base(load)
        {
        }
        private Point3d m_point = Point3d.Unset;
        public AdSecLoadGoo(ILoad load, Plane local)
        {
            this.m_value = load;
            Point3d point = new Point3d(
                load.ZZ.As(DocumentUnits.MomentUnit),
                load.YY.As(DocumentUnits.MomentUnit),
                load.X.As(DocumentUnits.ForceUnit));
            Rhino.Geometry.Transform mapFromLocal = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldXY, local);
            point.Transform(mapFromLocal);
            m_point = point;
        }
        public override bool IsValid => true;

        public override string TypeName => "Load";

        public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

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

        public override IGH_Goo Duplicate()
        {
            return new AdSecLoadGoo(this.Value);
        }
        public override string ToString()
        {
            IQuantity quantityMoment = new Oasys.Units.Moment(0, DocumentUnits.MomentUnit);
            string unitMomentAbbreviation = string.Concat(quantityMoment.ToString().Where(char.IsLetter));
            IQuantity quantityForce = new UnitsNet.Force(0, DocumentUnits.ForceUnit);
            string unitforceAbbreviation = string.Concat(quantityForce.ToString().Where(char.IsLetter));
            return "AdSec " + TypeName + " {"
                + Math.Round(this.Value.X.As(DocumentUnits.ForceUnit), 4) + unitforceAbbreviation + ", "
                + Math.Round(this.Value.YY.As(DocumentUnits.MomentUnit), 4) + unitMomentAbbreviation + ", "
                + Math.Round(this.Value.ZZ.As(DocumentUnits.MomentUnit), 4) + unitMomentAbbreviation + "}";
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
                    new UnitsNet.Force(point.X, DocumentUnits.ForceUnit),
                    new Oasys.Units.Moment(point.Y, DocumentUnits.MomentUnit),
                    new Oasys.Units.Moment(point.Z, DocumentUnits.MomentUnit));
                AdSecLoadGoo temp = new AdSecLoadGoo(load);
                this.Value = temp.Value;
                return true;
            }

            GH_Point ptGoo = source as GH_Point;
            if (ptGoo != null)
            {
                Point3d point = ptGoo.Value;
                ILoad load = ILoad.Create(
                    new UnitsNet.Force(point.X, DocumentUnits.ForceUnit),
                    new Oasys.Units.Moment(point.Y, DocumentUnits.MomentUnit),
                    new Oasys.Units.Moment(point.Z, DocumentUnits.MomentUnit));
                AdSecLoadGoo temp = new AdSecLoadGoo(load);
                this.Value = temp.Value;
                return true;
            }

            Point3d pt = new Point3d();
            if (GH_Convert.ToPoint3d(source, ref pt, GH_Conversion.Both))
            {
                Point3d point = pt;
                ILoad load = ILoad.Create(
                    new UnitsNet.Force(point.X, DocumentUnits.ForceUnit),
                    new Oasys.Units.Moment(point.Y, DocumentUnits.MomentUnit),
                    new Oasys.Units.Moment(point.Z, DocumentUnits.MomentUnit));
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
                Color defaultCol = Grasshopper.Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
                if (args.Color.R == defaultCol.R && args.Color.G == defaultCol.G && args.Color.B == defaultCol.B) // not selected
                    args.Pipeline.DrawPoint(m_point, PointStyle.X, 7, AdSecGH.UI.Colour.ArupRed);
                else
                    args.Pipeline.DrawPoint(m_point, PointStyle.X, 8, AdSecGH.UI.Colour.GsaLightGrey);
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
