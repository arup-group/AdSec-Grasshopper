using Grasshopper.Kernel.Types;

using Oasys.AdSec.Reinforcement;

using OasysGH.Units;

using OasysUnits;

namespace AdSecGH.Parameters {
  public class AdSecRebarBundleGoo : GH_Goo<IBarBundle> {
    public override bool IsValid => true;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "Rebar Bundle";

    public AdSecRebarBundleGoo(IBarBundle bar) : base(bar) {
    }

    public override IGH_Goo Duplicate() {
      return new AdSecRebarBundleGoo(Value);
    }

    public override string ToString() {
      string bar = "Rebar {";
      Length thk1 = Value.Diameter.ToUnit(DefaultUnits.LengthUnitGeometry);
      bar += $"Ø{thk1}";
      if (Value.CountPerBundle > 1) {
        bar += $", Bundle ({Value.CountPerBundle})";
      }
      //bar += ", " + Value.Material.ToString();
      return $"AdSec {bar}}}";
    }
  }
}
