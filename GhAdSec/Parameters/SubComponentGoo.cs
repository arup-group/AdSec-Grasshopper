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
using UnitsNet.Units;
using Oasys.AdSec;
using Oasys.AdSec.DesignCode;

namespace AdSecGH.Parameters
{
    public class AdSecSubComponentGoo : GH_GeometricGoo<ISubComponent>, IGH_PreviewData
    {
        public AdSecSubComponentGoo(ISubComponent subComponent, Plane local, IDesignCode code)
        : base(subComponent)
        {
            m_offset = subComponent.Offset;
            m_sectionGoo = new AdSecSection(subComponent.Section, code, local, m_offset);
            m_plane = local;
        }
        private AdSecSection m_sectionGoo;
        private IPoint m_offset;
        private Plane m_plane;
        private Line previewXaxis;
        private Line previewYaxis;
        private Line previewZaxis;
        public AdSecSubComponentGoo(ISection section, Plane local, IPoint point, IDesignCode code)
        {
            this.m_value = ISubComponent.Create(section, point);
            m_offset = point;
            m_sectionGoo = new AdSecSection(section, code, local, m_offset);
            m_plane = local;
            // local axis
            if (m_plane != null)
            {
                if (m_plane != Plane.WorldXY & local != Plane.WorldYZ & local != Plane.WorldZX)
                {
                    UnitsNet.Area area = this.m_sectionGoo.Section.Profile.Area();
                    double pythogoras = Math.Sqrt(area.As(UnitsNet.Units.AreaUnit.SquareMeter));
                    UnitsNet.Length length = new UnitsNet.Length(pythogoras * 0.15, UnitsNet.Units.LengthUnit.Meter);
                    previewXaxis = new Line(local.Origin, local.XAxis, length.As(DocumentUnits.LengthUnit));
                    previewYaxis = new Line(local.Origin, local.YAxis, length.As(DocumentUnits.LengthUnit));
                    previewZaxis = new Line(local.Origin, local.ZAxis, length.As(DocumentUnits.LengthUnit));
                }
            }
        }

        public override string ToString()
        {
            return "AdSec " + TypeName + " {" + m_sectionGoo.ToString() + " Offset: " + m_offset.ToString() + "}";
        }
        public override string TypeName => "SubComponent";

        public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return new AdSecSubComponentGoo(this.Value, this.m_plane, this.m_sectionGoo.DesignCode);
        }
        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null) { return BoundingBox.Empty; }
                return m_sectionGoo.SolidBrep.GetBoundingBox(false);
            }
        }
        public override BoundingBox GetBoundingBox(Transform xform)
        {
            return m_sectionGoo.SolidBrep.GetBoundingBox(xform);
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
            if (typeof(TQ).IsAssignableFrom(typeof(AdSecSubComponentGoo)))
            {
                target = (TQ)(object)this.Duplicate();
                return true;
            }

            if (typeof(TQ).IsAssignableFrom(typeof(AdSecSectionGoo)))
            {
                target = (TQ)(object)new AdSecSectionGoo(this.m_sectionGoo.Duplicate());
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
        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            //Draw shape.
            if (m_sectionGoo.SolidBrep != null)
            {
                // draw profile
                args.Pipeline.DrawBrepShaded(m_sectionGoo.SolidBrep, m_sectionGoo.m_profileColour);
                // draw subcomponents
                for (int i = 0; i < m_sectionGoo.m_subProfiles.Count; i++)
                {
                    args.Pipeline.DrawBrepShaded(m_sectionGoo.m_subProfiles[i], m_sectionGoo.m_subColours[i]);
                }
                // draw rebars
                for (int i = 0; i < m_sectionGoo.m_rebars.Count; i++)
                {
                    args.Pipeline.DrawBrepShaded(m_sectionGoo.m_rebars[i], m_sectionGoo.m_rebarColours[i]);
                }
            }
        }
        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (m_sectionGoo == null) { return; }

            Color defaultCol = Grasshopper.Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
            if (args.Color.R == defaultCol.R && args.Color.G == defaultCol.G && args.Color.B == defaultCol.B) // not selected
            {
                args.Pipeline.DrawPolyline(m_sectionGoo.m_profileEdge, AdSecGH.UI.Colour.OasysBlue, 2);
                if (m_sectionGoo.m_profileVoidEdges != null)
                {
                    foreach (Polyline crv in m_sectionGoo.m_profileVoidEdges)
                    {
                        args.Pipeline.DrawPolyline(crv, AdSecGH.UI.Colour.OasysBlue, 1);
                    }
                }
                if (m_sectionGoo.m_subEdges != null)
                {
                    foreach (Polyline crv in m_sectionGoo.m_subEdges)
                    {
                        args.Pipeline.DrawPolyline(crv, AdSecGH.UI.Colour.OasysBlue, 1);
                    }
                }
                if (m_sectionGoo.m_subVoidEdges != null)
                {
                    foreach (List<Polyline> crvs in m_sectionGoo.m_subVoidEdges)
                    {
                        foreach (Polyline crv in crvs)
                        {
                            args.Pipeline.DrawPolyline(crv, AdSecGH.UI.Colour.OasysBlue, 1);
                        }
                    }
                }
                if (m_sectionGoo.m_rebarEdges != null)
                {
                    foreach (Circle crv in m_sectionGoo.m_rebarEdges)
                    {
                        args.Pipeline.DrawCircle(crv, Color.Black, 1);
                    }
                }
            }
            else // selected
            {
                args.Pipeline.DrawPolyline(m_sectionGoo.m_profileEdge, AdSecGH.UI.Colour.OasysYellow, 3);
                if (m_sectionGoo.m_profileVoidEdges != null)
                {
                    foreach (Polyline crv in m_sectionGoo.m_profileVoidEdges)
                    {
                        args.Pipeline.DrawPolyline(crv, AdSecGH.UI.Colour.OasysYellow, 2);
                    }
                }
                if (m_sectionGoo.m_subEdges != null)
                {
                    foreach (Polyline crv in m_sectionGoo.m_subEdges)
                    {
                        args.Pipeline.DrawPolyline(crv, AdSecGH.UI.Colour.OasysYellow, 2);
                    }
                }
                if (m_sectionGoo.m_subVoidEdges != null)
                {
                    foreach (List<Polyline> crvs in m_sectionGoo.m_subVoidEdges)
                    {
                        foreach (Polyline crv in crvs)
                        {
                            args.Pipeline.DrawPolyline(crv, AdSecGH.UI.Colour.OasysYellow, 2);
                        }
                    }
                }
                if (m_sectionGoo.m_rebarEdges != null)
                {
                    foreach (Circle crv in m_sectionGoo.m_rebarEdges)
                    {
                        args.Pipeline.DrawCircle(crv, AdSecGH.UI.Colour.UILightGrey, 2);
                    }
                }
            }

            // local axis
            if (previewXaxis != null)
            {
                args.Pipeline.DrawLine(previewZaxis, System.Drawing.Color.FromArgb(255, 244, 96, 96), 1);
                args.Pipeline.DrawLine(previewXaxis, System.Drawing.Color.FromArgb(255, 96, 244, 96), 1);
                args.Pipeline.DrawLine(previewYaxis, System.Drawing.Color.FromArgb(255, 96, 96, 234), 1);
            }
        }
    }
}
