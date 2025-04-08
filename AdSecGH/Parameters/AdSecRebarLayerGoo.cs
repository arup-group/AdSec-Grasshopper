using Grasshopper.Kernel.Types;

using Oasys.AdSec.Reinforcement.Layers;

using OasysGH.Units;

namespace AdSecGH.Parameters {
  public class AdSecRebarLayerGoo : GH_Goo<ILayer> {
    public override bool IsValid => true;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "Rebar Spacing";

    public AdSecRebarLayerGoo(ILayer layer) : base(layer) {
    }

    public override IGH_Goo Duplicate() {
      return new AdSecRebarLayerGoo(Value);
    }

    public override string ToString() {
      string bar = "";
      var diameter = Value.BarBundle.Diameter.ToUnit(DefaultUnits.LengthUnitGeometry);
      bar += $"Ø{diameter}";
      if (Value.BarBundle.CountPerBundle > 1) {
        bar += $", Bundle ({Value.BarBundle.CountPerBundle})";
      }

      string text = string.Empty;
      switch (Value) {
        case ILayerByBarCount byBarCount:
          text = $"{byBarCount.Count}No. {bar}";
          break;
        case ILayerByBarPitch byBarPitch: {
            var spacing = byBarPitch.Pitch.ToUnit(DefaultUnits.LengthUnitGeometry);
            text = $"{bar} bars / {spacing}";
            break;
          }
      }

      return $"AdSec {TypeName} {{{text}}}";
    }
  }
}
