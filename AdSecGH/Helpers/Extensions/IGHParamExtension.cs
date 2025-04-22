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

    private static void UpdateInput(
      this IGH_Param owner, string name, string nickname, string description, GH_ParamAccess access,
      bool optional = false) {
      owner.Name = name;
      owner.NickName = nickname;
      owner.Description = description;
      owner.Access = access;
      owner.Optional = optional;
    }

    public static void UpdateItemInput(
      this IGH_Param owner, string name, string nickname, string description, bool optional = false) {
      owner.UpdateInput(name, nickname, description, GH_ParamAccess.item, optional);
    }

    public static void UpdateListInput(
      this IGH_Param owner, string name, string nickname, string description, bool optional = false) {
      owner.UpdateInput(name, nickname, description, GH_ParamAccess.list, optional);
    }
  }
}
