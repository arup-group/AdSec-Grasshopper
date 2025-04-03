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
        return _section.SolidBrep.GetBoundingBox(false);
      }
    }
    public BoundingBox ClippingBox => Boundingbox;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "SubComponent";
    internal AdSecSection _section;
    private IPoint _offset;
    private Plane _plane;
    private Line _previewXaxis;
    private Line _previewYaxis;
    private Line _previewZaxis;

    public AdSecSubComponentGoo(SubComponent subComponent) : base(subComponent.ISubComponent) {
      _offset = subComponent.ISubComponent.Offset;
      var sectionDesign = subComponent.SectionDesign;
      _plane = sectionDesign.LocalPlane.ToGh();
      _section = new AdSecSection(sectionDesign.Section, sectionDesign.DesignCode.IDesignCode, sectionDesign.MaterialName, sectionDesign.CodeName, _plane, _offset);
    }

    public AdSecSubComponentGoo(ISubComponent subComponent, Plane local, IDesignCode code, string codeName, string materialName) : base(subComponent) {
      _offset = subComponent.Offset;
      _section = new AdSecSection(subComponent.Section, code, codeName, materialName, local, _offset);
      _plane = local;
    }

    public AdSecSubComponentGoo(ISection section, Plane local, IPoint point, IDesignCode code, string codeName, string materialName) {
      m_value = ISubComponent.Create(section, point);
      _offset = point;
      _section = new AdSecSection(section, code, codeName, materialName, local, _offset);
      _plane = local;
      // local axis
      if (_plane != Plane.WorldXY && local != Plane.WorldYZ && local != Plane.WorldZX) {
        var area = _section.Section.Profile.Area();
        double pythogoras = Math.Sqrt(area.As(AreaUnit.SquareMeter));
        var length = new Length(pythogoras * 0.15, LengthUnit.Meter);
        _previewXaxis = new Line(local.Origin, local.XAxis, length.As(DefaultUnits.LengthUnitGeometry));
        _previewYaxis = new Line(local.Origin, local.YAxis, length.As(DefaultUnits.LengthUnitGeometry));
        _previewZaxis = new Line(local.Origin, local.ZAxis, length.As(DefaultUnits.LengthUnitGeometry));
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
        target = (Q)(object)new AdSecSectionGoo(_section.Duplicate());
        return true;
      }
      target = default;
      return false;
    }

    public void DrawViewportMeshes(GH_PreviewMeshArgs args) {
      //Draw shape.
      if (_section.SolidBrep != null) {
        // draw profile
        args.Pipeline.DrawBrepShaded(_section.SolidBrep, _section.m_profileColour);
        // draw subcomponents
        for (int i = 0; i < _section._subProfiles.Count; i++) {
          args.Pipeline.DrawBrepShaded(_section._subProfiles[i], _section.m_subColours[i]);
        }
        // draw rebars
        for (int i = 0; i < _section.m_rebars.Count; i++) {
          args.Pipeline.DrawBrepShaded(_section.m_rebars[i], _section.m_rebarColours[i]);
        }
      }
    }

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      if (_section == null) { return; }

      Color defaultCol = Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
      if (args.Color.R == defaultCol.R && args.Color.G == defaultCol.G && args.Color.B == defaultCol.B) // not selected
      {
        args.Pipeline.DrawPolyline(_section.m_profileEdge, Colour.OasysBlue, 2);
        if (_section.m_profileVoidEdges != null) {
          foreach (var crv in _section.m_profileVoidEdges) {
            args.Pipeline.DrawPolyline(crv, Colour.OasysBlue, 1);
          }
        }

        if (_section.m_subEdges != null) {
          foreach (var crv in _section.m_subEdges) {
            args.Pipeline.DrawPolyline(crv, Colour.OasysBlue, 1);
          }
        }

        if (_section.m_subVoidEdges != null) {
          foreach (var crvs in _section.m_subVoidEdges) {
            foreach (var crv in crvs) {
              args.Pipeline.DrawPolyline(crv, Colour.OasysBlue, 1);
            }
          }
        }

        if (_section.m_rebarEdges != null) {
          foreach (var crv in _section.m_rebarEdges) {
            args.Pipeline.DrawCircle(crv, Color.Black, 1);
          }
        }
      } else {
        args.Pipeline.DrawPolyline(_section.m_profileEdge, Colour.OasysYellow, 3);
        if (_section.m_profileVoidEdges != null) {
          foreach (var crv in _section.m_profileVoidEdges) {
            args.Pipeline.DrawPolyline(crv, Colour.OasysYellow, 2);
          }
        }

        if (_section.m_subEdges != null) {
          foreach (var crv in _section.m_subEdges) {
            args.Pipeline.DrawPolyline(crv, Colour.OasysYellow, 2);
          }
        }

        if (_section.m_subVoidEdges != null) {
          foreach (var crvs in _section.m_subVoidEdges) {
            foreach (var crv in crvs) {
              args.Pipeline.DrawPolyline(crv, Colour.OasysYellow, 2);
            }
          }
        }

        if (_section.m_rebarEdges != null) {
          foreach (var crv in _section.m_rebarEdges) {
            args.Pipeline.DrawCircle(crv, Colour.UILightGrey, 2);
          }
        }
      }

      // local axis
      args.Pipeline.DrawLine(_previewZaxis, Color.FromArgb(255, 244, 96, 96), 1);
      args.Pipeline.DrawLine(_previewXaxis, Color.FromArgb(255, 96, 244, 96), 1);
      args.Pipeline.DrawLine(_previewYaxis, Color.FromArgb(255, 96, 96, 234), 1);
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return new AdSecSubComponentGoo(Value, _plane, _section.DesignCode, _section._codeName, _section._materialName);
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      return _section.SolidBrep.GetBoundingBox(xform);
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      return null;
    }

    public override string ToString() {
      return $"AdSec {TypeName} {{{_section} Offset: {_offset}}}";
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      return null;
    }
  }
}
