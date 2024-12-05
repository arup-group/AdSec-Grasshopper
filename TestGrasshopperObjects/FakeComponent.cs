using System;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;

namespace AdSecGHTests.Helpers {

  public class FakeComponent : DropdownAdapter<FakeBusiness> {
    public override GH_Exposure Exposure => GH_Exposure.hidden;
    public override Guid ComponentGuid => new Guid("caa08c9e-417c-42ae-b704-91f214c8c871");
    public override OasysPluginInfo PluginInfo { get; }
  }
}
