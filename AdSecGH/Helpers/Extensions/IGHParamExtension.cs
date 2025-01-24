using Grasshopper.Kernel;

namespace AdSecGH.Helpers {
  public static class GHParamExtension {
    public static void ConvertToError(this IGH_Param owner, string tryCastTo) {
      owner.ConvertFromToError(string.Empty, tryCastTo);
    }

    public static void ConvertFromToError(this IGH_Param owner, string tryCastFrom, string tryCastTo) {
      string fromObjectString = string.IsNullOrEmpty(tryCastFrom) ? string.Empty : $"from {tryCastFrom} ";
      owner.AddRuntimeError($"Unable to convert {owner.NickName} {fromObjectString}to {tryCastTo}");
    }

    public static void FailedToCollectDataWarning(this IGH_Param owner) {
      owner.AddRuntimeWarning($"Input parameter {owner.NickName} failed to collect data!");
    }
  }
}
