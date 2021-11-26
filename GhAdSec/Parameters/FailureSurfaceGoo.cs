using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Oasys.AdSec.Mesh;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Drawing;

namespace AdSecGH.Parameters
{
    public class AdSecFailureSurfaceGoo : GH_GeometricGoo<Mesh>, IGH_PreviewData
    {
        public AdSecFailureSurfaceGoo(ILoadSurface loadsurface, Plane local, Mesh mesh = null)
        : base(mesh)
        {
            if (mesh == null)
                this.m_value = MeshFromILoadSurface(loadsurface, local);
            m_loadsurface = loadsurface;
            m_plane = local;
            UpdatePreview();
        }
        public AdSecFailureSurfaceGoo(ILoadSurface loadsurface, Plane local)
        {
            this.m_value = MeshFromILoadSurface(loadsurface, local);
            m_loadsurface = loadsurface;
            m_plane = local;
            UpdatePreview();
        }

        private ILoadSurface m_loadsurface;
        private Plane m_plane;
        private Line previewPosXaxis;
        private Line previewPosYaxis;
        private Line previewPosZaxis;
        private Line previewNegXaxis;
        private Line previewNegYaxis;
        private Line previewNegZaxis;
        internal Rhino.Display.Text3d posN;
        internal Rhino.Display.Text3d negN;
        internal Rhino.Display.Text3d posMyy;
        internal Rhino.Display.Text3d negMyy;
        internal Rhino.Display.Text3d posMzz;
        internal Rhino.Display.Text3d negMzz;
        private BoundingBox bbox;
        public ILoadSurface FailureSurface
        {
            get { return m_loadsurface; }
        }
        private void UpdatePreview()
        {
            // local axis
            if (m_plane != null)
            {
                Rhino.Geometry.Transform mapFromLocal = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldYZ, m_plane);

                double maxN = bbox.PointAt(1, 0.5, 0.5).X;
                double minN = bbox.PointAt(0, 0.5, 0.5).X;

                double maxMyy = bbox.PointAt(0.5, 0.5, 1).Z;
                double minMyy = bbox.PointAt(0.5, 0.5, 0).Z;

                double maxMzz = bbox.PointAt(0.5, 1, 0.5).Y;
                double minMzz = bbox.PointAt(0.5, 0, 0.5).Y;

                //UnitsNet.Length length = new UnitsNet.Length(pythogoras * 0.15, UnitsNet.Units.LengthUnit.Meter);
                previewPosXaxis = new Line(m_plane.Origin, m_plane.ZAxis, maxN);
                previewPosYaxis = new Line(m_plane.Origin, m_plane.YAxis, maxMyy);
                previewPosZaxis = new Line(m_plane.Origin, m_plane.XAxis, maxMzz);
                previewNegXaxis = new Line(m_plane.Origin, m_plane.ZAxis, minN);
                previewNegYaxis = new Line(m_plane.Origin, m_plane.YAxis, minMyy);
                previewNegZaxis = new Line(m_plane.Origin, m_plane.XAxis, minMzz);

                double size = Math.Max(Math.Max(Math.Max(Math.Max(Math.Max(Math.Abs(maxN), Math.Abs(minN)), Math.Abs(maxMyy)), Math.Abs(minMyy)), Math.Abs(maxMzz)), Math.Abs(minMzz));
                size = size / 50;
                Plane plnPosN = new Plane(m_plane);
                plnPosN.Origin = previewPosXaxis.PointAt(1.05);
                posN = new Rhino.Display.Text3d("Tension", plnPosN, size);
                posN.HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Center;
                posN.VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Bottom;

                Plane plnNegN = new Plane(m_plane);
                plnNegN.Origin = previewNegXaxis.PointAt(1.05);
                negN = new Rhino.Display.Text3d("Compression", plnNegN, size);
                negN.HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Center;
                negN.VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Bottom;

                Plane plnPosMyy = new Plane(m_plane);
                plnPosMyy.Origin = previewPosYaxis.PointAt(1.05);
                posMyy = new Rhino.Display.Text3d("+Myy", plnPosMyy, size);
                posMyy.HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Center;
                posMyy.VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Bottom;

                Plane plnNegMyy = new Plane(m_plane);
                plnNegMyy.Origin = previewNegYaxis.PointAt(1.05);
                negMyy = new Rhino.Display.Text3d("-Myy", plnNegMyy, size);
                negMyy.HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Center;
                negMyy.VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Top;

                Plane plnPosMzz = new Plane(m_plane);
                plnPosMzz.Origin = previewPosZaxis.PointAt(1.05);
                posMzz = new Rhino.Display.Text3d("+Mzz", plnPosMzz, size);
                posMzz.HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Left;
                posMzz.VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Middle;

                Plane plnNegMzz = new Plane(m_plane);
                plnNegMzz.Origin = previewNegZaxis.PointAt(1.05);
                negMzz = new Rhino.Display.Text3d("-Mzz", plnNegMzz, size);
                negMzz.HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Right;
                negMzz.VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Middle;
            }
        }
        internal Mesh MeshFromILoadSurface(ILoadSurface loadsurface, Plane local)
        {
            Mesh outMesh = new Mesh();

            outMesh.Vertices.AddVertices(
                loadsurface.Vertices.Select(pt => new Point3d(
                    pt.X.As(Units.ForceUnit),
                    pt.ZZ.As(Units.MomentUnit),
                    pt.YY.As(Units.MomentUnit)
                    )));

            outMesh.Faces.AddFaces(
                loadsurface.Faces.Select(face => new MeshFace(
                    face.Vertex1, face.Vertex2, face.Vertex3)));
            
            // transform to local plane
            Rhino.Geometry.Transform mapFromLocal = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldYZ, local);

