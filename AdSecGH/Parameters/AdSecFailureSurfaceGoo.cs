using System;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Oasys.AdSec.Mesh;
using OasysGH.Units;
using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecFailureSurfaceGoo : GH_GeometricGoo<Mesh>, IGH_PreviewData {
    public override BoundingBox Boundingbox {
      get {
        if (Value == null) {
          return BoundingBox.Empty;
        }
        return Value.GetBoundingBox(false);
      }
    }
    public BoundingBox ClippingBox => Boundingbox;
    public ILoadSurface FailureSurface { get; }
    public override string TypeDescription => "AdSec " + TypeName + " Parameter";
    public override string TypeName => "FailureSurface";
    internal Rhino.Display.Text3d negMyy;
    internal Rhino.Display.Text3d negMzz;
    internal Rhino.Display.Text3d negN;
    internal Rhino.Display.Text3d posMyy;
    internal Rhino.Display.Text3d posMzz;
    internal Rhino.Display.Text3d posN;
    private BoundingBox bbox;
    private Plane m_plane;
    private Line previewNegXaxis;
    private Line previewNegYaxis;
    private Line previewNegZaxis;
    private Line previewPosXaxis;
    private Line previewPosYaxis;
    private Line previewPosZaxis;

    public AdSecFailureSurfaceGoo(ILoadSurface loadsurface, Plane local, Mesh mesh = null) : base(mesh) {
      if (mesh == null) {
        m_value = MeshFromILoadSurface(loadsurface, local);
      }
      FailureSurface = loadsurface;
      m_plane = local;
      UpdatePreview();
    }

    public AdSecFailureSurfaceGoo(ILoadSurface loadsurface, Plane local) {
      m_value = MeshFromILoadSurface(loadsurface, local);
      FailureSurface = loadsurface;
      m_plane = local;
      UpdatePreview();
    }

    public override bool CastFrom(object source) {
      if (source == null) {
        return false;
      }

      return false;
    }

    public override bool CastTo<Q>(out Q target) {
      if (typeof(Q).IsAssignableFrom(typeof(AdSecFailureSurfaceGoo))) {
        target = (Q)(object)new AdSecFailureSurfaceGoo(FailureSurface, m_plane, Value);
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
      Color defaultCol = Grasshopper.Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
      if (args.Material.Diffuse.R == defaultCol.R && args.Material.Diffuse.G == defaultCol.G && args.Material.Diffuse.B == defaultCol.B) {
        // not selected
        args.Pipeline.DrawMeshShaded(Value, UI.Colour.FailureNormal);
      } else {
        args.Pipeline.DrawMeshShaded(Value, UI.Colour.FailureSelected);
      }
    }

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      if (!Value.IsValid) { return; }
      args.Pipeline.DrawMeshWires(Value, UI.Colour.UILightGrey, 1);
      // local axis
      if (previewPosXaxis != null) {
        args.Pipeline.DrawArrow(previewPosXaxis, Color.FromArgb(255, 244, 96, 96), 15, 5);//red
        args.Pipeline.DrawArrow(previewPosYaxis, Color.FromArgb(255, 96, 244, 96), 15, 5);//green
        args.Pipeline.DrawArrow(previewPosZaxis, Color.FromArgb(255, 96, 96, 234), 15, 5);//blue
        args.Pipeline.DrawArrow(previewNegXaxis, Color.FromArgb(255, 244, 96, 96), 15, 5);//red
        args.Pipeline.DrawArrow(previewNegYaxis, Color.FromArgb(255, 96, 244, 96), 15, 5);//green
        args.Pipeline.DrawArrow(previewNegZaxis, Color.FromArgb(255, 96, 96, 234), 15, 5);//blue
        args.Pipeline.Draw3dText(posN, Color.FromArgb(255, 244, 96, 96));
        args.Pipeline.Draw3dText(negN, Color.FromArgb(255, 244, 96, 96));
        args.Pipeline.Draw3dText(posMyy, Color.FromArgb(255, 96, 244, 96));
        args.Pipeline.Draw3dText(negMyy, Color.FromArgb(255, 96, 244, 96));
        args.Pipeline.Draw3dText(posMzz, Color.FromArgb(255, 96, 96, 234));
        args.Pipeline.Draw3dText(negMzz, Color.FromArgb(255, 96, 96, 234));
      }
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return new AdSecFailureSurfaceGoo(FailureSurface, m_plane);
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      if (Value == null) {
        return BoundingBox.Empty;
      }
      return Value.GetBoundingBox(xform);
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      if (Value == null) {
        return null;
      }
      Mesh m = Value.DuplicateMesh();
      xmorph.Morph(m);
      var local = new Plane(m_plane);
      xmorph.Morph(ref local);
      return new AdSecFailureSurfaceGoo(FailureSurface, local, m);
    }

    public override string ToString() {
      var mesh = new GH_Mesh(Value);
      return "AdSec " + TypeName + mesh.ToString();
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      if (Value == null) {
        return null;
      }
      Mesh m = Value.DuplicateMesh();
      m.Transform(xform);
      var local = new Plane(m_plane);
      local.Transform(xform);
      return new AdSecFailureSurfaceGoo(FailureSurface, local, m);
    }

    internal Mesh MeshFromILoadSurface(ILoadSurface loadsurface, Plane local) {
      var outMesh = new Mesh();

      outMesh.Vertices.AddVertices(
          loadsurface.Vertices.Select(pt => new Point3d(
              pt.X.As(DefaultUnits.ForceUnit),
              pt.ZZ.As(DefaultUnits.MomentUnit),
              pt.YY.As(DefaultUnits.MomentUnit)
              )));

      outMesh.Faces.AddFaces(
          loadsurface.Faces.Select(face => new MeshFace(
              face.Vertex1, face.Vertex2, face.Vertex3)));

      // transform to local plane
      var mapFromLocal = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldYZ, local);

      bbox = outMesh.GetBoundingBox(false);
      bbox.Transform(Rhino.Geometry.Transform.Scale(new Point3d(0, 0, 0), 1.05));
      //bbox.Transform(mapFromLocal);
      outMesh.Transform(mapFromLocal);

      return outMesh;
    }

    private void UpdatePreview() {
      // local axis
      if (m_plane != null) {
        var mapFromLocal = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldYZ, m_plane);

        double maxN = bbox.PointAt(1, 0.5, 0.5).X;
        double minN = bbox.PointAt(0, 0.5, 0.5).X;

        double maxMyy = bbox.PointAt(0.5, 0.5, 1).Z;
        double minMyy = bbox.PointAt(0.5, 0.5, 0).Z;

        double maxMzz = bbox.PointAt(0.5, 1, 0.5).Y;
        double minMzz = bbox.PointAt(0.5, 0, 0.5).Y;

        //Length length = new Length(pythogoras * 0.15, LengthUnit.Meter);
        previewPosXaxis = new Line(m_plane.Origin, m_plane.ZAxis, maxN);
        previewPosYaxis = new Line(m_plane.Origin, m_plane.YAxis, maxMyy);
        previewPosZaxis = new Line(m_plane.Origin, m_plane.XAxis, maxMzz);
        previewNegXaxis = new Line(m_plane.Origin, m_plane.ZAxis, minN);
        previewNegYaxis = new Line(m_plane.Origin, m_plane.YAxis, minMyy);
        previewNegZaxis = new Line(m_plane.Origin, m_plane.XAxis, minMzz);

        double size = Math.Max(Math.Max(Math.Max(Math.Max(Math.Max(Math.Abs(maxN), Math.Abs(minN)), Math.Abs(maxMyy)), Math.Abs(minMyy)), Math.Abs(maxMzz)), Math.Abs(minMzz));
        size /= 50;
        var plnPosN = new Plane(m_plane) {
          Origin = previewPosXaxis.PointAt(1.05)
        };
        posN = new Rhino.Display.Text3d("Tension", plnPosN, size) {
          HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Center,
          VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Bottom
        };

        var plnNegN = new Plane(m_plane) {
          Origin = previewNegXaxis.PointAt(1.05)
        };
        negN = new Rhino.Display.Text3d("Compression", plnNegN, size) {
          HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Center,
          VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Bottom
        };

        var plnPosMyy = new Plane(m_plane) {
          Origin = previewPosYaxis.PointAt(1.05)
        };
        posMyy = new Rhino.Display.Text3d("+Myy", plnPosMyy, size) {
          HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Center,
          VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Bottom
        };

        var plnNegMyy = new Plane(m_plane) {
          Origin = previewNegYaxis.PointAt(1.05)
        };
        negMyy = new Rhino.Display.Text3d("-Myy", plnNegMyy, size) {
          HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Center,
          VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Top
        };

        var plnPosMzz = new Plane(m_plane) {
          Origin = previewPosZaxis.PointAt(1.05)
        };
        posMzz = new Rhino.Display.Text3d("+Mzz", plnPosMzz, size) {
          HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Left,
          VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Middle
        };

        var plnNegMzz = new Plane(m_plane) {
          Origin = previewNegZaxis.PointAt(1.05)
        };
        negMzz = new Rhino.Display.Text3d("-Mzz", plnNegMzz, size) {
          HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Right,
          VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Middle
        };
      }
    }
  }
}
