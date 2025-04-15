using AdSecCore.Functions;

using Grasshopper.Kernel.Types;

using Oasys.AdSec.Reinforcement;

using OasysGH.Units;

using OasysUnits;

namespace AdSecGH.Parameters {
  public class AdSecRebarBundleGoo : GH_Goo<BarBundle> {
    public override bool IsValid => true;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "Rebar Bundle";

    public AdSecRebarBundleGoo(BarBundle bar) : base(bar) {
    }

    public AdSecRebarBundleGoo(IBarBundle bar, string codeDescription) {
      Value = new BarBundle {
        Bundle = bar,
        CodeDescription = codeDescription,
      };
    }

    public override IGH_Goo Duplicate() {
      return new AdSecRebarBundleGoo(Value);
    }

    public override string ToString() {
      string bar = "Rebar {";
      Length thk1 = Value.Bundle.Diameter.ToUnit(DefaultUnits.LengthUnitGeometry);
      bar += $"Ø{thk1}";
      if (Value.Bundle.CountPerBundle > 1) {
        bar += $", Bundle ({Value.Bundle.CountPerBundle})";
      }
      return $"AdSec {bar}}}";
    }
  }
}