            bbox = outMesh.GetBoundingBox(false);
            bbox.Transform(Rhino.Geometry.Transform.Scale(new Point3d(0, 0, 0), 1.05));
            //bbox.Transform(mapFromLocal);
            outMesh.Transform(mapFromLocal);

            return outMesh;
        }

        public override string ToString()
        {
            GH_Mesh mesh = new GH_Mesh(Value);
            return "AdSec " + TypeName + mesh.ToString();
        }
        public override string TypeName => "FailureSurface";

        public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return new AdSecFailureSurfaceGoo(this.FailureSurface, m_plane);
        }
        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null) { return BoundingBox.Empty; }
                return Value.GetBoundingBox(false);
            }
        }
        public override BoundingBox GetBoundingBox(Transform xform)
        {
            if (Value == null) { return BoundingBox.Empty; }
            return Value.GetBoundingBox(xform);
        }
        public override IGH_GeometricGoo Transform(Transform xform)
        {
            if (Value == null) { return null; }
            Mesh m = Value.DuplicateMesh();
            m.Transform(xform);
            Plane local = new Plane(m_plane);
            local.Transform(xform);
            return new AdSecFailureSurfaceGoo(this.FailureSurface, local, m);
        }
        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            if (Value == null) { return null; }
            Mesh m = Value.DuplicateMesh();
            xmorph.Morph(m);
            Plane local = new Plane(m_plane);
            xmorph.Morph(ref local);
            return new AdSecFailureSurfaceGoo(this.FailureSurface, local, m);
        }

        public override object ScriptVariable()
        {
            return Value;
        }
        public override bool CastTo<TQ>(out TQ target)
        {
            if (typeof(TQ).IsAssignableFrom(typeof(AdSecFailureSurfaceGoo)))
            {
                target = (TQ)(object)new AdSecFailureSurfaceGoo(this.FailureSurface, this.m_plane, this.Value);
                return true;
            }

            if (typeof(TQ).IsAssignableFrom(typeof(Mesh)))
            {
                target = (TQ)(object)Value;
                return true;
            }

            if (typeof(TQ).IsAssignableFrom(typeof(GH_Mesh)))
            {
                target = (TQ)(object)new GH_Mesh(Value);
                return true;
            }

            if (typeof(TQ).IsAssignableFrom(typeof(ILoadSurface)))
            {
                target = (TQ)(object)FailureSurface;
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
            if (!Value.IsValid) { return; }
            args.Pipeline.DrawMeshWires(Value, AdSecGH.UI.Colour.UILightGrey, 1);
            // local axis
            if (previewPosXaxis != null)
            {
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
        public void DrawViewportMeshes(GH_PreviewMeshArgs args) 
        {
            Color defaultCol = Grasshopper.Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
            if (args.Material.Diffuse.R == defaultCol.R && args.Material.Diffuse.G == defaultCol.G && args.Material.Diffuse.B == defaultCol.B) // not selected
                args.Pipeline.DrawMeshShaded(Value, AdSecGH.UI.Colour.FailureNormal);
            else
                args.Pipeline.DrawMeshShaded(Value, AdSecGH.UI.Colour.FailureSelected);
            
        }
    }
}
