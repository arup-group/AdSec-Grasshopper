using Grasshopper.Kernel.Types;

using Oasys.Profiles;

using OasysGH.Units;

using OasysUnits;

namespace AdSecGH.Parameters {
  public class AdSecProfileFlangeGoo : GH_Goo<IFlange> {
    public override bool IsValid => true;
    public override string TypeDescription => "AdSec " + TypeName + " Parameter";
    public override string TypeName => "Flange Profile";

    public AdSecProfileFlangeGoo(IFlange flange) : base(flange) {
    }

    public override IGH_Goo Duplicate() {
      return new AdSecProfileFlangeGoo(Value);
    }

    public override string ToString() {
      string flange = "AdSec Flange {";
      Length thk1 = Value.Width.ToUnit(DefaultUnits.LengthUnitGeometry);
      Length thk2 = Value.Thickness.ToUnit(DefaultUnits.LengthUnitGeometry);
      flange += "Width:" + thk1.ToString() + ", Thk:" + thk2.ToString() + "}";
      return flange;
    }
  }
}
