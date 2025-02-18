using System;

using AdSecGH.Helpers;

using Grasshopper.Kernel;

using OasysGH;
using OasysGH.Components;

namespace TestGrasshopperObjects.Extensions {
  public class AdSecProfileGooTestComponent : GH_OasysComponent {
    public bool Optional { get; set; }
    public AdSecProfileGooTestComponent() : base("t0", "t1", "t2", "t3", "t4") { }
    public override Guid ComponentGuid => Guid.NewGuid();

    public override OasysPluginInfo PluginInfo => null;

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("test Input", "i", "input", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("test Output", "o", "output", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      var result = this.GetAdSecProfileGoo(DA, 0, Optional);
      DA.SetData(0, result);
    }

  }
}
