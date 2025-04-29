using System;
using System.Drawing;
using System.Linq;

using AdSecGH.UI;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec.Mesh;

using OasysGH.Units;

using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecFailureSurfaceGoo : GH_GeometricGoo<Mesh>, IGH_PreviewData {
    public override BoundingBox Boundingbox => Value?.GetBoundingBox(false) ?? BoundingBox.Empty;
    public BoundingBox ClippingBox => Boundingbox;
    public ILoadSurface FailureSurface { get; }
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "FailureSurface";
    internal Text3d _negMyy;
    internal Text3d _negMzz;
    internal Text3d _negN;
    internal Text3d _posMyy;
    internal Text3d _posMzz;
    internal Text3d _posN;
    private BoundingBox _bbox;
    private Plane _plane;
    private Line _previewNegXaxis;
    private Line _previewNegYaxis;
    private Line _previewNegZaxis;
    private Line _previewPosXaxis;
    private Line _previewPosYaxis;
    private Line _previewPosZaxis;

    public AdSecFailureSurfaceGoo(ILoadSurface loadsurface, Plane local, Mesh mesh) : base(mesh) {
      if (mesh == null) {
        m_value = MeshFromILoadSurface(loadsurface, local);
      }

      FailureSurface = loadsurface;
      _plane = local;
      UpdatePreview();
    }

    public AdSecFailureSurfaceGoo(ILoadSurface loadsurface, Plane local) {
      m_value = MeshFromILoadSurface(loadsurface, local);
      FailureSurface = loadsurface;
      _plane = local;
      UpdatePreview();
    }

    public override bool CastFrom(object source) {
      return false;
    }

    public override bool CastTo<Q>(out Q target) {
      if (typeof(Q).IsAssignableFrom(typeof(AdSecFailureSurfaceGoo))) {
        target = (Q)(object)new AdSecFailureSurfaceGoo(FailureSurface, _plane, Value);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(Mesh))) {
        target = (Q)(object)Value;
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Mesh))) {
        target = (Q)(object)new GH_Mesh(Value);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(ILoadSurface))) {
        target = (Q)(object)FailureSurface;
        return true;
      }

      target = default;
      return false;
    }

    public void DrawViewportMeshes(GH_PreviewMeshArgs args) {
      var defaultColor = Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
      if (args.Material.Diffuse.R == defaultColor.R && args.Material.Diffuse.G == defaultColor.G
        && args.Material.Diffuse.B == defaultColor.B) {
        // not selected
        args.Pipeline.DrawMeshShaded(Value, Colour.FailureNormal);
      } else {
        args.Pipeline.DrawMeshShaded(Value, Colour.FailureSelected);
      }
    }

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      if (!Value.IsValid) {
        return;
      }

      args.Pipeline.DrawMeshWires(Value, Colour.UILightGrey, 1);
      // local axis
      if (!_previewPosXaxis.IsValid) {
        return;
      }

      args.Pipeline.DrawArrow(_previewPosXaxis, Color.FromArgb(255, 244, 96, 96), 15, 5); //red
      args.Pipeline.DrawArrow(_previewPosYaxis, Color.FromArgb(255, 96, 244, 96), 15, 5); //green
      args.Pipeline.DrawArrow(_previewPosZaxis, Color.FromArgb(255, 96, 96, 234), 15, 5); //blue
      args.Pipeline.DrawArrow(_previewNegXaxis, Color.FromArgb(255, 244, 96, 96), 15, 5); //red
      args.Pipeline.DrawArrow(_previewNegYaxis, Color.FromArgb(255, 96, 244, 96), 15, 5); //green
      args.Pipeline.DrawArrow(_previewNegZaxis, Color.FromArgb(255, 96, 96, 234), 15, 5); //blue
      args.Pipeline.Draw3dText(_posN, Color.FromArgb(255, 244, 96, 96));
      args.Pipeline.Draw3dText(_negN, Color.FromArgb(255, 244, 96, 96));
      args.Pipeline.Draw3dText(_posMyy, Color.FromArgb(255, 96, 244, 96));
      args.Pipeline.Draw3dText(_negMyy, Color.FromArgb(255, 96, 244, 96));
      args.Pipeline.Draw3dText(_posMzz, Color.FromArgb(255, 96, 96, 234));
      args.Pipeline.Draw3dText(_negMzz, Color.FromArgb(255, 96, 96, 234));
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return !Value.IsValid ? null : (IGH_GeometricGoo)new AdSecFailureSurfaceGoo(FailureSurface, _plane);
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      return Value?.GetBoundingBox(xform) ?? BoundingBox.Empty;
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      if (Value == null && !Value.IsValid) {
        return null;
      }

      var mesh = Value.DuplicateMesh();
      xmorph.Morph(mesh);
      var local = new Plane(_plane);
      xmorph.Morph(ref local);
      return new AdSecFailureSurfaceGoo(FailureSurface, local, mesh);
    }

    public override string ToString() {
      var mesh = new GH_Mesh(Value);
      return $"AdSec {TypeName}{mesh}";
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      if (Value == null && !Value.IsValid) {
        return null;
      }

      var mesh = Value.DuplicateMesh();
      mesh.Transform(xform);
      var local = new Plane(_plane);
      local.Transform(xform);
      return new AdSecFailureSurfaceGoo(FailureSurface, local, mesh);
    }

    internal Mesh MeshFromILoadSurface(ILoadSurface loadsurface, Plane local) {
      var outMesh = new Mesh();

      outMesh.Vertices.AddVertices(loadsurface.Vertices.Select(load => new Point3d(load.X.As(DefaultUnits.ForceUnit),
        load.ZZ.As(DefaultUnits.MomentUnit), load.YY.As(DefaultUnits.MomentUnit))));

      outMesh.Faces.AddFaces(loadsurface.Faces.Select(face => new MeshFace(face.Vertex1, face.Vertex2, face.Vertex3)));

      // transform to local plane
      var mapFromLocal = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldYZ, local);

      _bbox = outMesh.GetBoundingBox(false);
      _bbox.Transform(Rhino.Geometry.Transform.Scale(new Point3d(0, 0, 0), 1.05));
      outMesh.Transform(mapFromLocal);

      return outMesh;
    }

    private void UpdatePreview() {
      // local axis
      if (!_plane.IsValid) {
        return;
      }

      double maxN = _bbox.PointAt(1, 0.5, 0.5).X;
      double minN = _bbox.PointAt(0, 0.5, 0.5).X;

      double maxMyy = _bbox.PointAt(0.5, 0.5, 1).Z;
      double minMyy = _bbox.PointAt(0.5, 0.5, 0).Z;

      double maxMzz = _bbox.PointAt(0.5, 1, 0.5).Y;
      double minMzz = _bbox.PointAt(0.5, 0, 0.5).Y;

      _previewPosXaxis = new Line(_plane.Origin, _plane.ZAxis, maxN);
      _previewPosYaxis = new Line(_plane.Origin, _plane.YAxis, maxMyy);
      _previewPosZaxis = new Line(_plane.Origin, _plane.XAxis, maxMzz);
      _previewNegXaxis = new Line(_plane.Origin, _plane.ZAxis, minN);
      _previewNegYaxis = new Line(_plane.Origin, _plane.YAxis, minMyy);
      _previewNegZaxis = new Line(_plane.Origin, _plane.XAxis, minMzz);

      double size = Math.Max(
        Math.Max(Math.Max(Math.Max(Math.Max(Math.Abs(maxN), Math.Abs(minN)), Math.Abs(maxMyy)), Math.Abs(minMyy)),
          Math.Abs(maxMzz)), Math.Abs(minMzz));
      size /= 50;
      var plnPosN = new Plane(_plane) {
        Origin = _previewPosXaxis.PointAt(1.05),
      };
      _posN = new Text3d("Tension", plnPosN, size) {
        HorizontalAlignment = TextHorizontalAlignment.Center,
        VerticalAlignment = TextVerticalAlignment.Bottom,
      };

      var plnNegN = new Plane(_plane) {
        Origin = _previewNegXaxis.PointAt(1.05),
      };
      _negN = new Text3d("Compression", plnNegN, size) {
        HorizontalAlignment = TextHorizontalAlignment.Center,
        VerticalAlignment = TextVerticalAlignment.Bottom,
      };

      var plnPosMyy = new Plane(_plane) {
        Origin = _previewPosYaxis.PointAt(1.05),
      };
      _posMyy = new Text3d("+Myy", plnPosMyy, size) {
        HorizontalAlignment = TextHorizontalAlignment.Center,
        VerticalAlignment = TextVerticalAlignment.Bottom,
      };

      var plnNegMyy = new Plane(_plane) {
        Origin = _previewNegYaxis.PointAt(1.05),
      };
      _negMyy = new Text3d("-Myy", plnNegMyy, size) {
        HorizontalAlignment = TextHorizontalAlignment.Center,
        VerticalAlignment = TextVerticalAlignment.Top,
      };

      var plnPosMzz = new Plane(_plane) {
        Origin = _previewPosZaxis.PointAt(1.05),
      };
      _posMzz = new Text3d("+Mzz", plnPosMzz, size) {
        HorizontalAlignment = TextHorizontalAlignment.Left,
        VerticalAlignment = TextVerticalAlignment.Middle,
      };

      var plnNegMzz = new Plane(_plane) {
        Origin = _previewNegZaxis.PointAt(1.05),
      };
      _negMzz = new Text3d("-Mzz", plnNegMzz, size) {
        HorizontalAlignment = TextHorizontalAlignment.Right,
        VerticalAlignment = TextVerticalAlignment.Middle,
      };
    }
  }
}
