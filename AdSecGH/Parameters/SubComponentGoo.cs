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
    public AdSecSubComponentGoo(ISubComponent subComponent, Plane local, IDesignCode code, string codeName, string materialName)
    : base(subComponent)
    {
      m_offset = subComponent.Offset;
      section = new AdSecSection(subComponent.Section, code, codeName, materialName, local, m_offset);
      m_plane = local;
    }
    internal AdSecSection section;
    private IPoint m_offset;
    private Plane m_plane;
    private Line previewXaxis;
    private Line previewYaxis;
    private Line previewZaxis;
    public AdSecSubComponentGoo(ISection section, Plane local, IPoint point, IDesignCode code, string codeName, string materialName)
    {
      this.m_value = ISubComponent.Create(section, point);
      m_offset = point;
      this.section = new AdSecSection(section, code, codeName, materialName, local, m_offset);
      m_plane = local;
      // local axis
      if (m_plane != null)
      {
        if (m_plane != Plane.WorldXY & local != Plane.WorldYZ & local != Plane.WorldZX)
        {
          UnitsNet.Area area = this.section.Section.Profile.Area();
          double pythogoras = Math.Sqrt(area.As(AreaUnit.SquareMeter));
          UnitsNet.Length length = new UnitsNet.Length(pythogoras * 0.15, LengthUnit.Meter);
          previewXaxis = new Line(local.Origin, local.XAxis, length.As(Units.LengthUnit));
          previewYaxis = new Line(local.Origin, local.YAxis, length.As(Units.LengthUnit));
          previewZaxis = new Line(local.Origin, local.ZAxis, length.As(Units.LengthUnit));
        }
      }
    }

    public override string ToString()
    {
      return "AdSec " + TypeName + " {" + section.ToString() + " Offset: " + m_offset.ToString() + "}";
    }
    public override string TypeName => "SubComponent";

    public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

    public override IGH_GeometricGoo DuplicateGeometry()
    {
      return new AdSecSubComponentGoo(this.Value, this.m_plane, this.section.DesignCode, this.section.codeName, this.section.materialName);
    }
    public override BoundingBox Boundingbox
    {
      get
      {
        if (Value == null) { return BoundingBox.Empty; }
        return section.SolidBrep.GetBoundingBox(false);
      }
    }
    public override BoundingBox GetBoundingBox(Transform xform)
    {
      return section.SolidBrep.GetBoundingBox(xform);
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
        target = (TQ)(object)new AdSecSectionGoo(this.section.Duplicate());
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
      if (section.SolidBrep != null)
      {
        // draw profile
        args.Pipeline.DrawBrepShaded(section.SolidBrep, section.m_profileColour);
        // draw subcomponents
        for (int i = 0; i < section.m_subProfiles.Count; i++)
        {
          args.Pipeline.DrawBrepShaded(section.m_subProfiles[i], section.m_subColours[i]);
        }
        // draw rebars
        for (int i = 0; i < section.m_rebars.Count; i++)
        {
          args.Pipeline.DrawBrepShaded(section.m_rebars[i], section.m_rebarColours[i]);
        }
      }
    }
    public void DrawViewportWires(GH_PreviewWireArgs args)
    {
      if (section == null) { return; }

      Color defaultCol = Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
      if (args.Color.R == defaultCol.R && args.Color.G == defaultCol.G && args.Color.B == defaultCol.B) // not selected
      {
        args.Pipeline.DrawPolyline(section.m_profileEdge, UI.Colour.OasysBlue, 2);
        if (section.m_profileVoidEdges != null)
        {
          foreach (Polyline crv in section.m_profileVoidEdges)
          {
            args.Pipeline.DrawPolyline(crv, UI.Colour.OasysBlue, 1);
          }
        }
        if (section.m_subEdges != null)
        {
          foreach (Polyline crv in section.m_subEdges)
          {
            args.Pipeline.DrawPolyline(crv, UI.Colour.OasysBlue, 1);
          }
        }
        if (section.m_subVoidEdges != null)
        {
          foreach (List<Polyline> crvs in section.m_subVoidEdges)
          {
            foreach (Polyline crv in crvs)
            {
              args.Pipeline.DrawPolyline(crv, UI.Colour.OasysBlue, 1);
            }
          }
        }
        if (section.m_rebarEdges != null)
        {
          foreach (Circle crv in section.m_rebarEdges)
          {
            args.Pipeline.DrawCircle(crv, Color.Black, 1);
          }
        }
      }
      else // selected
      {
        args.Pipeline.DrawPolyline(section.m_profileEdge, UI.Colour.OasysYellow, 3);
        if (section.m_profileVoidEdges != null)
        {
          foreach (Polyline crv in section.m_profileVoidEdges)
          {
            args.Pipeline.DrawPolyline(crv, UI.Colour.OasysYellow, 2);
          }
        }
        if (section.m_subEdges != null)
        {
          foreach (Polyline crv in section.m_subEdges)
          {
            args.Pipeline.DrawPolyline(crv, UI.Colour.OasysYellow, 2);
          }
        }
        if (section.m_subVoidEdges != null)
        {
          foreach (List<Polyline> crvs in section.m_subVoidEdges)
          {
            foreach (Polyline crv in crvs)
            {
              args.Pipeline.DrawPolyline(crv, UI.Colour.OasysYellow, 2);
            }
          }
        }
        if (section.m_rebarEdges != null)
        {
          foreach (Circle crv in section.m_rebarEdges)
          {
            args.Pipeline.DrawCircle(crv, UI.Colour.UILightGrey, 2);
          }
        }
      }

      // local axis
      if (previewXaxis != null)
      {
        args.Pipeline.DrawLine(previewZaxis, Color.FromArgb(255, 244, 96, 96), 1);
        args.Pipeline.DrawLine(previewXaxis, Color.FromArgb(255, 96, 244, 96), 1);
        args.Pipeline.DrawLine(previewYaxis, Color.FromArgb(255, 96, 96, 234), 1);
      }
    }
  }
}
