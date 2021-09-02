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
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.AdSec;

namespace GhAdSec.Parameters
{
    public class AdSecRebarGroupGoo : GH_Goo<IGroup>
    {
        public AdSecRebarGroupGoo(IGroup group)
        : base(group)
        {
            //// ### get geometry ###
            //// get centre point as xy-plane and rotate it
            //Plane xy = Plane.WorldXY;
            //xy.Origin = new Point3d(arc.Centre.Y.As(GhAdSec.DocumentUnits.LengthUnit), arc.Centre.Z.As(GhAdSec.DocumentUnits.LengthUnit), 0);
            //xy.Rotate(arc.StartAngle.As(UnitsNet.Units.AngleUnit.Radian), xy.ZAxis);

            //// create arc curve
            //Arc arcCrv = new Arc(
            //    xy,
            //    arc.Radius.As(GhAdSec.DocumentUnits.LengthUnit),
            //    arc.SweepAngle.As(UnitsNet.Units.AngleUnit.Radian));

            //// create points by dividing arc
            //List<Point3d> pts = new List<Point3d>();
            //if (arc.Layer is ILayerByBarCount)
            //{
            //    ILayerByBarCount layer = (ILayerByBarCount)arc.Layer;
            //    for (int i = 0; i < layer.Count; i++)
            //    {
            //        double t = i / (layer.Count - 1);
            //        pts.Add(arcCrv.PointAt(t));
            //    }
            //}
            //else if (arc.Layer is ILayerByBarPitch)
            //{
            //    ILayerByBarPitch layer = (ILayerByBarPitch)arc.Layer;
            //    double idealSpacing = arcCrv.Length / layer.Pitch.As(GhAdSec.DocumentUnits.LengthUnit);
            //    int spacing = (int)Math.Ceiling(idealSpacing);
            //    for (int i = 0; i < spacing; i++)
            //    {
            //        double t = i / (spacing - 1);
            //        pts.Add(arcCrv.PointAt(t));
            //    }
            //}

            //// create bar cross-section
            //List<Circle> circles = new List<Circle>();
            //foreach (Point3d pt in pts)
            //{
            //    switch (arc.Layer.BarBundle.CountPerBundle)
            //    {
            //        case 1:

            //            circles.Add(new Circle(pt, arc.Layer.BarBundle.Diameter.As(GhAdSec.DocumentUnits.LengthUnit) / 2));

            //            break;

            //        case 2:

            //            double factor = (arc.Layer.BarBundle.Diameter.As(GhAdSec.DocumentUnits.LengthUnit) / 2) /
            //                Math.Sqrt(Math.Pow(pt.X, 2) + Math.Pow(pt.Y, 2));
            //            Vector3d vec1 = new Vector3d(pt.X * factor, pt.Y * factor, 0);
            //            Point3d pt1 = new Point3d(pt);
            //            pt1.Transform(Rhino.Geometry.Transform.Translation(vec1));
            //            circles.Add(new Circle(pt1, arc.Layer.BarBundle.Diameter.As(GhAdSec.DocumentUnits.LengthUnit) / 2));
            //            Point3d pt2 = new Point3d(pt);
            //            Vector3d vec2 = new Vector3d(vec1);
            //            vec2.Reverse();
            //            pt2.Transform(Rhino.Geometry.Transform.Translation(vec2));
            //            circles.Add(new Circle(pt2, arc.Layer.BarBundle.Diameter.As(GhAdSec.DocumentUnits.LengthUnit) / 2));

            //            break;

            //        case 3:

            //    }
            //}

        }

        public override bool IsValid => true;
        public override string TypeName => "Rebar Layout";
        public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

        public override IGH_Goo Duplicate()
        {
            return new AdSecRebarGroupGoo(this.Value);
        }

        public override string ToString()
        {
            string m_ToString = "";
            try
            {
                IArcGroup arc = (IArcGroup)Value;
                m_ToString = "Arc Type Layout";

            }
            catch (Exception)
            {
                try
                {
                    ICircleGroup cir = (ICircleGroup)Value;
                    m_ToString = "Circle Type Layout";
                }
                catch (Exception)
                {
                    try
                    {
                        ILineGroup lin = (ILineGroup)Value;
                        m_ToString = "Line Type Layout";
                    }
                    catch (Exception)
                    {
                        try
                        {
                            ISingleBars sin = (ISingleBars)Value;
                            m_ToString = "SingleBars Type Layout";
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }

            return "AdSec " + TypeName + " {" + m_ToString + "}";
        }
    }
}
