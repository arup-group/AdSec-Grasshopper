using System;
using OasysGH;

namespace AdSecGH {
  internal sealed class PluginInfo {
    public static OasysPluginInfo Instance => lazy.Value;
    private static readonly Lazy<OasysPluginInfo> lazy = new Lazy<OasysPluginInfo>(()
      => new OasysPluginInfo(AdSecGHInfo.ProductName, AdSecGHInfo.PluginName, AdSecGHInfo.Vers,
        AdSecGHInfo.isBeta, "phc_alOp3OccDM3D18xJTWDoW44Y1cJvbEScm5LJSX8qnhs"));

    private PluginInfo() { }
  }
}
