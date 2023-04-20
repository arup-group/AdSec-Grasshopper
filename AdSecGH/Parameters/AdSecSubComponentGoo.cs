using System;
using System.Collections.Generic;
using System.Drawing;
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
    public override string TypeDescription => "AdSec " + TypeName + " Parameter";
    public override string TypeName => "SubComponent";
    internal AdSecSection _section;
    private IPoint _offset;
    private Plane _plane;
    private Line _previewXaxis;
    private Line _previewYaxis;
    private Line _previewZaxis;

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
      if (_plane != null) {
        if (_plane != Plane.WorldXY & local != Plane.WorldYZ & local != Plane.WorldZX) {
          Area area = _section.Section.Profile.Area();
          double pythogoras = Math.Sqrt(area.As(AreaUnit.SquareMeter));
          var length = new Length(pythogoras * 0.15, LengthUnit.Meter);
          _previewXaxis = new Line(local.Origin, local.XAxis, length.As(DefaultUnits.LengthUnitGeometry));
          _previewYaxis = new Line(local.Origin, local.YAxis, length.As(DefaultUnits.LengthUnitGeometry));
          _previewZaxis = new Line(local.Origin, local.ZAxis, length.As(DefaultUnits.LengthUnitGeometry));
        }
      }
    }

    public override bool CastFrom(object source) {
      if (source == null)
        return false;
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
      target = default(Q);
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
        args.Pipeline.DrawPolyline(_section.m_profileEdge, UI.Colour.OasysBlue, 2);
        if (_section.m_profileVoidEdges != null) {
          foreach (Polyline crv in _section.m_profileVoidEdges) {
            args.Pipeline.DrawPolyline(crv, UI.Colour.OasysBlue, 1);
          }
        }
        if (_section.m_subEdges != null) {
          foreach (Polyline crv in _section.m_subEdges) {
            args.Pipeline.DrawPolyline(crv, UI.Colour.OasysBlue, 1);
          }
        }
        if (_section.m_subVoidEdges != null) {
          foreach (List<Polyline> crvs in _section.m_subVoidEdges) {
            foreach (Polyline crv in crvs) {
              args.Pipeline.DrawPolyline(crv, UI.Colour.OasysBlue, 1);
            }
          }
        }
        if (_section.m_rebarEdges != null) {
          foreach (Circle crv in _section.m_rebarEdges) {
            args.Pipeline.DrawCircle(crv, Color.Black, 1);
          }
        }
      } else // selected
        {
        args.Pipeline.DrawPolyline(_section.m_profileEdge, UI.Colour.OasysYellow, 3);
        if (_section.m_profileVoidEdges != null) {
          foreach (Polyline crv in _section.m_profileVoidEdges) {
            args.Pipeline.DrawPolyline(crv, UI.Colour.OasysYellow, 2);
          }
        }
        if (_section.m_subEdges != null) {
          foreach (Polyline crv in _section.m_subEdges) {
            args.Pipeline.DrawPolyline(crv, UI.Colour.OasysYellow, 2);
          }
        }
        if (_section.m_subVoidEdges != null) {
          foreach (List<Polyline> crvs in _section.m_subVoidEdges) {
            foreach (Polyline crv in crvs) {
              args.Pipeline.DrawPolyline(crv, UI.Colour.OasysYellow, 2);
            }
          }
        }
        if (_section.m_rebarEdges != null) {
          foreach (Circle crv in _section.m_rebarEdges) {
            args.Pipeline.DrawCircle(crv, UI.Colour.UILightGrey, 2);
          }
        }
      }

      // local axis
      if (_previewXaxis != null) {
        args.Pipeline.DrawLine(_previewZaxis, Color.FromArgb(255, 244, 96, 96), 1);
        args.Pipeline.DrawLine(_previewXaxis, Color.FromArgb(255, 96, 244, 96), 1);
        args.Pipeline.DrawLine(_previewYaxis, Color.FromArgb(255, 96, 96, 234), 1);
      }
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
      return "AdSec " + TypeName + " {" + _section.ToString() + " Offset: " + _offset.ToString() + "}";
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      return null;
    }
  }
}
