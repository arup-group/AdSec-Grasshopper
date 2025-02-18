using Grasshopper.Kernel;

namespace AdSecGH.Helpers {
  public static class IghActiveObjectExtensions {
    public static void AddRuntimeWarning(this IGH_ActiveObject component, string message) {
      component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
    }

    public static void AddRuntimeError(this IGH_ActiveObject component, string message) {
      component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, message);
    }

    public static void AddRuntimeRemark(this IGH_ActiveObject component, string message) {
      component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, message);
    }
  }
}
