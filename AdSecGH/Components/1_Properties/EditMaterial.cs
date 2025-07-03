using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Parameters;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Components;

namespace AdSecGH.Components {
  public class EditMaterialGh : EditMaterialFunction {
    public EditMaterialGh() { }
  }

  public class EditMaterial : ComponentAdapter<EditMaterialGh> {
    public override Guid ComponentGuid => new Guid("87f26bee-c72c-4d88-9b30-492190df2910");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Properties.Resources.EditMaterial;

    public EditMaterial() {

    }
  }
}
