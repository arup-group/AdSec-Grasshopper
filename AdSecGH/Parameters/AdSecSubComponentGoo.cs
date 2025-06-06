using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecCore;
using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.UI;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.Profiles;

using Rhino.Display;
using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecSubComponentGoo : GH_GeometricGoo<ISubComponent>, IGH_PreviewData {
    public override BoundingBox Boundingbox
      => Value == null ? BoundingBox.Empty : section.SolidBrep.GetBoundingBox(false);
    public BoundingBox ClippingBox => Boundingbox;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "SubComponent";
    internal AdSecSection section;
    private readonly IPoint offset;
    private readonly Plane plane;
    public readonly Line previewXaxis;
    public readonly Line previewYaxis;
    public readonly Line previewZaxis;
    internal List<DrawInstructions> DrawInstructionsList { get; } = new List<DrawInstructions>();

    public AdSecSubComponentGoo(SubComponent subComponent) : base(subComponent.ISubComponent) {
      offset = subComponent.ISubComponent.Offset;
      var sectionDesign = subComponent.SectionDesign;
      plane = sectionDesign.LocalPlane.ToGh();
      section = new AdSecSection(sectionDesign.Section, sectionDesign.DesignCode.IDesignCode,
        sectionDesign.MaterialName, sectionDesign.CodeName, plane, offset);
    }

    public AdSecSubComponentGoo(
      ISubComponent subComponent, Plane local, IDesignCode code, string codeName, string materialName) :
      base(subComponent) {
      offset = subComponent.Offset;
      section = new AdSecSection(subComponent.Section, code, codeName, materialName, local, offset);
      plane = local;
    }

    public AdSecSubComponentGoo(
      ISection section, Plane local, IPoint point, IDesignCode code, string codeName, string materialName) {
      m_value = ISubComponent.Create(section, point);
      offset = point;
      this.section = new AdSecSection(section, code, codeName, materialName, local, offset);
      plane = local;

      if (!PlaneHelper.IsNotParallelToWorldXYZ(plane)) {
        return;
      }

      (previewXaxis, previewYaxis, previewZaxis) = AxisHelper.GetLocalAxisLines(section.Profile, local);
    }

    public override bool CastFrom(object source) {
      return false;
    }

    public override bool CastTo<Q>(out Q target) {
      if (typeof(Q).IsAssignableFrom(typeof(AdSecSubComponentGoo))) {
        target = (Q)(object)Duplicate();
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(AdSecSectionGoo))) {
        target = (Q)(object)new AdSecSectionGoo(section.Duplicate());
        return true;
      }

      target = default;
      return false;
    }

    public void DrawViewportMeshes(GH_PreviewMeshArgs args) {
      if (!TryPrepareDrawMeshesInstructions()) {
        return;
      }

      DrawAll(args.Pipeline);
    }

    internal bool TryPrepareDrawMeshesInstructions() {
      if (section?.SolidBrep == null) {
        return false;
      }

      DrawInstructionsList.Clear();
      AddBrepShaded(section.SolidBrep, section.ProfileData.ProfileColour);

      var subComponentsPreviewData = section.SubProfilesData;
      var sectionReinforcementData = section.ReinforcementData;

      AddBrepsShaded(subComponentsPreviewData.SubProfiles, subComponentsPreviewData.SubColours);
      AddBrepsShaded(sectionReinforcementData.Rebars, sectionReinforcementData.RebarColours);
      return true;
    }

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      if (!TryPrepareDrawWiresInstructions(args.Color)) {
        return;
      }

      DrawAll(args.Pipeline);
      if (!previewXaxis.IsValid) {
        return;
      }

      DrawingHelper.DrawLocalAxis(args, previewZaxis, previewXaxis, previewYaxis);
    }

    internal bool TryPrepareDrawWiresInstructions(Color color) {
      if (section == null || !section.IsValid) {
        return false;
      }

      DrawInstructionsList.Clear();

      var defaultCol = Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
      bool isSelected = !color.IsRgbEqualTo(defaultCol);
      var edgeColor = isSelected ? Colour.OasysYellow : Colour.OasysBlue;
      int mainEdgeWidth = isSelected ? 3 : 2;
      int voidEdgeWidth = isSelected ? 2 : 1;
      var rebarColor = isSelected ? Colour.UILightGrey : Color.Black;
      int rebarWidth = isSelected ? 2 : 1;

      DrawInstructionsList.Add(new DrawPolyline() { Color = edgeColor, Polyline = section.ProfileData.ProfileEdge, Thickness = mainEdgeWidth, });

      AddEdges(section.ProfileData.ProfileVoidEdges, edgeColor, voidEdgeWidth);
      AddEdges(section.SubProfilesData.SubEdges, edgeColor, voidEdgeWidth);
      AddNestedEdges(section.SubProfilesData.SubVoidEdges, edgeColor, voidEdgeWidth);
      AddCircles(section.ReinforcementData.RebarEdges, rebarColor, rebarWidth);

      return true;
    }

    internal void AddBrepShaded(Brep brep, DisplayMaterial displayMaterial) {
      DrawInstructionsList.Add(
        new DrawBrepShaded() { Brep = brep, DisplayMaterial = displayMaterial, Geometry = brep, });
    }

    internal void AddBrepsShaded(IList<Brep> breps, IList<DisplayMaterial> displayMaterials) {
      if (breps == null || displayMaterials == null) {
        return;
      }

      for (int i = 0; i < breps.Count; i++) {
        AddBrepShaded(breps[i], displayMaterials[i]);
      }
    }

    internal void AddEdges(IEnumerable<Polyline> edges, Color color, int width) {
      if (edges != null) {
        DrawInstructionsList.AddRange(edges.Select(item => new DrawPolyline() { Polyline = item, Color = color, Thickness = width, }));
      }
    }

    internal void AddNestedEdges(IEnumerable<IEnumerable<Polyline>> nestedEdges, Color color, int width) {
      if (nestedEdges == null) {
        return;
      }

      foreach (var edgeList in nestedEdges) {
        DrawInstructionsList.AddRange(edgeList.Select(item => new DrawPolyline() { Polyline = item, Color = color, Thickness = width, }));
      }
    }

    internal void AddCircles(IEnumerable<Circle> circles, Color color, int width) {
      if (circles == null) {
        return;
      }

      DrawInstructionsList.AddRange(circles.Select(item => new DrawCircle() { Circle = item, Color = color, Thickness = width, }));
    }

    private void DrawAll(DisplayPipeline pipeline) {
      foreach (var drawInstruction in
        DrawInstructionsList.Where(drawInstruction => drawInstruction?.Geometry != null)) {
        DrawingHelper.Draw(pipeline, drawInstruction);
      }
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return new AdSecSubComponentGoo(Value, plane, section.DesignCode, section._codeName, section._materialName);
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      return section.SolidBrep.GetBoundingBox(xform);
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      return null;
    }

    public override string ToString() {
      return $"AdSec {TypeName} {{{section} Offset: {offset}}}";
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      return null;
    }
  }
}
