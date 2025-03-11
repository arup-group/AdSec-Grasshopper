using System;

using AdSecCore.Functions;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using Attribute = AdSecCore.Functions.Attribute;

namespace AdSecGHTests.Helpers {
  public class ParameterMissing : ParameterAttribute<string> { }

  public class FunctionWithNoPreviouslyDefinedParameters : IFunction {

    public ParameterMissing parameter = new ParameterMissing() { Name = "One" };
    public StringParameter stringParameter = new StringParameter() { Name = "Two" };

    public FuncAttribute Metadata { get; set; } = new FuncAttribute();
    public Organisation Organisation { get; set; } = new Organisation();
    public Attribute[] GetAllInputAttributes() { return new Attribute[] { stringParameter }; }

    public Attribute[] GetAllOutputAttributes() { return new Attribute[] { parameter }; }
    public void Compute() { }
  }

  public class
    ComponentWithNoPreviouslyDefinedParameters : ComponentAdapter<FunctionWithNoPreviouslyDefinedParameters> {
    public override GH_Exposure Exposure { get; } = GH_Exposure.hidden;
    public override Guid ComponentGuid => new Guid("CBB08C9E-417C-42AE-B734-91F214C8B87F");
  }
}
