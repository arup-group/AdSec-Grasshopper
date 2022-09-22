using System;
using Grasshopper.Kernel.Types;
using Oasys.AdSec.Reinforcement;

namespace AdSecGH.Parameters
{
  public class AdSecRebarBundleGoo : GH_Goo<IBarBundle>
  {
    public AdSecRebarBundleGoo(IBarBundle bar)
    : base(bar)
    {
    }

    public override bool IsValid => true;

    public override string TypeName => "Rebar Bundle";

    public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

    public override IGH_Goo Duplicate()
    {
      return new AdSecRebarBundleGoo(this.Value);
    }
    public override string ToString()
    {
      string bar = "Rebar {";
      UnitsNet.Length thk1 = this.Value.Diameter.ToUnit(Units.LengthUnit);
      bar += "Ø" + thk1.ToString();
      if (this.Value.CountPerBundle > 1)
      {
        bar += ", Bundle (" + this.Value.CountPerBundle + ")";
      }
      //bar += ", " + this.Value.Material.ToString();
      return "AdSec " + bar + "}";
    }
  }
}
