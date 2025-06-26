using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Properties;

using GH_IO.Serialization;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;

namespace AdSecGH.Components {
  public class CreateCustomMaterial : DropdownAdapter<CreateCustomMaterialFunction> {
    private bool isConcrete = true;

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

      Enum.TryParse(_selectedItems[0], out MaterialType _type);

      isConcrete = _selectedItems[i] == MaterialType.Concrete.ToString();

      BusinessComponent.SetMaterialType(_type);

      base.UpdateUI();
    }

    public override bool Write(GH_IWriter writer) {
      writer.SetBoolean("isConcrete", isConcrete);
      return base.Write(writer);
    }

    protected override void UpdateUIFromSelectedItems() {
      Enum.TryParse(_selectedItems[0], out MaterialType _type);
      CreateAttributes();

      BusinessComponent.SetMaterialType(_type);

      UpdateUI();
    }
  }
}
