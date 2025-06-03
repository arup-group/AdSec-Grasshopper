using AdSecCore.Parameters;

using AdSecGHCore.Constants;

namespace AdSecCore.Functions {
  public class CreateRebarSpacingFunction : Function {

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute() {
      Name = "Create Rebar Spacing",
      NickName = "Spacing",
      Description = "Create Rebar spacing (by Count or Pitch) for an AdSec Section",
    };
    public override Organisation Organisation { get; set; } = new Organisation() {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat3()
    };

    public RebarBundleParameter Rebar { get; set; } = Default.RebarBundle();

    public DoubleParameter Spacing { get; set; } = new DoubleParameter() {
      Name = "Spacing",
      NickName = "S",
      Description
        = "Number of bars is calculated based on the available length and the given bar pitch. The bar pitch is re-calculated to place the bars at equal spacing, with a maximum final pitch of the given value. Example: If the available length for the bars is 1000mm and the given bar pitch is 300mm, then the number of spacings that can fit in the available length is calculated as 1000 / 300 i.e. 3.333. The number of spacings is rounded up (3.333 rounds up to 4) and the bar pitch re-calculated (1000mm / 4), resulting in a final pitch of 250mm."
    };

    public IntegerParameter Count { get; set; } = new IntegerParameter() {
      Name = "Count",
      NickName = "N",
      Description
        = "The number of bundles or single bars. The bundles or single bars are spaced out evenly over the available space."
    };

    public RebarLayerParameter SpacedRebars { get; set; } = new RebarLayerParameter() {
      Name = "Spaced Rebars",
      NickName = "RbS",
      Description = "Rebars Spaced in a Layer for AdSec Reinforcement"
    };

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] { Rebar, Spacing };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] { SpacedRebars };
    }

    public override void Compute() { }
  }
}
