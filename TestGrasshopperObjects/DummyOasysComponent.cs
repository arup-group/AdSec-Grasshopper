using System;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

namespace AdSecGHTests.Helpers {
  public class DummyOasysComponent : BusinessOasysGlue<FakeBusiness> {

    public override GH_Exposure Exposure { get; } = GH_Exposure.hidden;

    public override Guid ComponentGuid => new Guid("CAA08C9E-417C-42AE-B734-91F214C8B87F");

    protected override void SolveInstance(IGH_DataAccess DA) { }
  }
}
