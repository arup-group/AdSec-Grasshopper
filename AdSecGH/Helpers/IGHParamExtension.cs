using Grasshopper.Kernel;

namespace AdSecGH.Helpers {
  public static class GHParamExtension {
    public static void ConvertError(this IGH_Param owner, string triedCastTo) {
      owner.AddRuntimeError($"Unable to convert {owner.NickName} to {triedCastTo}");
    }

    public static void FailedToCollectDataWarning(this IGH_Param owner) {
      owner.AddRuntimeWarning("Input parameter " + owner.NickName + " failed to collect data!");
    }
  }
}
