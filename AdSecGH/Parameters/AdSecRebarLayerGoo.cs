using AdSecCore.Functions;

using Grasshopper.Kernel.Types;

using Oasys.AdSec.Reinforcement.Layers;

using OasysGH.Units;

using OasysUnits;

namespace AdSecGH.Parameters {
  public class AdSecRebarLayerGoo : GH_Goo<BarLayer> {
    public override bool IsValid => true;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "Rebar Spacing";

    public AdSecRebarLayerGoo(ILayer layer, string codeDescription) {
      Value = new BarLayer {
        Layer = layer,
        CodeDescription = codeDescription,
      };
    }

    public override IGH_Goo Duplicate() {
      return new AdSecRebarLayerGoo(Value.Layer, Value.CodeDescription);
    }

    public override string ToString() {
      string bar = string.Empty;
      Length dia = Value.Layer.BarBundle.Diameter.ToUnit(DefaultUnits.LengthUnitGeometry);
      bar += $"Ø{dia}";
      if (Value.Layer.BarBundle.CountPerBundle > 1) {
        bar += $", Bundle ({Value.Layer.BarBundle.CountPerBundle})";
      }
      string layerInfo = string.Empty;
      switch (Value.Layer) {
        case ILayerByBarCount layerByCount:
          layerInfo = $"{layerByCount.Count}No. {bar}";
          break;
        default:
          var byBarPitch = (ILayerByBarPitch)Value.Layer;
          Length spacing = byBarPitch.Pitch.ToUnit(DefaultUnits.LengthUnitGeometry);
          layerInfo = $"{bar} bars / {spacing}";
          break;
      }
      return $"AdSec {TypeName} {{{layerInfo}}}";
    }
  }
}
