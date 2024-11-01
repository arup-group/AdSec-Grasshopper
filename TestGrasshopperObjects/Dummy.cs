using System;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

namespace AdSecGHTests.Helpers {

  public class Dummy : BusinessGlue<DummyBusiness> {
    public override GH_Exposure Exposure { get; } = GH_Exposure.hidden;
    public override Guid ComponentGuid => new Guid("CAA08C9E-417C-42AE-B704-91F214C8C871");
  }
}
