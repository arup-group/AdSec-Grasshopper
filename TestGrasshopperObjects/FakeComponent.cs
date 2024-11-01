using System;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

namespace AdSecGHTests.Helpers {

  public class FakeComponent : BusinessOasysDropdownGlue<FakeBusiness> {
    public override GH_Exposure Exposure { get; } = GH_Exposure.hidden;
    public override Guid ComponentGuid => new Guid("caa08c9e-417c-42ae-b704-91f214c8c871");
  }
}
