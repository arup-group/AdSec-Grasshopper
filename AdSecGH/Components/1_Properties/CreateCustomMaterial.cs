using System;
using System.Drawing;

using AdSecCore;
using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using GH_IO.Serialization;

using Grasshopper.Kernel;

using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.GH.Helpers;

using OasysGH;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateCustomMaterial : DropdownAdapter<CreateCustomMaterialFunction> {
    private bool isConcrete = true;
    private MaterialType _type = MaterialType.Concrete;

    public override Guid ComponentGuid => new Guid("29f87bee-c84c-5d11-9b30-492190df2910");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.CreateCustomMaterial;

    public CreateCustomMaterial() {
      BusinessComponent.OnVariableInputChanged += () => {
        RecordUndoEvent("Changed dropdown");
      };
    }

    public override bool Read(GH_IReader reader) {
      isConcrete = reader.GetBoolean("isConcrete");
      return base.Read(reader);
    }

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];

      Enum.TryParse(_selectedItems[0], out _type);

      isConcrete = _selectedItems[i] == MaterialType.Concrete.ToString();

      BusinessComponent.SetMaterialType(_type);

      base.UpdateUI();
    }

    public override bool Write(GH_IWriter writer) {
      writer.SetBoolean("isConcrete", isConcrete);
      return base.Write(writer);
    }

    protected override void UpdateUIFromSelectedItems() {
      Enum.TryParse(_selectedItems[0], out _type);
      CreateAttributes();

      BusinessComponent.SetMaterialType(_type);

      UpdateUI();
    }
  }
}
