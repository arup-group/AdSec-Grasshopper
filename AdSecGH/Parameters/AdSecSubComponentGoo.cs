using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.UI;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.Profiles;

using OasysGH.Units;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecSubComponentGoo : GH_GeometricGoo<ISubComponent>, IGH_PreviewData {
    public override BoundingBox Boundingbox {
      get {
        if (Value == null) {
          return BoundingBox.Empty;
        }

        return section.SolidBrep.GetBoundingBox(false);
      }
    }
    public BoundingBox ClippingBox => Boundingbox;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "SubComponent";
    internal AdSecSection section;
    private readonly IPoint offset;
    private readonly Plane plane;
    private readonly Line previewXaxis;
    private readonly Line previewYaxis;
    private readonly Line previewZaxis;

    public AdSecSubComponentGoo(SubComponent subComponent) : base(subComponent.ISubComponent) {
      offset = subComponent.ISubComponent.Offset;
      var sectionDesign = subComponent.SectionDesign;
      plane = sectionDesign.LocalPlane.ToGh();
      section = new AdSecSection(sectionDesign.Section, sectionDesign.DesignCode.IDesignCode,
        sectionDesign.MaterialName, sectionDesign.CodeName, plane, offset);
    }

    public AdSecSubComponentGoo(
      ISubComponent subComponent, Plane local, IDesignCode code, string codeName, string materialName) :
      base(subComponent) {
      offset = subComponent.Offset;
      section = new AdSecSection(subComponent.Section, code, codeName, materialName, local, offset);
      plane = local;
    }

    public AdSecSubComponentGoo(
      ISection section, Plane local, IPoint point, IDesignCode code, string codeName, string materialName) {
      m_value = ISubComponent.Create(section, point);
      offset = point;
      this.section = new AdSecSection(section, code, codeName, materialName, local, offset);
      plane = local;
      // local axis
      if (plane != null && plane != Plane.WorldXY && local != Plane.WorldYZ && local != Plane.WorldZX) {
        var area = this.section.Section.Profile.Area();
        double pythogoras = Math.Sqrt(area.As(AreaUnit.SquareMeter));
        var length = new Length(pythogoras * 0.15, LengthUnit.Meter);
        previewXaxis = new Line(local.Origin, local.XAxis, length.As(DefaultUnits.LengthUnitGeometry));
        previewYaxis = new Line(local.Origin, local.YAxis, length.As(DefaultUnits.LengthUnitGeometry));
        previewZaxis = new Line(local.Origin, local.ZAxis, length.As(DefaultUnits.LengthUnitGeometry));
      }
    }

    public override bool CastFrom(object source) {
      if (source == null) {
        return false;
      }

      return false;
    }

    public override bool CastTo<Q>(out Q target) {
      if (typeof(Q).IsAssignableFrom(typeof(AdSecSubComponentGoo))) {
        target = (Q)(object)Duplicate();
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(AdSecSectionGoo))) {
        target = (Q)(object)new AdSecSectionGoo(section.Duplicate());
        return true;
      }

      target = default;
      return false;
    }

    public void DrawViewportMeshes(GH_PreviewMeshArgs args) {
      //Draw shape.
      if (section.SolidBrep != null) {
        // draw profile
        args.Pipeline.DrawBrepShaded(section.SolidBrep, section._profileData.ProfileColour);
        // draw subcomponents
        var subComponentsPreviewData = section._subProfilesData;
        for (int i = 0; i < subComponentsPreviewData.SubProfiles.Count; i++) {
          args.Pipeline.DrawBrepShaded(subComponentsPreviewData.SubProfiles[i], subComponentsPreviewData.SubColours[i]);
        }

        // draw rebars
        var sectionReinforcementData = section._reinforcementData;
        for (int i = 0; i < sectionReinforcementData.Rebars.Count; i++) {
          args.Pipeline.DrawBrepShaded(sectionReinforcementData.Rebars[i], sectionReinforcementData.RebarColours[i]);
        }
      }
    }

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      if (section == null) {
        return;
      }

      var defaultCol = Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
      if (args.Color.R == defaultCol.R && args.Color.G == defaultCol.G && args.Color.B == defaultCol.B) // not selected
      {
        args.Pipeline.DrawPolyline(section._profileData.ProfileEdge, Colour.OasysBlue, 2);
        if (section._profileData.ProfileVoidEdges != null) {
          foreach (var crv in section._profileData.ProfileVoidEdges) {
            args.Pipeline.DrawPolyline(crv, Colour.OasysBlue, 1);
          }
        }

        if (section._subProfilesData.SubEdges != null) {
          foreach (var crv in section._subProfilesData.SubEdges) {
            args.Pipeline.DrawPolyline(crv, Colour.OasysBlue, 1);
          }
        }

        if (section._subProfilesData.SubVoidEdges != null) {
          foreach (var crvs in section._subProfilesData.SubVoidEdges) {
            foreach (var crv in crvs) {
              args.Pipeline.DrawPolyline(crv, Colour.OasysBlue, 1);
            }
          }
        }

        if (section._reinforcementData.RebarEdges != null) {
          foreach (var crv in section._reinforcementData.RebarEdges) {
            args.Pipeline.DrawCircle(crv, Color.Black, 1);
          }
        }
      } else {
        args.Pipeline.DrawPolyline(section._profileData.ProfileEdge, Colour.OasysYellow, 3);
        if (section._profileData.ProfileVoidEdges != null) {
          foreach (var crv in section._profileData.ProfileVoidEdges) {
            args.Pipeline.DrawPolyline(crv, Colour.OasysYellow, 2);
          }
        }

        if (section._subProfilesData.SubEdges != null) {
          foreach (var crv in section._subProfilesData.SubEdges) {
            args.Pipeline.DrawPolyline(crv, Colour.OasysYellow, 2);
          }
        }

        if (section._subProfilesData.SubVoidEdges != null) {
          foreach (var crvs in section._subProfilesData.SubVoidEdges) {
            foreach (var crv in crvs) {
              args.Pipeline.DrawPolyline(crv, Colour.OasysYellow, 2);
            }
          }
        }

        if (section._reinforcementData.RebarEdges != null) {
          foreach (var crv in section._reinforcementData.RebarEdges) {
            args.Pipeline.DrawCircle(crv, Colour.UILightGrey, 2);
          }
        }
      }

      // local axis
      if (!previewXaxis.IsValid) {
        return;
      }

      args.Pipeline.DrawLine(previewZaxis, Color.FromArgb(255, 244, 96, 96), 1);
      args.Pipeline.DrawLine(previewXaxis, Color.FromArgb(255, 96, 244, 96), 1);
      args.Pipeline.DrawLine(previewYaxis, Color.FromArgb(255, 96, 96, 234), 1);
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return new AdSecSubComponentGoo(Value, plane, section.DesignCode, section._codeName, section._materialName);
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      return section.SolidBrep.GetBoundingBox(xform);
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      return null;
    }

    public override string ToString() {
      return $"AdSec {TypeName} {{{section} Offset: {offset}}}";
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      return null;
    }
  }
}
