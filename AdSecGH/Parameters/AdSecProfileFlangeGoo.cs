using System;
using Grasshopper.Kernel.Types;
using Oasys.Profiles;
using OasysUnits;

namespace AdSecGH.Parameters
{
  public class AdSecProfileFlangeGoo : GH_Goo<IFlange>
  {
    public AdSecProfileFlangeGoo(IFlange flange)
    : base(flange)
    {
    }

    public override bool IsValid => true;

    public override string TypeName => "Flange Profile";

    public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

    public override IGH_Goo Duplicate()
    {
      return new AdSecProfileFlangeGoo(this.Value);
    }
    public override string ToString()
    {
      string flange = "AdSec Flange {";
      Length thk1 = this.Value.Width.ToUnit(Units.LengthUnit);
      Length thk2 = this.Value.Thickness.ToUnit(Units.LengthUnit);
      flange += "Width:" + thk1.ToString() + ", Thk:" + thk2.ToString() + "}";
      return flange;
    }
  }
}
