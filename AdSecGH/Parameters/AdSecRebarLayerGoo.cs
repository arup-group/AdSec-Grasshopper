using Grasshopper.Kernel.Types;
using Oasys.AdSec.Reinforcement.Layers;
using OasysGH.Units;
using OasysUnits;
using System;

namespace AdSecGH.Parameters {
  public class AdSecRebarLayerGoo : GH_Goo<ILayer> {
    public override bool IsValid => true;
    public override string TypeDescription => "AdSec " + TypeName + " Parameter";
    public override string TypeName => "Rebar Spacing";

    public AdSecRebarLayerGoo(ILayer layer) : base(layer) {
    }

    public override IGH_Goo Duplicate() {
      return new AdSecRebarLayerGoo(Value);
    }

    public override string ToString() {
      string bar = "";
      Length dia = Value.BarBundle.Diameter.ToUnit(DefaultUnits.LengthUnitGeometry);
      bar += "Ø" + dia.ToString();
      if (Value.BarBundle.CountPerBundle > 1) {
        bar += ", Bundle (" + Value.BarBundle.CountPerBundle + ")";
      }

      string str = "";
      try {
        ILayerByBarCount byBarCount = (ILayerByBarCount)Value;
        str = byBarCount.Count.ToString() + "No. " + bar;
      }
      catch (Exception) {
        try {
          ILayerByBarPitch byBarPitch = (ILayerByBarPitch)Value;
          Length spacing = byBarPitch.Pitch.ToUnit(DefaultUnits.LengthUnitGeometry);
          str = bar + " bars / " + spacing.ToString();
        }
        catch (Exception) {
        }
      }
      return "AdSec " + TypeName + " {" + str + "}";
    }
  }
}
