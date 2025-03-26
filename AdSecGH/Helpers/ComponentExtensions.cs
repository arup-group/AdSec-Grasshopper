using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;

namespace Oasys.GH.Helpers {
  public static class ComponentExtensions {

    public static IGH_Param GetInputParam(this GH_ComponentParamServer @params, string name) {
      int index = @params.IndexOfInputParam(name);
      return @params.Input[index];
    }

    public static IGH_Param GetOutputParam(this GH_ComponentParamServer @params, string name) {
      int index = @params.IndexOfOutputParam(name);
      return @params.Output[index];
    }

    public static IGH_Param GetInputParamAt(this GH_Component component, int index) {
      return component.Params.Input[index];
    }

    public static IGH_Param GetOutputParamAt(this GH_Component component, int index) {
      return component.Params.Output[index];
    }

    public static bool SetInputParamAt(this GH_Component component, int index, object value) {
      var ghParam = component.Params.Input[index];
      return ghParam.AddVolatileData(new GH_Path(0), 0, value);
    }

    public static void ClearInputs(this GH_Component component) {
      foreach (var param in component.Params.Input) {
        param.ClearData();
      }
    }

    public static T GetValue<T>(this IGH_Param param, int branch, int index) where T : class {
      return param.VolatileData.get_Branch(branch)[index] as T;
    }

    public static object GetValue(this IGH_Param param, int branch, int index) {
      return param.VolatileData.get_Branch(branch)[index];
    }

    [SuppressMessage("Major Code Smell", "S3011:Make sure that this accessibility bypass is safe here.",
      Justification = "This is for testing purposes")]
    public static bool MatchesExpectedIcon(this GH_Component component, Bitmap expected) {
      var propertyInfo = component.GetType().GetProperty("Icon", BindingFlags.Instance | BindingFlags.NonPublic);
      var icon = (Bitmap)propertyInfo?.GetValue(component, null);
      var expectedRawFormat = expected.RawFormat;
      return expectedRawFormat.Guid.Equals(icon?.RawFormat.Guid);
    }
  }
}
