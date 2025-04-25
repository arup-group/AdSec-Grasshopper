using AdSecCore;

using Grasshopper.Kernel.Types;

using Oasys.Profiles;

using OasysGH.Units;

namespace AdSecGH.Parameters {
  public class AdSecProfileWebGoo : GH_Goo<IWeb> {
    public override bool IsValid => true;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "Web Profile";

    public AdSecProfileWebGoo(IWeb web) : base(web) {
    }

    public override IGH_Goo Duplicate() {
      return new AdSecProfileWebGoo(Value);
    }

    public override string ToString() {
      string web = "AdSec Web {";
      var comparer = new DoubleComparer();
      if (comparer.Equals(Value.BottomThickness.Value, Value.TopThickness.Value)) {
        var thickness = Value.BottomThickness.ToUnit(DefaultUnits.LengthUnitGeometry);
        web += $"Constant {thickness}}}";
      } else {
        var topThickness = Value.TopThickness.ToUnit(DefaultUnits.LengthUnitGeometry);
        var bottomThickness = Value.BottomThickness.ToUnit(DefaultUnits.LengthUnitGeometry);
        web += $"Tapered: Top:{topThickness}, Bottom:{bottomThickness}}}";
      }
      return web;
    }
  }
}
