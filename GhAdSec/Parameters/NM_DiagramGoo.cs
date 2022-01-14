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
using AdSecGH.Helpers;

namespace AdSecGH.Parameters
{
    public class AdSecNMMCurveGoo : GH_GeometricGoo<Polyline>, IGH_PreviewData
    {
        public AdSecNMMCurveGoo(Polyline curve, ILoadCurve loadCurve, NMM_InteractionCurveType interactionType, Rectangle3d plotBoundary)
        : base(curve)
        {
            if (loadCurve == null) { return; }

            m_type = interactionType;
            LoadCurve = loadCurve;
            m_value = curve;
            m_plotBounds = plotBoundary;
            UpdatePreview(m_plotBounds);
        }
        private List<Line> m_grids = new List<Line>();
        private List<Line> m_axes = new List<Line>();
        private List<Text3d> m_txts;
        private Rectangle3d m_plotBounds;
        private void UpdatePreview(Rectangle3d plotBoundary)
        {
            // get bounding box of load curve polyline
            BoundingBox unitbbox = this.m_value.BoundingBox;
            // get bounding box of plot boundary
            BoundingBox plotbbox = plotBoundary.BoundingBox;

            // create axes
            Diagram.GridAxis xAxis = new Diagram.GridAxis((float)unitbbox.PointAt(0, 0, 0).X, (float)unitbbox.PointAt(1, 0, 0).X);
            Diagram.GridAxis yAxis = new Diagram.GridAxis((float)unitbbox.PointAt(0, 0, 0).Y, (float)unitbbox.PointAt(0, 1, 0).Y);

            // move to plot boundary
            Vector3d translate = new Vector3d(
              plotbbox.PointAt(0, 0, 0).X - xAxis.min_value,
              plotbbox.PointAt(0, 0, 0).Y - yAxis.min_value,
              0);
            this.m_value.Transform(Rhino.Geometry.Transform.Translation(translate));


            // set plane for NU scaling operation
            Plane pln = Plane.WorldXY;
            pln.Origin = plotbbox.PointAt(0, 0, 0);

            // calculate x-factor
            double sclX = (plotbbox.PointAt(1, 0, 0).X - plotbbox.PointAt(0, 0, 0).X) /
              (xAxis.max_value - xAxis.min_value);

            // calculate y-factor
            double sclY = (plotbbox.PointAt(0, 1, 0).Y - plotbbox.PointAt(0, 0, 0).Y) /
              (yAxis.max_value - yAxis.min_value);

            // scale unit polyline to fit in plot boundary
            this.m_value.Transform(Rhino.Geometry.Transform.Scale(
              pln, sclX, sclY, 1));

            // set annotation text size
            double size = Math.Min(
                Math.Abs(plotbbox.PointAt(1, 0, 0).X - plotbbox.PointAt(0, 0, 0).X),
                Math.Abs(plotbbox.PointAt(0, 1, 0).Y - plotbbox.PointAt(0, 0, 0).Y)) / 50;

            // create grid lines
            m_grids = new List<Line>();
            m_axes = new List<Line>();
            m_txts = new List<Text3d>();
            Plane txtPln = Plane.WorldXY;
            // loop through all values in y axis to create x-dir grids
            int item = (m_type == NMM_InteractionCurveType.NM) ? yAxis.MajorRange.Length - 1 : 0;
            foreach (float step in yAxis.MajorRange)
            {
                // create gridline in original unit
                Line grid = new Line(
                  new Point3d(xAxis.min_value, step, 0),
                  new Point3d(xAxis.max_value, step, 0));
                // move to plot boundary
                grid.Transform(Rhino.Geometry.Transform.Translation(translate));
                // scale to plot boundary
                grid.Transform(Rhino.Geometry.Transform.Scale(
                  pln, sclX, sclY, 1));
                // if step value is 0 we want to add it to the major axis 
                // that we will give a different colour
                if (step == 0)
                    m_axes.Add(grid);
                else
                    m_grids.Add(grid);

                // add step annotation 
                txtPln.Origin = new Point3d(
                    grid.PointAt(0).X - (xAxis.major_step / 2) * sclX,
                    grid.PointAt(0).Y, 0);

                // add step annotation 
                txtPln.Origin = new Point3d(
                  grid.PointAt(0).X - size,
                  grid.PointAt(0).Y, 0);
                string displayval = (m_type == NMM_InteractionCurveType.NM) ? (step * -1).ToString() : step.ToString();
                Text3d txt = new Text3d(displayval, txtPln, size);
                txt.HorizontalAlignment = TextHorizontalAlignment.Right;
                txt.VerticalAlignment = TextVerticalAlignment.Middle;
                m_txts.Add(txt);
            }
            
            // do the same as above but for the other axis
            foreach (float step in xAxis.MajorRange)
            {
                Line grid = new Line(
                  new Point3d(step, yAxis.min_value, 0),
                  new Point3d(step, yAxis.max_value, 0));
                grid.Transform(Rhino.Geometry.Transform.Translation(translate));
                grid.Transform(Rhino.Geometry.Transform.Scale(
                  pln, sclX, sclY, 1));
                if (step == 0)
                    m_axes.Add(grid);
                else
                    m_grids.Add(grid);

                txtPln.Origin = new Point3d(
                  grid.PointAt(0).X,
                  grid.PointAt(0).Y - size, 0);
                Text3d txt = new Text3d(step.ToString(), txtPln, size);
                txt.HorizontalAlignment = TextHorizontalAlignment.Center;
                txt.VerticalAlignment = TextVerticalAlignment.Top;
                m_txts.Add(txt);
            }
            // add the boundary lines
            m_axes.AddRange(plotBoundary.ToPolyline().GetSegments());

            // Create axis labels
            string momentAxis = "Moment [" + Oasys.Units.Moment.GetAbbreviation(Units.MomentUnit) + "]";
            string myyAxis = "Myy [" + Oasys.Units.Moment.GetAbbreviation(Units.MomentUnit) + "]";
            string mzzAxis = "Mzz [" + Oasys.Units.Moment.GetAbbreviation(Units.MomentUnit) + "]";
            IQuantity force = new Force(0, Units.ForceUnit);
            string forceUnitAbbreviation = string.Concat(force.ToString().Where(char.IsLetter));
            string forceAxis = "Axial force [" + forceUnitAbbreviation + "]";

            string annoXaxis = (m_type == NMM_InteractionCurveType.NM) ? momentAxis : myyAxis;
            string annoYaxis = (m_type == NMM_InteractionCurveType.NM) ? forceAxis : mzzAxis;

            // find largest annotation value string length
            double offset = Math.Max(
                Math.Abs(Math.Min(xAxis.min_value, yAxis.min_value)).ToString().Length + 1,
                Math.Max(xAxis.max_value, yAxis.max_value).ToString().Length);

            txtPln.Origin = new Point3d(
              plotbbox.PointAt(0.5, 0, 0).X,
              plotbbox.PointAt(0, 0, 0).Y - size * 3, 0);
            Text3d txtX = new Text3d(annoXaxis, txtPln, size);
            txtX.HorizontalAlignment = TextHorizontalAlignment.Center;
            txtX.VerticalAlignment = TextVerticalAlignment.Top;
            m_txts.Add(txtX);
            txtPln.Origin = new Point3d(
              plotbbox.PointAt(0, 0, 0).X - size * offset * 1.1,
              plotbbox.PointAt(0, 0.5, 0).Y, 0);
            txtPln.Rotate(Math.PI / 2, Vector3d.ZAxis);
            Text3d txtY = new Text3d(annoYaxis, txtPln, size);
            txtY.HorizontalAlignment = TextHorizontalAlignment.Center;
            txtY.VerticalAlignment = TextVerticalAlignment.Bottom;
            m_txts.Add(txtY);

        }

