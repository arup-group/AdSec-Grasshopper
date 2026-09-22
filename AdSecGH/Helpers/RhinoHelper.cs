using System.Drawing;
using System.Linq;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

using Rhino.Geometry;

namespace AdSecGH.Helpers {
  public interface IGrasshopperDocumentContext {
    void AddObject(object obj, bool recordUndo = false);
  }

  public class GrasshopperDocumentContext : IGrasshopperDocumentContext {
    public void AddObject(object obj, bool recordUndo = false) {
      Instances.ActiveCanvas.Document.AddObject((IGH_DocumentObject)obj, recordUndo);
    }
  }

  public static class RhinoHelper {
    public static GH_Panel CreatePanel(IGH_Attributes attributes, string text) {
      var panel = new GH_Panel();
      panel.CreateAttributes();

      const int offset = 40;
      var attributesBounds = attributes.DocObject.Attributes.Bounds;
      var panelBounds = panel.Attributes.Bounds;

      panel.Attributes.Pivot = new PointF(attributesBounds.Left - panelBounds.Width - offset,
        attributesBounds.Bottom - panelBounds.Height);

      panel.UserText = text;

      return panel;
    }

    public static bool TryFitPlaneToPolyline(Polyline polyline, out Plane plane) {
      plane = Plane.Unset;
      var points = polyline.ToList();
      if (points[0].DistanceTo(points[points.Count - 1]) < 1e-6) {
        points.RemoveAt(points.Count - 1);
      }
      return Plane.FitPlaneToPoints(points, out plane) == PlaneFitResult.Success;
    }

    private static Transform GlobalToLocal(Plane localPlane, Plane globalPlane) {
      var maptToGlobal = Transform.PlaneToPlane(Plane.WorldYZ, globalPlane);
      if (localPlane.Origin == Point3d.Unset) {
        return maptToGlobal;
      }
      var globalToLocal = Transform.PlaneToPlane(globalPlane, localPlane);
      return globalToLocal * maptToGlobal;
    }

    public static void GlobalToLocal(this ref Point3d point, Plane localPlane, Plane globalPlane) {
      var transform = GlobalToLocal(localPlane, globalPlane);
      point.Transform(transform);
    }

    public static void GlobalToLocal(this ref Vector3d point, Plane localPlane, Plane globalPlane) {
      var transform = GlobalToLocal(localPlane, globalPlane);
      point.Transform(transform);
    }
  }
}
