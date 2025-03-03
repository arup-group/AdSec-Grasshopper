using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;

using Attribute = AdSecCore.Functions.Attribute;
namespace AdSecGH.Components {
  public class FindCrackLoadGh : FindCrackLoadFunction {
    public FindCrackLoadGh() {
      var solution = AdSecSolution as Attribute;
      Solution.Update(ref solution);
      AdSecSolution.OnValueChanged += goo => {
        Solution.Value = goo.Value;
      };

      var baseLoad = AdSecBaseLoad as Attribute;
      BaseLoad.Update(ref baseLoad);
      AdSecBaseLoad.OnValueChanged += goo => {
        BaseLoad.Value = goo.Value;
      };


      var adSecSectionLoad = AdSecSectionLoad as Attribute;
      SectionLoad.Update(ref adSecSectionLoad);
      SectionLoad.OnValueChanged += goo => {
        AdSecSectionLoad.Value = new AdSecLoadGoo(goo);
      };

      var adSecMaximumCracking = AdSecMaximumCracking as Attribute;
      MaximumCracking.Update(ref adSecMaximumCracking);
      MaximumCracking.OnValueChanged += goo => {
        AdSecMaximumCracking.Value = new AdSecCrackGoo(goo);
      };
    }
    public AdSecSolutionParameter AdSecSolution { get; set; } = new AdSecSolutionParameter();
    public AdSecLoadParameter AdSecBaseLoad { get; set; } = new AdSecLoadParameter();
    public AdSecLoadParameter AdSecSectionLoad { get; set; } = new AdSecLoadParameter();
    public AdSecCrackParameter AdSecMaximumCracking { get; set; } = new AdSecCrackParameter();
    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
       AdSecSolution,
       AdSecBaseLoad,
       OptimisedLoad,
       LoadIncrement,
       MaximumCrack,
      };
    }
    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        AdSecSectionLoad,
        AdSecMaximumCracking,
      };
    }
  };


  public class FindCrackLoad : ComponentAdapter<FindCrackLoadGh> {
    public FindCrackLoad() { Hidden = true; }
    public override Guid ComponentGuid => new Guid("f0b27be7-f367-4a2c-b90c-3ba0f66ae584");
    public override GH_Exposure Exposure => GH_Exposure.quarternary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.CrackLoad;

  }
}