        /// <summary>
        /// this method will create an M-M type interaction diagram
        /// </summary>
        /// <param name="loadCurve"></param>
        internal AdSecNMMCurveGoo(ILoadCurve loadCurve, Rectangle3d plotBoundary)
        {
            if (loadCurve == null) { return; }

            m_type = NMM_InteractionCurveType.MM;
            LoadCurve = loadCurve;

            List<Point3d> pts = new List<Point3d>();
            foreach (ILoad load in loadCurve.Points)
            {
                Point3d pt = new Point3d(
                    load.YY.As(Units.MomentUnit), // plot yy on x-axis
                    load.ZZ.As(Units.MomentUnit), // plot zz on y-axis
                    0);
                pts.Add(pt);
            }
            // add first point to the end to make a closed curve
            pts.Add(pts[0]);

            m_value = new Polyline(pts);
            m_plotBounds = plotBoundary;
            UpdatePreview(m_plotBounds);
        }
        /// <summary>
        /// this method will create an N-M type interaction diagram
        /// </summary>
        /// <param name="loadCurve"></param>
        /// <param name="angle"></param>
        internal AdSecNMMCurveGoo(ILoadCurve loadCurve, Angle angle, Rectangle3d plotBoundary)
        {
            if (loadCurve == null) { return; }

            LoadCurve = loadCurve;
            m_type = NMM_InteractionCurveType.NM;

            List<Point3d> pts = new List<Point3d>();
            foreach (ILoad load in loadCurve.Points)
            {
                Point3d pt = new Point3d(
                    load.ZZ.As(Units.MomentUnit),
                    load.YY.As(Units.MomentUnit),
                    load.X.As(Units.ForceUnit) * -1); // flip y-axis for NM-diagram
                pts.Add(pt);
            }
            // add first point to the end to make a closed curve
            pts.Add(pts[0]);

            
            Plane local = Plane.WorldYZ;
            if (angle.Radians != 0)
                local.Rotate(angle.Radians * -1, Vector3d.ZAxis);

            // transform to local plane
            Transform mapFromLocal = Rhino.Geometry.Transform.PlaneToPlane(local, Plane.WorldXY);

            m_value = new Polyline(pts);
            m_value.Transform(mapFromLocal);
            m_plotBounds = plotBoundary;
            UpdatePreview(m_plotBounds);
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
            return new AdSecNMMCurveGoo(this.m_value.Duplicate(), this.LoadCurve, this.m_type, this.m_plotBounds);
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
                target = (TQ)(object)new AdSecNMMCurveGoo(this.m_value.Duplicate(), this.LoadCurve, this.m_type, this.m_plotBounds);
                return true;
            }

            if (typeof(TQ).IsAssignableFrom(typeof(Line)))
            {
                target = (TQ)(object)Value;
                return true;
            }

            if (typeof(TQ).IsAssignableFrom(typeof(GH_Curve)))
            {
                PolylineCurve pln = this.m_value.ToPolylineCurve();
                target = (TQ)(object)new GH_Curve(pln);
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
                // draw diagram polyline
                if (args.Color == Color.FromArgb(255, 150, 0, 0)) //Grasshopper.Instances.Settings.GetValue("DefaultPreviewColourSelected", System.Drawing.Color.White))
                    args.Pipeline.DrawPolyline(Value, UI.Colour.OasysBlue, 2);
                else
                    args.Pipeline.DrawPolyline(Value, UI.Colour.OasysYellow, 2);

                // draw plot diagram
                args.Pipeline.DrawLines(m_grids, UI.Colour.OasysDarkGrey, 1);
                args.Pipeline.DrawLines(m_axes, UI.Colour.OasysDarkGrey, 2);
                foreach (Text3d txt in m_txts)
                    args.Pipeline.Draw3dText(txt, UI.Colour.OasysDarkGrey);
            }
        }
        public void DrawViewportMeshes(GH_PreviewMeshArgs args) { }
    }
}
