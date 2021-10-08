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
using Oasys.AdSec.Mesh;
using Oasys.AdSec;
using UnitsNet;

namespace AdSecGH.Parameters
{
    public class AdSecNMMCurveGoo : GH_GeometricGoo<Polyline>, IGH_PreviewData
    {
        public AdSecNMMCurveGoo(Polyline curve, ILoadCurve loadCurve, NMM_InteractionCurveType interactionType)
        : base(curve)
        {
            if (loadCurve == null) { return; }

            m_type = interactionType;
            LoadCurve = loadCurve;
            m_value = curve;
        }

        internal AdSecNMMCurveGoo(ILoadCurve loadCurve)
        {
            if (loadCurve == null) { return; }

            m_type = NMM_InteractionCurveType.MM;
            LoadCurve = loadCurve;

            List<Point3d> pts = new List<Point3d>();
            foreach (ILoad load in loadCurve.Points)
            {
                Point3d pt = new Point3d(
                    load.ZZ.As(DocumentUnits.MomentUnit),
                    load.YY.As(DocumentUnits.MomentUnit),
                    0);
                pts.Add(pt);
            }
            // add first point to the end to make a closed curve
            pts.Add(pts[0]);

            m_value = new Polyline(pts);
        }
        internal AdSecNMMCurveGoo(ILoadCurve loadCurve, Angle angle)
        {
            if (loadCurve == null) { return; }

            LoadCurve = loadCurve;
            m_type = NMM_InteractionCurveType.NM;

            List<Point3d> pts = new List<Point3d>();
            foreach (ILoad load in loadCurve.Points)
            {
                Point3d pt = new Point3d(
                    load.X.As(DocumentUnits.ForceUnit),
                    load.ZZ.As(DocumentUnits.MomentUnit),
                    load.YY.As(DocumentUnits.MomentUnit));
                pts.Add(pt);
            }
            // add first point to the end to make a closed curve
            pts.Add(pts[0]);

            
            Plane local = Plane.WorldYZ;
            local.Rotate(angle.Radians, Vector3d.ZAxis);

            // transform to local plane
            Rhino.Geometry.Transform mapFromLocal = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldYZ, local);

            m_value = new Polyline(pts);
            m_value.Transform(mapFromLocal);
        }

        public enum NMM_InteractionCurveType
        {
            NM,
            MM
        }
        internal ILoadCurve LoadCurve;
        private NMM_InteractionCurveType m_type;
        public override string ToString()
        {
            string interactionType = "";
            if (m_type == NMM_InteractionCurveType.NM)
                interactionType = "N-M (Force-Moment Interaction)";
            else
                interactionType = "M-M (Moment-Moment Interaction)";
            return "AdSec " + TypeName + " {" + interactionType + "}";
        }
        public override string TypeName => (m_type == NMM_InteractionCurveType.NM) ? "N-M" : "M-M";

        public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return new AdSecNMMCurveGoo(this.m_value.Duplicate(), this.LoadCurve, this.m_type);
        }
        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null) { return BoundingBox.Empty; }
                return Value.BoundingBox;
            }
        }
        public override BoundingBox GetBoundingBox(Transform xform)
        {
            Polyline dup = this.m_value.Duplicate();
            dup.Transform(xform);
            return dup.BoundingBox;
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
            if (typeof(TQ).IsAssignableFrom(typeof(AdSecNMMCurveGoo)))
            {
                target = (TQ)(object)new AdSecNMMCurveGoo(this.m_value.Duplicate(), this.LoadCurve, this.m_type);
                return true;
            }

            if (typeof(TQ).IsAssignableFrom(typeof(Line)))
            {
                target = (TQ)(object)Value;
                return true;
            }

            if (typeof(TQ).IsAssignableFrom(typeof(GH_Curve)))
            {
                target = (TQ)(object)new GH_Curve(this.m_value.ToPolylineCurve());
                return true;
            }

            target = default(TQ);
            return false;
        }
        public override bool CastFrom(object source)
        {
            if (source == null) return false;

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
                if (args.Color == Grasshopper.Instances.Settings.GetValue("DefaultPreviewColour", System.Drawing.Color.White))
                    args.Pipeline.DrawPolyline(Value, AdSecGH.UI.Colour.OasysBlue, 2);
                else
                    args.Pipeline.DrawPolyline(Value, AdSecGH.UI.Colour.OasysYellow, 2);
            }
        }
        public void DrawViewportMeshes(GH_PreviewMeshArgs args) { }
    }
}
