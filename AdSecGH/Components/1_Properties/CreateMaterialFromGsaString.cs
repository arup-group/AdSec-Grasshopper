using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;

namespace AdSecGH.Components {

  public class FromGsaStringGh : FromGsaStringFunction {
    public FromGsaStringGh() { }
  }

  public class CreateMaterialFromGsaString : ComponentAdapter<FromGsaStringGh> {
    public override Guid ComponentGuid => new Guid("f3a9c2d1-8b5e-4f7a-b2c1-9d3e5a7b8f2c");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.StandardMaterial;
  }
}
