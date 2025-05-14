using System.Collections.Generic;

using Rhino.Display;
using Rhino.Geometry;

namespace AdSecGH.Helpers.PreviewHelpers {
  internal sealed class ProfilePreviewData {
    public Brep Profile { get; set; }
    public Polyline ProfileEdge { get; set; }
    public List<Polyline> ProfileVoidEdges { get; set; }
    public DisplayMaterial ProfileColour { get; set; }
  }
}
