using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace AdSecGH.Parameters
{
  public class AdSecSectionGoo : GH_GeometricGoo<AdSecSection>, IGH_PreviewData
  {
    #region properties
    public override string TypeName => "Section";
    public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";
    public override BoundingBox Boundingbox
    {
      get
      {
        if (this.Value == null)
          return BoundingBox.Empty;
        if (this.Value.SolidBrep == null)
          return BoundingBox.Empty;
        return this.Value.SolidBrep.GetBoundingBox(false);
      }
    }
    public BoundingBox ClippingBox
    {
      get
      {
        return this.Boundingbox;
      }
    }
    public override bool IsValid
    {
      get
      {
        if (this.Value == null)
          return false;
        if (this.Value.SolidBrep == null || this.Value.IsValid == false)
          return false;
        return true;
      }
    }
    public override string IsValidWhyNot
    {
      get
      {
        if (this.Value.IsValid)
          return string.Empty;
        return this.Value.IsValid.ToString();
      }
    }
    public override string ToString()
    {
      if (this.Value == null)
        return "Null AdSec Section";
      else
        return "AdSec " + this.TypeName + " {" + this.Value.ToString() + "}";
    }

    #endregion

    #region constructors
    public AdSecSectionGoo()
    {
    }

    public AdSecSectionGoo(AdSecSection section)
    {
      if (section == null)
        this.Value = null;
      else
        this.Value = section.Duplicate();
    }

    public override IGH_GeometricGoo DuplicateGeometry()
    {
      return DuplicateAdSecSection();
    }

    public AdSecSectionGoo DuplicateAdSecSection()
    {
      if (this.Value == null)
        return null;
      else
        return new AdSecSectionGoo(Value.Duplicate());
    }
    #endregion

    public override BoundingBox GetBoundingBox(Transform xform)
    {
      if (this.Value == null)
        return BoundingBox.Empty;
      if (this.Value.SolidBrep == null)
        return BoundingBox.Empty;
      return this.Value.SolidBrep.GetBoundingBox(xform);
    }

    #region casting methods
    public override bool CastTo<Q>(out Q target)
    {
      //if (InteropAdSecComputeTypes.IsPresent())
      //{
      //  Type type = InteropAdSecComputeTypes.GetType(typeof(IAdSecSection));
      //  if (typeof(Q).IsAssignableFrom(type))
      //  {
      //    if (Value == null)
      //      target = default;
      //    else
      //    {
      //      target = (Q)(object)InteropAdSecComputeTypes.CastToSection(Value.Section, Value.codeName, Value.materialName);
      //    }
      //    return true;
      //  }
      //}
      if (typeof(Q).IsAssignableFrom(typeof(AdSecSectionGoo)))
      {
        if (this.Value == null)
          target = default;
        else
          target = (Q)(object)this.Value.Duplicate();
        return true;
      }
      if (typeof(Q).IsAssignableFrom(typeof(AdSecSection)))
      {
        if (this.Value == null)
          target = default;
        else
          target = (Q)(object)new AdSecSection(this.Value.Section, this.Value.DesignCode, this.Value.codeName, this.Value.materialName, this.Value.LocalPlane);
        return true;
      }
      if (typeof(Q).IsAssignableFrom(typeof(AdSecProfileGoo)))
      {
        if (this.Value == null)
          target = default;
        else
          target = (Q)(object)new AdSecProfileGoo(this.Value.Section.Profile, this.Value.LocalPlane);
        return true;
      }
      if (typeof(Q).IsAssignableFrom(typeof(Brep)))
      {
        if (this.Value == null)
          target = default;
        else
          target = (Q)(object)this.Value.SolidBrep.DuplicateBrep();
        return true;
      }
      if (typeof(Q).IsAssignableFrom(typeof(GH_Brep)))
      {
        if (this.Value == null)
          target = default;
        else
          target = (Q)(object)this.Value.SolidBrep.DuplicateBrep();
        return true;
      }

      target = default;
      return false;
    }

    public override bool CastFrom(object source)
    {
      if (source == null)
        return false;

      return false;
    }
    #endregion

    #region transformation methods
    public override IGH_GeometricGoo Transform(Transform xform)
    {
      //return new AdSecSectionGoo(Value.Transform(xform));
      return null;
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
      //return new AdSecSectionGoo(Value.Morph(xmorph));
      return null;
    }
    #endregion

    #region drawing methods
    public void DrawViewportMeshes(GH_PreviewMeshArgs args)
    {
      //Draw shape.
      if (this.Value.SolidBrep != null)
      {
        // draw profile
        args.Pipeline.DrawBrepShaded(this.Value.SolidBrep, this.Value.m_profileColour);
        // draw subcomponents
        for (int i = 0; i < this.Value.m_subProfiles.Count; i++)
        {
          args.Pipeline.DrawBrepShaded(this.Value.m_subProfiles[i], this.Value.m_subColours[i]);
        }
        // draw rebars
        for (int i = 0; i < this.Value.m_rebars.Count; i++)
        {
          args.Pipeline.DrawBrepShaded(this.Value.m_rebars[i], this.Value.m_rebarColours[i]);
        }
      }
    }

    public void DrawViewportWires(GH_PreviewWireArgs args)
    {
      if (this.Value == null) { return; }

      Color defaultCol = Grasshopper.Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
      if (args.Color.R == defaultCol.R && args.Color.G == defaultCol.G && args.Color.B == defaultCol.B) // not selected
      {
        args.Pipeline.DrawPolyline(this.Value.m_profileEdge, UI.Colour.OasysBlue, 2);
        if (this.Value.m_profileVoidEdges != null)
        {
          foreach (Polyline crv in this.Value.m_profileVoidEdges)
          {
            args.Pipeline.DrawPolyline(crv, UI.Colour.OasysBlue, 1);
          }
        }
        if (this.Value.m_subEdges != null)
        {
          foreach (Polyline crv in this.Value.m_subEdges)
          {
            args.Pipeline.DrawPolyline(crv, UI.Colour.OasysBlue, 1);
          }
        }
        if (this.Value.m_subVoidEdges != null)
        {
          foreach (List<Polyline> crvs in this.Value.m_subVoidEdges)
          {
            foreach (Polyline crv in crvs)
            {
              args.Pipeline.DrawPolyline(crv, UI.Colour.OasysBlue, 1);
            }
          }
        }
        if (this.Value.m_rebarEdges != null)
        {
          foreach (Circle crv in this.Value.m_rebarEdges)
          {
            args.Pipeline.DrawCircle(crv, Color.Black, 1);
          }
        }
        if (this.Value.m_linkEdges != null)
        {
          foreach (Curve crv in this.Value.m_linkEdges)
          {
            args.Pipeline.DrawCurve(crv, Color.Black, 1);
          }
        }
      }
      else // selected
      {
        args.Pipeline.DrawPolyline(Value.m_profileEdge, UI.Colour.OasysYellow, 3);
        if (this.Value.m_profileVoidEdges != null)
        {
          foreach (Polyline crv in this.Value.m_profileVoidEdges)
          {
            args.Pipeline.DrawPolyline(crv, UI.Colour.OasysYellow, 2);
          }
        }
        if (this.Value.m_subEdges != null)
        {
          foreach (Polyline crv in this.Value.m_subEdges)
          {
            args.Pipeline.DrawPolyline(crv, UI.Colour.OasysYellow, 2);
          }
        }
        if (this.Value.m_subVoidEdges != null)
        {
          foreach (List<Polyline> crvs in this.Value.m_subVoidEdges)
          {
            foreach (Polyline crv in crvs)
            {
              args.Pipeline.DrawPolyline(crv, UI.Colour.OasysYellow, 2);
            }
          }
        }
        if (this.Value.m_rebarEdges != null)
        {
          foreach (Circle crv in this.Value.m_rebarEdges)
          {
            args.Pipeline.DrawCircle(crv, UI.Colour.UILightGrey, 2);
          }
        }
        if (this.Value.m_linkEdges != null)
        {
          foreach (Curve crv in this.Value.m_linkEdges)
          {
            args.Pipeline.DrawCurve(crv, UI.Colour.UILightGrey, 2);
          }
        }
      }
      // local axis
      if (this.Value.previewXaxis != null)
      {
        args.Pipeline.DrawLine(this.Value.previewZaxis, Color.FromArgb(255, 244, 96, 96), 1);
        args.Pipeline.DrawLine(this.Value.previewXaxis, Color.FromArgb(255, 96, 244, 96), 1);
        args.Pipeline.DrawLine(this.Value.previewYaxis, Color.FromArgb(255, 96, 96, 234), 1);
      }
    }
    #endregion
  }
}
