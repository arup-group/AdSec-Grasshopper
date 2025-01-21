using System;

using AdSecGH.Helpers;

using Grasshopper.Kernel;

using OasysGH;
using OasysGH.Components;

namespace TestGrasshopperObjects.Extensions {
  public class AdSecStressStrainPointsTestComponent : GH_OasysComponent {
    public bool Optional = true;
    public AdSecStressStrainPointsTestComponent() : base("t0", "t1", "t2", "t3", "t4") { }
    public override Guid ComponentGuid => new Guid();

    public override OasysPluginInfo PluginInfo => null;

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("test Input", "i", "input", GH_ParamAccess.list);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("test Output", "o", "output", GH_ParamAccess.list);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      DA.SetData(0, this.GetStressStrainPoints(DA, 0, Optional));
    }
  }
}
