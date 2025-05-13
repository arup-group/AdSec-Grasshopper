using System.Collections.Generic;

using Rhino.Display;
using Rhino.Geometry;

namespace AdSecGH.Helpers.PreviewHelpers {
  internal sealed class ReinforcementPreviewData {
    public List<Brep> Rebars { get; set; } = new List<Brep>();
    public List<Circle> RebarEdges { get; set; } = new List<Circle>();
    public List<Curve> LinkEdges { get; set; } = new List<Curve>();
    public List<DisplayMaterial> RebarColours { get; set; } = new List<DisplayMaterial>();
  }
}
