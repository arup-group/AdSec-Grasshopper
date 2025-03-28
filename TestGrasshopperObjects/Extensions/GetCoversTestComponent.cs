using System;

using AdSecGH.Helpers;

using Grasshopper.Kernel;

using OasysGH;
using OasysGH.Components;

using OasysUnits.Units;

namespace TestGrasshopperObjects.Extensions {
  public class GetCoversTestComponent : GH_OasysComponent {
    public override Guid ComponentGuid => Guid.NewGuid();

    public override OasysPluginInfo PluginInfo => null;
    public GetCoversTestComponent() : base("t0", "t1", "t2", "t3", "t4") { }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("test Input", "i", "input", GH_ParamAccess.list);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("test Output", "o", "output", GH_ParamAccess.list);
    }

    protected override void SolveInstance(IGH_DataAccess da) {
      var result = this.GetCovers(da, 0, LengthUnit.Meter);
      da.SetData(0, result);
    }
  }
}
