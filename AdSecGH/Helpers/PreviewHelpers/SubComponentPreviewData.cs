using System.Collections.Generic;

using Rhino.Display;
using Rhino.Geometry;

namespace AdSecGH.Helpers.PreviewHelpers {
  internal sealed class SubComponentsPreviewData {
    public List<Brep> SubProfiles { get; set; } = new List<Brep>();
    public List<Polyline> SubEdges { get; set; } = new List<Polyline>();
    public List<List<Polyline>> SubVoidEdges { get; set; } = new List<List<Polyline>>();
    public List<DisplayMaterial> SubColours { get; set; } = new List<DisplayMaterial>();
  }
}
