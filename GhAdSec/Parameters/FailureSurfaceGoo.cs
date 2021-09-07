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
using UnitsNet;
using Oasys.AdSec.Mesh;

namespace GhAdSec.Parameters
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
        }
        public AdSecFailureSurfaceGoo(ILoadSurface loadsurface, Plane local)
        {
            this.m_value = MeshFromILoadSurface(loadsurface, local);
            m_loadsurface = loadsurface;
            m_plane = local;
        }
        
        private ILoadSurface m_loadsurface;
        private Plane m_plane;
        public ILoadSurface FailureSurface
        {
            get { return m_loadsurface; }
        }

        internal Mesh MeshFromILoadSurface(ILoadSurface loadsurface, Plane local)
        {
            Mesh outMesh = new Mesh();

            outMesh.Vertices.AddVertices(
                loadsurface.Vertices.Select(pt => new Point3d(
                    pt.X.As(GhAdSec.DocumentUnits.ForceUnit),
                    pt.ZZ.As(GhAdSec.DocumentUnits.MomentUnit),
                    pt.YY.As(GhAdSec.DocumentUnits.MomentUnit)
                    )));

            outMesh.Faces.AddFaces(
                loadsurface.Faces.Select(face => new MeshFace(
                    face.Vertex1, face.Vertex2, face.Vertex3)));
            
            // transform to local plane
            Rhino.Geometry.Transform mapFromLocal = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldYZ, local);

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
            args.Pipeline.DrawMeshWires(Value, GhAdSec.UI.Colour.GsaLightGrey, 1);
        }
        public void DrawViewportMeshes(GH_PreviewMeshArgs args) 
        {
            if (args.Material.Diffuse == System.Drawing.Color.FromArgb(255, 150, 0, 0)) // not selected
                args.Pipeline.DrawMeshShaded(Value, GhAdSec.UI.Colour.FailureNormal);
            else
                args.Pipeline.DrawMeshShaded(Value, GhAdSec.UI.Colour.FailureSelected);
        }
    }
}
