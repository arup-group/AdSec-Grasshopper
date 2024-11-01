using Grasshopper.Kernel;

namespace Oasys.GH.Helpers {
  public static class ComponentExtensions {

    public static IGH_Param GetInputParam(this GH_ComponentParamServer @params, string name) {
      int index = @params.IndexOfInputParam(name);
      return @params.Input[index];
    }

    public static IGH_Param GetOutputParam(this GH_ComponentParamServer @params, string name) {
      int index = @params.IndexOfOutputParam(name);
      return @params.Input[index];
    }

    public static IGH_Param GetParamAt(this GH_Component component, int index) {
      return component.Params.Input[index];
    }

    public static object GetValue(this IGH_Param param, int branch, int index) {
      return param.VolatileData.get_Branch(branch)[index];
    }
  }
}
